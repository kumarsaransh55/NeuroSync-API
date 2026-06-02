# NeuroSync API (Backend)

The backend for **NeuroSync** — an inclusive work companion for employees with
ADHD and dyslexia. This API uses **Google Gemini** to (1) break messy tasks into
ordered micro-steps with time estimates, and (2) summarise documents/emails into
plain language, action items, deadlines, tone, highlights, hidden tasks, and a
**dyslexia-friendly rewrite**.

> The **React frontend** lives in a separate repo and calls this API. Product
> strategy, pitch and demo materials live in the frontend's `/docs`.

---

## Tech stack
- **ASP.NET Core 8** (C#)
- **Entity Framework Core 8** + **SQL Server** (Azure SQL in prod, LocalDB in dev)
- **Google Gemini** via `Google.GenAI` (`gemini-3-flash-preview`)
- **JWT** auth (`Microsoft.AspNetCore.Authentication.JwtBearer`) + **BCrypt** password hashing
- **Swagger** (enabled in Development)

---

## API endpoints

Base path: `/api`

### Auth (public)
| Method | Path | Body | Returns |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | `{ email, fullName, password }` | `{ message }` |
| POST | `/api/auth/login` | `{ email, password }` | `{ userName, token }` + sets `neurosync_jwt` cookie |

### Tasks (requires auth)
| Method | Path | Body | Returns |
| --- | --- | --- | --- |
| POST | `/api/tasks/create-task` | `{ rawText }` | `TaskItem` with `microSteps[]` (`heading`, `description`, `estimatedMinutes`, `orderIndex`) |
| GET | `/api/tasks/my-tasks` | – | `TaskItem[]` for the current user |

### Summarizer (requires auth)
| Method | Path | Body | Returns |
| --- | --- | --- | --- |
| POST | `/api/summarizer/analyze` | a raw JSON **string** (the document text) | `DocumentAnalysisResult` |

`DocumentAnalysisResult`:
```jsonc
{
  "summary": "string",
  "actionItems": ["string"],
  "deadline": "string",
  "tone": "string",
  "highlights": ["string"],     // key points
  "hiddenTasks": ["string"],    // implied tasks
  "simplifiedText": "string"    // dyslexia-friendly rewrite
}
```

> The summarizer endpoint binds `[FromBody] string`, so callers must send a
> JSON-encoded string (e.g. `JSON.stringify(text)` from the frontend).

---

## AI integration
`Services/AiAssistantService.cs` builds two purpose-written prompts:
- **Task breakdown** — *"expert ADHD Productivity Coach"*; returns a title, a simple summary, and 3–7 ordered steps with whole-number minute estimates.
- **Document analysis** — *"Dyslexia-friendly Document Assistant"*; returns the fields above, including a short-sentence/bulleted rewrite.

Both request JSON output (`ResponseMimeType = application/json`) and are
deserialized case-insensitively. AI calls are wrapped so failures return a clean
`502 { message }` instead of an unhandled 500.

---

## Configuration & secrets
**No secrets are committed.** Provide these via **user-secrets** (dev) or **Azure
App Settings / environment variables** (prod):

| Key | Purpose |
| --- | --- |
| `Jwt:Key` | JWT signing secret (use a long random value) |
| `Gemini:ApiKey` | Google Gemini API key |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Cors:AllowedOrigins` | *(optional)* array of allowed frontend origins (see below) |

Set secrets locally:
```bash
cd NeuroSync
dotnet user-secrets set "Jwt:Key" "<a-long-random-secret>"
dotnet user-secrets set "Gemini:ApiKey" "<your-gemini-key>"
```
In Azure, set them as App Settings using the double-underscore form, e.g.
`Jwt__Key`, `Gemini__ApiKey`, `Cors__AllowedOrigins__0`.

---

## Getting started (local)
### Prerequisites
- **.NET 8 SDK**
- **SQL Server LocalDB** (ships with Visual Studio) or a SQL Server instance

### Run
```bash
cd NeuroSync
dotnet restore
dotnet user-secrets set "Jwt:Key" "<secret>"
dotnet user-secrets set "Gemini:ApiKey" "<key>"
dotnet run
```
- EF Core migrations are **applied automatically** at startup (`db.Database.Migrate()`).
- Swagger UI is available in Development at `/swagger`.
- Note the HTTP/HTTPS port shown on startup and use it for the frontend's `VITE_API_BASE`.

---

## Auth model
- Login issues a JWT (7-day expiry) and:
  - sets an **HttpOnly `neurosync_jwt` cookie** (`SameSite=None; Secure`), best for a same-site web app; **and**
  - **returns the token in the body** so a cross-origin SPA can store it.
- JWT validation **prefers the cookie**, and **falls back to the `Authorization: Bearer` header** when no cookie is present (`Program.cs` → `OnMessageReceived`).
- Protected controllers use `[Authorize]`; the user id comes from the `NameIdentifier` claim.

---

## CORS
Configured in `Program.cs` (policy `NeuroSyncCors`). Allowed origins default to the
local Vite dev/preview servers and can be overridden via config:
```jsonc
// appsettings.json (or Azure App Settings)
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://your-deployed-frontend-url"
  ]
}
```
`UseCors` runs **before** authentication so preflight (OPTIONS) requests succeed.

---

## 🚀 Deploying to Azure (required for the live integration)
The browser integration depends on three changes that are in this codebase but
**must be redeployed** to `neurosync.azurewebsites.net` to take effect:
1. **CORS** enabled for the frontend origin.
2. Login **returns the JWT in the body**.
3. JWT auth **falls back to the Authorization header**.

Redeploy (e.g. via Visual Studio "Publish", `az webapp deploy`, or your existing
Azure DevOps pipeline), then confirm:
- `OPTIONS`/`POST` from the frontend origin succeed (no CORS error in the browser console),
- `POST /api/auth/login` response body contains `token`,
- `POST /api/tasks/create-task` with `Authorization: Bearer <token>` returns 200.

**Also add your deployed frontend's origin** to `Cors:AllowedOrigins` before the jury demo if you host the frontend somewhere other than localhost.

---

## Security & data handling (hackathon note)
- **Do not** push this code or any Accenture artifacts to a **public** GitHub repo (DLP). Use a private repo / Azure DevOps.
- Passwords are hashed with **BCrypt**; secrets are never committed.
- Consider adding rate limiting and stricter CORS origins before production.

## Known gaps / roadmap
- `Reminder` and `UserSettings` models exist but have **no endpoints yet** (focus mode, progress, reminders are client-side for now).
- No update/delete endpoints for tasks; no pagination on `my-tasks`.
