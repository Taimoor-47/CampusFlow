# CampusFlow

A student management system REST API built with **ASP.NET Core 8 (Web API)** and
**Entity Framework Core** on **SQL Server**. It manages students, teachers, GPA records,
class schedules, and assignment submissions, with **JWT cookie-based authentication** and
**role-based authorization** (Student vs Teacher).

> New to the codebase? Read this file top to bottom, then dive into
> [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for a deeper, beginner-friendly tour.
> For the file-upload feature specifically, see [`docs/AssignmentFeature.md`](docs/AssignmentFeature.md).

---

## 1. What the app does

| Area | Capability |
|------|-----------|
| **Auth** | Students & teachers register and log in. A JWT is issued and stored in an HTTP-only cookie. |
| **Roles** | Endpoints are protected by role — students access their own data; teachers manage students. |
| **GPA** | Teachers record a student's GPA per semester; students view their own GPA history. |
| **Schedules** | Teachers add class schedule entries; students view their timetable. |
| **Assignments** | Teachers upload assignments (with optional brief files); students upload file submissions. |

---

## 2. Tech stack

- **.NET 8** / ASP.NET Core Web API (controllers)
- **Entity Framework Core 8** (code-first migrations) + **SQL Server / LocalDB**
- **JWT Bearer** authentication (token delivered via cookie)
- **Swagger / OpenAPI** for interactive API docs (development only)
- **Docker** support + a **GitHub Actions** CI workflow

---

## 3. Project structure

```
CampusFlow/                      ← solution root
├─ CampusFlow.slnx               ← solution file
├─ README.md                     ← you are here
├─ docs/
│  ├─ ARCHITECTURE.md            ← full architecture & concepts guide
│  └─ AssignmentFeature.md       ← deep-dive on the file-upload feature
├─ .github/workflows/dotnet.yml  ← CI: build on push/PR
└─ CampusFlow/                   ← the actual project
   ├─ Program.cs                 ← app startup: DI, auth, middleware pipeline
   ├─ appsettings.json           ← config + secrets (gitignored; see .example)
   ├─ appsettings.example.json   ← template showing required config keys
   ├─ Dockerfile                 ← container build (⚠ stale — see Known Issues)
   │
   ├─ Controllers/               ← HTTP layer (routes, status codes)
   │  ├─ StudentController.cs
   │  ├─ TeacherController.cs
   │  ├─ AuthController.cs        ← logout
   │  └─ WeatherForecastController.cs  ← leftover scaffolding (safe to delete)
   │
   ├─ Services/                  ← business-logic layer
   │  ├─ IStudentService.cs / StudentService.cs
   │  ├─ ITeacherService.cs / TeacherService.cs
   │  ├─ IFileStorageService.cs / FileStorageService.cs  ← saves uploads to disk
   │  └─ JwtServicescs.cs        ← builds JWT tokens
   │
   ├─ Repositories/              ← data-access layer (only place touching the DB)
   │  └─ IStudentRepository.cs / StudentRepository.cs
   │
   ├─ Model/                     ← entities (become DB tables)
   │  ├─ Student.cs  Teacher.cs
   │  ├─ Assignment.cs  Submission.cs
   │  ├─ StudentGPA.cs  Schedules.cs
   │  └─ Course.cs  CourseEnrollment.cs
   │
   ├─ DTO/                       ← request/response shapes
   ├─ Data/AppDbContext.cs       ← EF Core DB context (tables + relationships)
   ├─ Helpers/                   ← PasswordHelper, Roles constants
   ├─ Migrations/                ← EF Core schema history
   └─ wwwroot/uploads/           ← saved files (gitignored)
```

---

## 4. Architecture in one diagram

The app follows a **3-layer architecture**. Each layer only depends on the one below it.

```
   HTTP request
        │
        ▼
 ┌─────────────┐   Reads the request, checks auth/role, returns status codes.
 │ Controller  │   Knows HTTP. Knows nothing about the database.
 └─────────────┘
        │ calls an interface (IStudentService, ITeacherService)
        ▼
 ┌─────────────┐   Business rules & validation.
 │   Service   │   Knows nothing about HTTP.
 └─────────────┘
        │ calls (StudentService → IStudentRepository)
        ▼
 ┌─────────────┐   The ONLY place that talks to AppDbContext / SQL.
 │ Repository  │
 └─────────────┘
        │
        ▼
 ┌─────────────┐
 │ AppDbContext│ → SQL Server
 └─────────────┘
```

The classes are wired together by **dependency injection**, configured in `Program.cs`.
(Note: `TeacherService` currently uses `AppDbContext` directly instead of a repository —
the student side is the cleaner reference example.)

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full explanation of every layer,
concept, and data model.

---

## 5. Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server **LocalDB** (ships with Visual Studio) or any SQL Server instance
- EF Core tools: `dotnet tool install --global dotnet-ef`

### Setup
```bash
# 1. Clone, then create your local config from the template
cd CampusFlow
cp appsettings.example.json appsettings.json
#   → edit appsettings.json: set a real Jwt:Key and your connection string

# 2. Create the database from the migrations
dotnet ef database update

# 3. Run
dotnet run
```

The API starts at:
- HTTPS: `https://localhost:7288`
- HTTP:  `http://localhost:5021`

Open **`https://localhost:7288/swagger`** to explore and test endpoints interactively.

---

## 6. API reference

All protected routes require a valid `jwt` cookie (set automatically on login).
`[Authorize(Roles = "...")]` restricts each route to a role.

### Auth (public)
| Method | Route | Body | Description |
|--------|-------|------|-------------|
| POST | `/api/student/register` | `StudentDto` (JSON) | Create a student account |
| POST | `/api/student/login` | `LoginDto` (JSON) | Log in → sets `jwt` cookie |
| POST | `/api/teacher/register` | `RegisterTeacherDto` (JSON) | Create a teacher account |
| POST | `/api/teacher/login` | `LoginDto` (JSON) | Log in → sets `jwt` cookie |
| POST | `/api/auth/logout` | — | Clears the `jwt` cookie |

### Student (role: Student)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/student/me` | Current student's id + email |
| GET | `/api/student/my-gpa` | My GPA records |
| GET | `/api/student/my-schedules` | My class schedule |
| GET | `/api/student/my-assignments` | All assignments + my submission status |
| POST | `/api/student/assignments/{id}/submit` | Upload my file (multipart) |

### Teacher (role: Teacher)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/teacher/students` | List all students |
| POST | `/api/teacher/add-gpa` | Record a student's semester GPA |
| POST | `/api/teacher/add-schedule` | Add a schedule entry for a student |
| POST | `/api/teacher/upload-assignment` | Create an assignment (+ optional file, multipart) |
| GET | `/api/teacher/assignments/{id}/submissions` | View all submissions for an assignment |

---

## 7. Authentication flow

1. User calls `login`. The service verifies the password (`PasswordHelper.Verify`).
2. `JwtServicescs.GenerateJwtToken` creates a signed JWT containing the user's **id**,
   **email**, and **role**.
3. The token is returned in an **HTTP-only cookie** named `jwt` (JS can't read it → safer).
4. On later requests, `Program.cs` reads the token from that cookie
   (`OnMessageReceived`), validates it, and populates `User` with the claims.
5. `[Authorize(Roles = "Student")]` etc. checks the role claim before running the action.

---

## 8. Database model (entities)

| Entity | Key fields | Relationships |
|--------|-----------|---------------|
| `Student` | Id, Name, Email, Password, Role | has many GPA, Schedules, Submissions |
| `Teacher` | Id, Name, Email, Password, Role | (creates Assignments) |
| `StudentGPA` | Id, Semester, Gpa, StudentId | belongs to Student |
| `Schedules` | Id, CourseTitle, Room, Start/EndTime, StudentId | belongs to Student |
| `Assignment` | Id, Title, Description, DueDate, FilePath, TeacherId | has many Submissions |
| `Submission` | Id, AssignmentId, StudentId, FilePath, SubmittedAt | belongs to Assignment + Student; unique per (Assignment, Student) |
| `Course` / `CourseEnrollment` | course catalog + grades | (defined; not yet exposed via endpoints) |

---

## 9. Configuration & secrets

`appsettings.json` holds the DB connection string; the active JWT signing key lives
**outside source control**. A previously committed key was rotated on 2026-08-26
because the repository is public; any copy preserved in Git history remains
compromised and must never be reused.

Local development: `dotnet user-secrets set "Jwt:Key" "<64-char random base64>"`
(run inside `CampusFlow/CampusFlow/`; the project already has a UserSecretsId).
Production: environment variable `Jwt__Key`. The committed `appsettings.json`
contains only a short placeholder that fails loudly if the real key is missing.

Required keys:
```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "Jwt": { "Key": "<via user-secrets or env, never committed>", "Issuer": "...", "Audience": "...", "TokenLifetimeDays": 7 }
}
```

---

## 10. Common commands

```bash
dotnet build                                  # compile
dotnet run                                    # run locally
dotnet ef migrations add <Name>               # create a new migration after model changes
dotnet ef database update                     # apply migrations to the DB
dotnet ef migrations remove                   # undo the last (unapplied) migration
```

---

## 11. Known issues / cleanup opportunities

These are existing rough edges worth knowing about (not introduced by recent work):

- **`Dockerfile` is stale** — it still references the old project name
  `StudentMAnagementSystem.csproj` and uses .NET **10** images, so it won't build as-is.
  Update the paths to `CampusFlow` and the image tags to `8.0`.
- **CI workflow has a typo** — `.github/workflows/dotnet.yml` references a non-existent
  `actions/setup-dependencies@v4` step. Remove it; the real setup is `actions/setup-dotnet@v4`.
- **Password hashing is unsalted SHA-256** (`Helpers/PasswordHelper.cs`). This is fine for
  learning but not production-safe — prefer a salted, slow hash like **BCrypt** or
  ASP.NET's `PasswordHasher<T>`.
- **`WeatherForecastController.cs`** is leftover template scaffolding and can be deleted.
- **`dotnet ef` is v10 while the runtime targets v8** — migrations scaffold fine, but
  aligning the tool version avoids surprises.

---

## 12. Where to learn more

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — full guide to the layers, DI, DTOs,
  auth, EF Core, and how a request flows end-to-end.
- [`docs/AssignmentFeature.md`](docs/AssignmentFeature.md) — the assignment/submission
  upload feature explained step by step.
