// Chuyển seed T-SQL (SQL Server) -> PostgreSQL. Xuất database/seed-postgres.sql.
const fs = require('fs');
const path = require('path');

const dir = __dirname;
const boolMap = JSON.parse(fs.readFileSync(path.join(dir, '.bool-map.json'), 'utf8'));
const files = ['seed-sqlserver.sql', 'seed-extra.sql', 'seed-jobs-extra.sql'];

let text = files.map(f => fs.readFileSync(path.join(dir, f), 'utf8')).join('\n');

// 1) Bỏ comment dòng, các câu lệnh điều khiển T-SQL
text = text.replace(/--[^\n]*/g, '');
text = text.replace(/^\s*(GO|BEGIN\s+TRANSACTION|COMMIT(\s+TRANSACTION)?|SET\s+NOCOUNT[^\n;]*|USE\s+[^\n;]*)\s*;?\s*$/gim, '');
text = text.replace(/PRINT\s+'(?:[^']|'')*'\s*;?/gi, '');

// 2) DATEADD(DAY, n, GETUTCDATE()) -> (now() + interval 'n days')  (mẫu cụ thể trong seed)
text = text.replace(/DATEADD\(\s*DAY\s*,\s*(-?\d+)\s*,\s*GETUTCDATE\(\)\s*\)/gi,
  (_, n) => `(now() + interval '${n} days')`);

// 3) Hàm thời gian / id
text = text.replace(/GETUTCDATE\(\)|SYSUTCDATETIME\(\)|GETDATE\(\)/gi, 'now()');
text = text.replace(/NEWID\(\)/gi, 'gen_random_uuid()');

// 3b) CAST(expr AS TYPE) -> (expr)::type  (non-greedy, đủ cho CAST(now() AS DATE))
text = text.replace(/CAST\(\s*(.+?)\s+AS\s+(\w+)\s*\)/gi, (_, e, t) => `(${e})::${t.toLowerCase()}`);

// 3c) DATEADD(unit, n, expr) -> (expr + interval 'n unit')  — trích 3 tham số theo ngoặc cân bằng
function convertDateadd(s) {
  let out = ''; let i = 0;
  while (i < s.length) {
    const idx = s.toUpperCase().indexOf('DATEADD(', i);
    if (idx < 0) { out += s.slice(i); break; }
    out += s.slice(i, idx);
    // tìm ngoặc đóng cân bằng
    let depth = 0, j = idx + 7, inStr = false, argStart = idx + 8;
    for (; j < s.length; j++) {
      const c = s[j];
      if (inStr) { if (c === "'") { if (s[j + 1] === "'") j++; else inStr = false; } }
      else if (c === "'") inStr = true;
      else if (c === '(') depth++;
      else if (c === ')') { depth--; if (depth === 0) break; }
    }
    const inner = s.slice(argStart, j);
    const parts = splitTop(inner);
    const unit = parts[0].trim().toLowerCase();
    const n = parts[1].trim();
    const expr = parts[2].trim();
    out += `(${expr} + interval '${n} ${unit}')`;
    i = j + 1;
  }
  return out;
}
text = convertDateadd(text);

// 4) DECLARE @var TYPE = expr;  -> lưu map, xóa dòng
const vars = {};
text = text.replace(/DECLARE\s+(@\w+)\s+[A-Za-z0-9()]+(?:\s*\([^)]*\))?\s*=\s*([^;]+);/gi,
  (_, name, val) => { vars[name] = val.trim(); return ''; });
// Thay thế biến (dài trước ngắn để tránh trùng tiền tố)
for (const name of Object.keys(vars).sort((a, b) => b.length - a.length)) {
  text = text.split(name).join(vars[name]);
}

// 5) [dbo].[X] và [Ident] -> "X"
text = text.replace(/\[dbo\]\.\[(\w+)\]/g, '"$1"');
text = text.replace(/\bdbo\.(\w+)/g, '"$1"');
text = text.replace(/\[(\w+)\]/g, '"$1"');

// 6) N'...' -> '...'  (chỉ khi N là TIỀN TỐ nvarchar: đứng sau ký tự không phải chữ/số,
//    tránh ăn nhầm chữ N cuối từ như trong "TechNova VN'")
text = text.replace(/(^|[^A-Za-z0-9_])N'/g, "$1'");

// 6b) "<Col>" LIKE '...'  -> "<Col>"::text LIKE '...'  (Postgres không LIKE trực tiếp trên uuid)
text = text.replace(/("\w+")\s+LIKE\s+'/gi, "$1::text LIKE '");

// ---- Tách statement theo ';' tôn trọng chuỗi ----
function splitStatements(s) {
  const out = []; let cur = ''; let inStr = false;
  for (let i = 0; i < s.length; i++) {
    const c = s[i];
    if (inStr) {
      cur += c;
      if (c === "'") { if (s[i + 1] === "'") { cur += s[++i]; } else { inStr = false; } }
    } else {
      if (c === "'") { inStr = true; cur += c; }
      else if (c === ';') { out.push(cur); cur = ''; }
      else cur += c;
    }
  }
  if (cur.trim()) out.push(cur);
  return out;
}

// Tách các phần tử top-level trong "( ... )" theo dấu phẩy, tôn trọng chuỗi + ngoặc lồng
function splitTop(s) {
  const out = []; let cur = ''; let depth = 0; let inStr = false;
  for (let i = 0; i < s.length; i++) {
    const c = s[i];
    if (inStr) {
      cur += c;
      if (c === "'") { if (s[i + 1] === "'") { cur += s[++i]; } else { inStr = false; } }
    } else {
      if (c === "'") { inStr = true; cur += c; }
      else if (c === '(') { depth++; cur += c; }
      else if (c === ')') { depth--; cur += c; }
      else if (c === ',' && depth === 0) { out.push(cur); cur = ''; }
      else cur += c;
    }
  }
  if (cur.trim() !== '') out.push(cur);
  return out;
}

// Tách danh sách tuple "(...),(...),..." thành từng "(...)"
function splitTuples(s) {
  const out = []; let depth = 0; let inStr = false; let start = -1;
  for (let i = 0; i < s.length; i++) {
    const c = s[i];
    if (inStr) { if (c === "'") { if (s[i + 1] === "'") i++; else inStr = false; } }
    else if (c === "'") inStr = true;
    else if (c === '(') { if (depth === 0) start = i; depth++; }
    else if (c === ')') { depth--; if (depth === 0) out.push(s.slice(start, i + 1)); }
  }
  return out;
}

// Bỏ khối điều khiển T-SQL: IF NOT EXISTS (<đk>) ... BEGIN ... END (đã có ON CONFLICT lo trùng)
function stripControlBlocks(s) {
  // xóa "IF NOT EXISTS (" + nội dung tới ngoặc đóng cân bằng (nhận biết chuỗi)
  let out = ''; let i = 0;
  const re = /IF\s+NOT\s+EXISTS\s*\(/gi;
  let m;
  while ((m = re.exec(s)) !== null) {
    out += s.slice(i, m.index);
    let depth = 1, j = m.index + m[0].length, inStr = false;
    for (; j < s.length; j++) {
      const c = s[j];
      if (inStr) { if (c === "'") { if (s[j + 1] === "'") j++; else inStr = false; } }
      else if (c === "'") inStr = true;
      else if (c === '(') depth++;
      else if (c === ')') { depth--; if (depth === 0) break; }
    }
    i = j + 1;            // bỏ luôn dấu ')' đóng điều kiện
    re.lastIndex = i;
  }
  out += s.slice(i);
  // PRINT '...'  (sau khi N' đã thành '), ELSE, và BEGIN/END đứng độc lập
  out = out.replace(/PRINT\s+'(?:[^']|'')*'\s*;?/gi, '');
  out = out.replace(/(^|\s)ELSE(?=\s|$)/gi, ' ');
  out = out.replace(/(^|\s)(BEGIN|END)(?=\s|$)/gi, ' ');
  return out;
}
text = stripControlBlocks(text);

const statements = splitStatements(text);
const outStatements = [];

for (let stmt of statements) {
  const trimmed = stmt.trim();
  if (!trimmed) continue;

  const m = trimmed.match(/^INSERT\s+INTO\s+"(\w+)"\s*\(([^)]*)\)\s*VALUES\s*([\s\S]*)$/i);
  if (!m) { outStatements.push(trimmed); continue; }

  const table = m[1];
  const cols = m[2].split(',').map(c => c.trim().replace(/^"|"$/g, ''));
  const boolCols = new Set(boolMap[table] || []);
  const boolIdx = cols.map((c, i) => boolCols.has(c) ? i : -1).filter(i => i >= 0);

  const tuples = splitTuples(m[3]);
  const newTuples = tuples.map(t => {
    const inner = t.slice(1, -1);
    const vals = splitTop(inner);
    for (const bi of boolIdx) {
      const v = (vals[bi] || '').trim();
      if (v === '1') vals[bi] = 'true';
      else if (v === '0') vals[bi] = 'false';
    }
    return '(' + vals.map(v => v.trim()).join(', ') + ')';
  });

  const colList = cols.map(c => `"${c}"`).join(', ');
  outStatements.push(
    `INSERT INTO "${table}" (${colList}) VALUES\n` +
    newTuples.join(',\n') +
    `\nON CONFLICT ("Id") DO NOTHING`
  );
}

const header = `-- Seed PostgreSQL (chuyển tự động từ seed T-SQL). Idempotent qua ON CONFLICT.\n` +
  `-- KHÔNG sửa tay; chạy convert-seed.cjs để tạo lại.\n\n`;
const result = header + outStatements.map(s => s.trim() + ';').join('\n\n') + '\n';
fs.writeFileSync(path.join(dir, 'seed-postgres.sql'), result);
console.log('wrote seed-postgres.sql —', outStatements.length, 'statements');
console.log('DECLARE vars resolved:', Object.keys(vars).length);
