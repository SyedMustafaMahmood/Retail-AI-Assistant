# E3 Retail — Retail AI Assistant

> An AI-powered internal tool for retail operations — built with .NET 10, EF Core, Google Gemini, and Angular 21.

---

## Overview

This AI Assistant is a full-stack Proof of Concept that demonstrates how AI can be embedded into real retail workflows. It combines a clean ASP.NET Core backend with an Angular frontend to deliver three core AI-powered features:

- **Ticket Summarization** — Support staff can summarize customer tickets on demand using Gemini
- **Hybrid Product Recommendation** — Combines collaborative filtering on transaction history with embedding-based cosine similarity
- **Policy QA Assistant** — RAG pipeline that answers questions grounded in uploaded policy documents

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core (.NET 10), EF Core, SQL Server |
| AI | Google Gemini API (completions + embeddings) |
| Auth | JWT + Refresh Token rotation, BCrypt |
| Frontend | Angular 21 (standalone components) |
| Versioning | Asp.Versioning (URL segment — V1/V2) |
| ORM | Entity Framework Core with migrations |

---

## Architecture

```
Angular 21 Frontend (Customer / Staff / Admin)
              ↓
ASP.NET Core Web API — Versioned (V1 / V2)
              ↓
Services Layer — IAIClient, IEmbeddingClient, IPolicyQAService
              ↓
Google Gemini API — Completions + Embeddings
              ↓
EF Core + SQL Server — 12 entities, migrations
```

### SOLID Principles Applied

- **S** — Each service has a single responsibility (`TicketService`, `EmbeddingBuilderService`, `JwtService`, `MarkdownLoader`)
- **I** — `IAIClient` (completions) and `IEmbeddingClient` (embeddings) are separate interfaces
- **D** — All services injected via `Program.cs` composition root — no `new` in controllers
- **O/L** — New AI provider can implement `IAIClient` without touching any business logic

---

## Features

### Customer Portal
- Browse products with images
- Add to cart — live "You might also like" recommendations
- Raise support tickets and view own ticket history
- Policy chatbot with streaming responses

### Staff Portal
- View all tickets with search and status filters
- Summarize tickets with streaming AI output
- Change ticket status (Open / InProgress / Resolved)
- Policy assistant for customer and internal questions

### Admin Portal
- All staff capabilities
- Upload policy documents (`.txt` / `.md`)
- Create SupportAgent accounts
- Delete resolved tickets

---

## Project Structure

```
Retail-AI-Assistant/
├── Track/                      ← .NET backend
│   ├── AI/                     ← IAIClient, IEmbeddingClient, GeminiClient
│   ├── Controllers/            ← API endpoints (V1, V2, unversioned)
│   ├── Data/                   ← AppDbContext, DataSeeder, AdminSeeder
│   ├── DTO/                    ← Request/response models
│   ├── Helpers/                ← MarkdownLoader, VectorHelper, Similarity
│   ├── Migrations/             ← EF Core migrations
│   ├── Models/                 ← 12 entity models
│   ├── Prompts/                ← Externalized .md prompt files
│   ├── Repositories/           ← Interface + implementation pattern
│   ├── Services/               ← Business logic layer
│   └── wwwroot/images/         ← Product images served statically
│
└── Frontend/
    └── track-ui/               ← Angular 21 frontend
        └── src/app/
            ├── core/           ← Guards, interceptors, services
            ├── features/       ← Customer, Staff, Admin feature modules
            └── shared/         ← Models, shared components
```

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (local or remote)
- Node.js 18+
- Angular CLI (`npm install -g @angular/cli`)
- Google Gemini API key

---

### Backend Setup

**1. Clone the repository**
```bash
git clone https://github.com/SyedMustafaMahmood/Retail-AI-Assistant.git
cd Retail-AI-Assistant/Track
```

**2. Configure `appsettings.json`**

Copy the example and fill in your values:
```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=AIRetail;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "https://localhost:7073",
    "Audience": "https://localhost:7073",
    "AccessTokenMinutes": 60
  }
}
```

> ⚠️ Never commit `appsettings.json` or `appsettings.Development.json` with real keys. These are gitignored by default.

**3. Apply migrations and seed data**
```bash
dotnet ef database update
```

The app auto-seeds on startup:
- Products + transaction history
- Admin account: `admin@gmail.com` / `Admin@123`
- Product embeddings via Gemini

**4. Run the backend**
```bash
dotnet run
```

Backend runs at `https://localhost:7073`

---

### Frontend Setup

**1. Navigate to frontend**
```bash
cd ../Frontend/track-ui
```

**2. Install dependencies**
```bash
npm install
```

**3. Check environment**

`src/environments/environment.ts` should point to your backend:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7073/api'
};
```

**4. Run the frontend**
```bash
ng serve
```

Frontend runs at `http://localhost:4200`

---

## User Accounts

| Role | How to Create | Default Credentials |
|------|--------------|-------------------|
| Admin | Auto-seeded on startup | `admin@gmail.com` / `Admin@123` |
| Customer | Register at `/register` | Any email + password |
| SupportAgent | Admin creates via API | Set by admin |

### Create a SupportAgent (via Postman)
```
POST https://localhost:7073/api/v2/auth/create-agent
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Staff User",
  "email": "staff@example.com",
  "password": "Staff@123"
}
```

---

## API Endpoints

### Auth (V2 — with refresh tokens)
```
POST /api/v2/auth/register          ← Customer registration
POST /api/v2/auth/login             ← Login (returns access + refresh token)
POST /api/v2/auth/refresh           ← Refresh access token
POST /api/v2/auth/create-agent      ← Admin only: create support agent
```

### Tickets
```
POST   /api/tickets                 ← Customer: create ticket
GET    /api/tickets                 ← Staff/Admin: get all tickets
GET    /api/tickets/my              ← Customer: get own tickets
GET    /api/tickets/{id}            ← Get ticket by ID
POST   /api/tickets/{id}/summarize  ← Staff/Admin: AI summarize
POST   /api/tickets/{id}/summarize-stream ← Staff/Admin: streaming AI summary
PATCH  /api/tickets/{id}/status     ← Staff/Admin: update status
DELETE /api/tickets/{id}            ← Admin: delete ticket
GET    /api/tickets/status/{status} ← Filter by status
GET    /api/tickets/search?query=   ← Search tickets
```

### Products & Recommendations
```
GET  /api/products                  ← Get all products
POST /api/recommend                 ← Hybrid product recommendations
```

### Policy QA
```
POST /api/policy/upload             ← Admin/Staff: upload policy document
POST /api/policy/ask                ← All roles: ask policy question
POST /api/policy/ask-stream         ← All roles: streaming policy answer
```

---

## AI Implementation Details

### Ticket Summarization
1. Staff triggers `POST /api/tickets/{id}/summarize-stream`
2. `TicketService` fetches ticket from DB
3. Loads `TicketSummarization.md` prompt via `MarkdownLoader`
4. Calls `IAIClient.GetCompletionStreamAsync()` → streams to frontend
5. Result logged in `RequestLog` table

### Hybrid Product Recommendation
1. Customer adds products to cart → `POST /api/recommend`
2. **Stage 1 — Collaborative filtering:** Analyzes `Transaction` + `TransactionItem` tables, finds co-purchase patterns, assigns confidence score
3. **Stage 2 — Embedding similarity:** Cosine similarity between cart product embeddings and candidate embeddings in `EmbeddingMetadata` table
4. **Hybrid scoring:** `(0.6 × confidence) + (0.4 × similarity)` when confidence ≥ 1, else `(0.2 × confidence) + (0.8 × similarity)`
5. Top 3 results enriched with AI-generated reason via `ProductRecommendation.md`

### Policy QA (RAG Pipeline)
1. Admin uploads `.txt`/`.md` document → split into 500-char chunks
2. Each chunk embedded via Gemini → stored in `DocumentChunk` table with `EmbeddingJson`
3. User query embedded → cosine similarity computed against all chunks
4. Top 3 chunks retrieved → injected into `PolicyQA.md` prompt
5. Gemini answers grounded in document context only → streamed to frontend
6. Response logged in `QueryLog` table

---

## Prompt Management

All AI prompts are externalized to `.md` files in the `Prompts/` folder and loaded via `MarkdownLoader` with `{{placeholder}}` replacement:

| File | Purpose |
|------|---------|
| `TicketSummarization.md` | Ticket summary prompt |
| `ProductRecommendation.md` | Recommendation reasoning prompt |
| `PolicyQA.md` | RAG-based policy answer prompt |

This means prompt engineering can be done without touching C# code.

---

## Database Entities

| Entity | Purpose |
|--------|---------|
| `User` | Authentication — Customer / SupportAgent / Admin |
| `RefreshToken` | JWT refresh token rotation |
| `Ticket` | Customer support tickets |
| `Product` | Product catalog with images |
| `Transaction` | Customer purchase history |
| `TransactionItem` | Individual items per transaction |
| `EmbeddingMetadata` | Product embeddings as vectors |
| `Document` | Uploaded policy documents |
| `DocumentChunk` | Policy document chunks with embeddings |
| `RequestLog` | AI prompt/response logs for tickets |
| `QueryLog` | Policy Q&A logs |
| `RecommendationLog` | Recommendation request logs |

---

## Resilience

- `IHttpClientFactory` via `builder.Services.AddHttpClient()` — no `new HttpClient()` per request
- All AI calls wrapped in try/catch — failures logged with `IsSuccess = false`
- Seeding and migration wrapped in try/catch — app starts even if seeding fails
- JWT interceptor in Angular automatically refreshes expired tokens on 401

---

## Stretch Goals Achieved

- ✅ **Streaming responses** — ticket summarization and policy QA stream word by word
- ✅ **RAG pipeline** — full embedding + retrieval with `DocumentChunk` table
- ✅ **Hybrid recommendation** — collaborative filtering + cosine similarity combined
- ✅ **Prompt externalization** — all prompts in `.md` files, version-controllable
- ✅ **API versioning** — V1 (basic JWT) and V2 (refresh token rotation)
- ✅ **Repository pattern** — full interface + implementation separation for all data access

---

## Future Scope

- Unit tests with mock `IAIClient` to prove SOLID contract
- Multi-document policy support with Customer vs Staff document separation
- Admin analytics dashboard with ticket and AI usage charts
- Docker + Azure deployment
- Mobile app using the same backend API
- Toast notifications replacing browser alerts
- Cart persistence across page refreshes

---

## Contributors

| Name | Role |
|------|------|
| Syed Mustafa | Backend AI features, Angular frontend |
| Sindhu Indukuri | Authentication, Repository pattern, Product images, Streaming |


## NOTE ⭐

--> If admin credentials dont work, it is probably because there is no Admin Seeder file. Manually enter admin credentials into your database and try again. You can then create a staff / internal support agent through admin endpoints (manually through postman). Try manually for customers too if it doesnt work.