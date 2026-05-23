# UniTask — Production Deployment Guide

Topology:
- **Frontend** (`unitask`, React + Vite) → Vercel
- **Backend**  (`unitask-API`, .NET 8)    → AWS EC2 (Docker)
- **Database** (SQL Server schema)        → Azure SQL Database

```
Browser ──HTTPS──► Vercel ──(rewrite /api/*)──► EC2:80 (Docker) ──TLS 1433──► Azure SQL
```

---

## 0. Rotate the leaked secrets FIRST

These values were previously committed to `appsettings*.json` and must be considered compromised:

| Secret | Where to rotate |
|---|---|
| Azure SQL password (`uni1234@`) | Azure Portal → SQL servers → `unitask` → *Reset password*. Use **>= 16 chars** with mixed case + symbols. |
| Groq API key (`gsk_...`) | https://console.groq.com → API Keys → revoke old, create new. |
| Qdrant API key (`eyJhbGc...`) | Qdrant Cloud → Cluster → Access → revoke old, create new. |
| JWT signing secret | Generate fresh: `openssl rand -base64 48`. Old tokens become invalid (users re-login). |

If the repo is/was public, also purge history (after backing up):
```bash
git filter-repo --path src/Unitask.Api/appsettings.json --invert-paths
git filter-repo --path src/Unitask.Api/appsettings.Production.json --invert-paths
git filter-repo --path src/Unitask.Api/appsettings.Development.example.json --invert-paths
# then force-push (coordinate with team first)
```

---

## 1. Azure SQL — prepare the database

### 1a. Run schema + seed
From a machine that can reach the Azure SQL server (your laptop with the firewall rule below, or Azure Cloud Shell):

```powershell
# Replace with your actual server / password
$server = "unitask.database.windows.net"
$user   = "unitask"
$pass   = "<NEW_STRONG_PASSWORD>"

sqlcmd -S $server -U $user -P $pass -d master  -i database/schema-sqlserver.sql
sqlcmd -S $server -U $user -P $pass -d UnitaskExe -i database/seed-sqlserver.sql
```

> The schema file starts with `USE UnitaskExe;` — create that database in Azure Portal first if it doesn't exist (SKU **Basic** ~$5/mo or **Serverless General Purpose** for auto-pause).

### 1b. Firewall — allow EC2 to reach Azure SQL
Azure Portal → SQL server `unitask` → **Networking** → **Public access**:

1. Allow your **EC2 public IP** (or Elastic IP — see §2a).
   - Rule name: `aws-ec2-prod`
   - Start IP = End IP = `<EC2_PUBLIC_IP>`
2. Also add your **laptop IP** while you're running migrations.
3. **Save**.

> Do **not** check "Allow Azure services" unless you really want every Azure tenant in the world able to connect.

---

## 2. AWS EC2 — host the backend

### 2a. (Strongly recommended) Allocate an Elastic IP
EC2 instances change public IP on stop/start. Bind an **Elastic IP**:
1. EC2 console → **Elastic IPs** → Allocate.
2. Associate it with your instance.
3. Use this IP in the Azure SQL firewall rule above.

### 2b. Security Group — open the right ports

| Port | Source | Why |
|---|---|---|
| 22 | your IP / VPN | SSH |
| 80 | `0.0.0.0/0`, `::/0` | HTTP from Vercel rewrite (and browsers) |
| 443 | `0.0.0.0/0`, `::/0` | when you add HTTPS later (Caddy/Nginx) |

### 2c. Install Docker on the instance
SSH in (`ssh ec2-user@<ip>` for AL2023, `ubuntu@<ip>` for Ubuntu) then:

**Amazon Linux 2023:**
```bash
sudo dnf update -y
sudo dnf install -y docker git
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
# log out and back in so the group takes effect
```

**Ubuntu 22.04 / 24.04:**
```bash
sudo apt update && sudo apt install -y docker.io docker-compose-plugin git
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
```

Verify: `docker --version && docker compose version`.

### 2d. Deploy the backend
```bash
# clone the repo on the instance
git clone <YOUR_REPO_URL> unitask-api
cd unitask-api

# create the env file (DO NOT commit this)
cp .env.example .env
nano .env   # paste real Azure SQL conn string, JWT secret, Groq/Qdrant keys

# build and run
docker compose up -d --build

# verify
docker compose ps
docker compose logs -f api
curl http://localhost/health    # -> {"status":"healthy"}
curl http://<EC2_PUBLIC_IP>/health  # from your laptop
```

### 2e. Updates after code changes
```bash
git pull
docker compose up -d --build
docker image prune -f
```

---

## 3. Vercel — point the frontend at the new backend

### 3a. Update `vercel.json`
Edit `unitask/vercel.json` and replace `YOUR_EC2_PUBLIC_DNS` with your EC2 public DNS (e.g. `ec2-3-1-2-3.ap-southeast-2.compute.amazonaws.com`) or Elastic IP:

```json
{
  "rewrites": [
    { "source": "/api/(.*)", "destination": "http://ec2-3-1-2-3.ap-southeast-2.compute.amazonaws.com/api/$1" },
    { "source": "/health",  "destination": "http://ec2-3-1-2-3.ap-southeast-2.compute.amazonaws.com/health" },
    { "source": "/(.*)",    "destination": "/index.html" }
  ]
}
```

Commit + push → Vercel auto-deploys.

### 3b. Vercel env vars
Vercel project → **Settings → Environment Variables**:
- `VITE_API_URL` = *(leave empty)* — keeps calls same-origin and routes through the rewrite above.

> Why empty: it avoids CORS preflights, hides the backend URL from the browser, and means HTTPS in the browser even though the backend is HTTP (Vercel's edge speaks TLS to the user, then HTTP to your EC2).

### 3c. (Optional) Add your Vercel domain to the backend CORS allowlist
Already covered by the wildcard in `Program.cs` (`*.vercel.app` and `*.vercel.com`). For a custom domain, add to `.env` on EC2:
```bash
Cors__AllowedOrigins__0=https://www.unitask.vn
```
then `docker compose up -d`.

---

## 4. Smoke test the full chain

From any browser:
```
https://<your-app>.vercel.app/health     -> {"status":"healthy"}
https://<your-app>.vercel.app/api/jobs   -> JSON list of jobs (from Azure SQL)
```

If `/health` works but `/api/jobs` returns 500, check:
```bash
docker compose logs api | tail -100
```
Most common cause: Azure SQL firewall hasn't whitelisted the EC2 IP yet, or the connection string in `.env` is wrong.

---

## 5. Hardening checklist (do later, not blocking)

- [ ] Put Caddy or Nginx in front for HTTPS on the backend (own domain → Let's Encrypt).
- [ ] Move `.env` values into AWS SSM Parameter Store / Secrets Manager.
- [ ] Use a least-privilege SQL user instead of `unitask` (the DBA-level account).
- [ ] CloudWatch Logs agent on the EC2 host for centralized logs.
- [ ] Enable Azure SQL **auditing** + **threat detection**.
- [ ] Snapshot the EBS volume on a schedule.

---

## 6. Local development (unchanged)

Backend:
```powershell
cd unitask-API\src\Unitask.Api
cp appsettings.Development.example.json appsettings.Development.json
# fill in real local values, then:
dotnet run
```

Frontend:
```powershell
cd unitask
npm install
npm run dev    # http://localhost:5173, proxies /api -> http://localhost:5076
```
