<#
  send-invitation.ps1 — Gửi email mời doanh nghiệp trải nghiệm UniTask.

  Đọc cấu hình Gmail (Username / App Password) từ appsettings.Development.json,
  render template 17-business-invitation.html và gửi qua SMTP.

  CÁCH CHẠY (mở PowerShell trong thư mục unitask-API):

    # Gửi cho 1 doanh nghiệp:
    ./scripts/send-invitation.ps1 -To "tnetcompany@gmail.com"

    # Gửi cho nhiều doanh nghiệp (cách nhau bởi dấu phẩy):
    ./scripts/send-invitation.ps1 -To "a@gmail.com","b@gmail.com","c@gmail.com"

    # Tùy chỉnh tên người gửi / link đăng ký:
    ./scripts/send-invitation.ps1 -To "a@gmail.com" -SenderName "Nguyễn Duy Lương" -RegisterUrl "https://unitask.io.vn/register"

  Nếu bị lỗi "không chạy được script", chạy 1 lần lệnh sau rồi thử lại:
    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#>

param(
  [Parameter(Mandatory = $true)]
  [string[]] $To,

  [string] $Subject = "Mời doanh nghiệp trải nghiệm miễn phí UniTask – nền tảng kết nối sinh viên & doanh nghiệp",
  [string] $SenderName = "Nhóm phát triển UniTask",
  [string] $RegisterUrl = "https://unitask.io.vn/register",
  [int]    $DelaySeconds = 5   # giãn nhịp giữa các email để tránh bị đánh dấu spam
)

# --- Đường dẫn (tính theo vị trí file script) ---
$root = Split-Path $PSScriptRoot -Parent
$cfgPath = Join-Path $root "src\Unitask.Api\appsettings.Development.json"
$tplPath = Join-Path $root "src\Unitask.Infrastructure\EmailTemplates\17-business-invitation.html"

if (-not (Test-Path $cfgPath)) { Write-Error "Không tìm thấy $cfgPath"; exit 1 }
if (-not (Test-Path $tplPath)) { Write-Error "Không tìm thấy $tplPath"; exit 1 }

# --- Đọc cấu hình email ---
$cfg = (Get-Content $cfgPath -Raw | ConvertFrom-Json).EmailSettings
$smtpUser = $cfg.Username
$smtpPass = $cfg.Password.Replace(' ', '')
$fromEmail = if ($cfg.FromEmail) { $cfg.FromEmail } else { $smtpUser }

if ([string]::IsNullOrWhiteSpace($smtpUser) -or [string]::IsNullOrWhiteSpace($smtpPass)) {
  Write-Error "Chưa cấu hình Username/Password trong appsettings.Development.json"; exit 1
}

# --- Đọc + render template ---
$tplRaw = Get-Content $tplPath -Raw

$utf8 = [System.Text.Encoding]::UTF8
$ok = 0; $fail = 0
$i = 0

foreach ($addr in $To) {
  $i++
  $body = $tplRaw.Replace("{{registerUrl}}", $RegisterUrl).Replace("{{senderName}}", $SenderName)

  $msg = New-Object System.Net.Mail.MailMessage
  $msg.From = New-Object System.Net.Mail.MailAddress($fromEmail, "UniTask Team", $utf8)
  $msg.To.Add($addr.Trim())
  $msg.Subject = $Subject
  $msg.SubjectEncoding = $utf8
  $msg.Body = $body
  $msg.BodyEncoding = $utf8
  $msg.IsBodyHtml = $true

  $smtp = New-Object System.Net.Mail.SmtpClient($cfg.Host, [int]$cfg.Port)
  $smtp.EnableSsl = $true
  $smtp.Credentials = New-Object System.Net.NetworkCredential($smtpUser, $smtpPass)

  try {
    $smtp.Send($msg)
    Write-Host ("[{0}/{1}] OK  -> {2}" -f $i, $To.Count, $addr) -ForegroundColor Green
    $ok++
  }
  catch {
    Write-Host ("[{0}/{1}] LỖI -> {2} : {3}" -f $i, $To.Count, $addr, $_.Exception.Message) -ForegroundColor Red
    $fail++
  }
  finally {
    $msg.Dispose(); $smtp.Dispose()
  }

  # Giãn nhịp giữa các email (trừ email cuối)
  if ($i -lt $To.Count -and $DelaySeconds -gt 0) { Start-Sleep -Seconds $DelaySeconds }
}

Write-Host ""
Write-Host ("Hoàn tất: {0} thành công, {1} lỗi." -f $ok, $fail) -ForegroundColor Cyan
