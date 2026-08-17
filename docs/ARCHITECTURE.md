# CampusFlow — Architecture & Concepts Guide

A beginner-friendly, in-depth tour of how the whole project is built and *why*. Read this
after the [README](../README.md). By the end you'll understand every folder, the patterns
used, and how a single HTTP request travels through the app.

---

## Table of contents
1. [Mental model: what is this app?](#1-mental-model)
2. [The request lifecycle (the big one)](#2-the-request-lifecycle)
3. [The 3-layer architecture](#3-the-3-layer-architecture)
4. [Dependency Injection explained](#4-dependency-injection)
5. [Program.cs line by line](#5-programcs-line-by-line)
6. [The data model & EF Core](#6-the-data-model--ef-core)
7. [DTOs: why they exist](#7-dtos)
8. [Authentication & authorization](#8-authentication--authorization)
9. [Migrations: how the DB schema is managed](#9-migrations)
10. [Folder-by-folder reference](#10-folder-by-folder-reference)
11. [Glossary](#11-glossary)

---

## 1. Mental model

CampusFlow is a **REST API**. It has no web pages of its own — it just receives HTTP
requests (from Swagger, Postman, or a frontend like Next.js) and returns **JSON**.

Think of it as a waiter in a restaurant:
- The **Controller** is the waiter — takes your order (request), brings back food (response).
- The **Service** is the chef — knows the recipes (business rules).
- The **Repository** is the pantry keeper — the only one allowed into the storeroom (database).

Keeping these jobs separate is what makes the code easy to change and test.

---

## 2. The request lifecycle

Let's trace **"a student views their assignments"** from click to response:

```
1. HTTP GET /api/student/my-assignments   (with the jwt cookie attached)
        │
2.  ASP.NET middleware pipeline (Program.cs):
        UseAuthentication → reads the jwt cookie, validates it, builds `User`
        UseAuthorization  → checks [Authorize(Roles="Student")]
        │
3.  StudentController.MyAssignments()
        • GetStudentId() pulls the student's id from the token claims
        • calls _studentService.GetMyAssignments(id)
        │
4.  StudentService.GetMyAssignments(id)
        • _repository.GetAllAssignments()
        • _repository.GetSubmissionsByStudentId(id)
        • merges them into a list of StudentAssignmentDto
        │
5.  StudentRepository
        • runs EF Core queries against AppDbContext → SQL Server
        │
6.  Result bubbles back up: Repository → Service → Controller
        │
7.  Controller returns Ok(dtos) → ASP.NET serializes to JSON → HTTP 200
```

Every feature follows this same shape. Once you understand one, you understand them all.

---

## 3. The 3-layer architecture

```
┌──────────────────────────────────────────────────────────────┐
│  CONTROLLER  (Controllers/*.cs)                                │
│  • Defines routes ([HttpGet], [HttpPost])                      │
│  • Reads the logged-in user (User.FindFirstValue)              │
│  • Translates results & exceptions into HTTP status codes      │
│  • Depends ONLY on service interfaces                          │
└──────────────────────────────────────────────────────────────┘
                         │  IStudentService / ITeacherService
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  SERVICE  (Services/*.cs)                                      │
│  • Business rules: "already submitted?", "save file first"     │
│  • Maps entities ↔ DTOs                                        │
│  • No HTTP types, no status codes — throws exceptions instead  │
└──────────────────────────────────────────────────────────────┘
                         │  IStudentRepository
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  REPOSITORY  (Repositories/*.cs)                              │
│  • The ONLY layer that imports AppDbContext                    │
│  • Pure data access: Add, Where, FirstOrDefault, ToList        │
└──────────────────────────────────────────────────────────────┘
                         │
                         ▼
                  AppDbContext → SQL Server
```

**Why interfaces between layers?** Each layer talks to the next through an *interface*
(`IStudentService`, `IStudentRepository`). This means a layer doesn't depend on a concrete
class — only on a contract. You can swap implementations (e.g. for testing with a fake
repository) without changing the caller. This is the **Dependency Inversion Principle**.

> ⚠️ **Consistency note:** `StudentService` uses a repository (the clean pattern).
> `TeacherService` skips the repository and talks to `AppDbContext` directly. Both compile
> and work, but if you want a consistent codebase, refactor `TeacherService` to use a
> `ITeacherRepository` the same way. Use the student side as your template.

---

## 4. Dependency Injection

You never see `new StudentService(...)` anywhere in the controllers. Instead, classes
**declare what they need** in their constructor, and ASP.NET supplies it. This is
**Dependency Injection (DI)**.

```csharp
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    // ASP.NET sees this constructor and automatically passes in an IStudentService.
    public StudentController(IStudentService studentService, JwtServicescs jwtService)
    {
        _studentService = studentService;
    }
}
```

For this to work, every service is **registered** in `Program.cs`:

```csharp
builder.Services.AddScoped<IStudentService, StudentService>();
//                         ^contract        ^concrete class to use
```

**Lifetimes** (how long an instance lives):
- `AddScoped` — one instance **per HTTP request** (used for services/repositories that use
  the DbContext, because the DbContext itself is scoped).
- `AddSingleton` — one instance for the **whole app** (used for `FileStorageService`,
  which holds no per-user state).
- `AddTransient` — a new instance **every time** it's requested (not used here).

---

## 5. Program.cs line by line

`Program.cs` is the app's entry point. It does two things: **register services** (the DI
container) and **build the middleware pipeline** (what happens to each request, in order).

```csharp
var builder = WebApplication.CreateBuilder(args);

// ── 1. Authentication: validate JWTs, and read the token from the "jwt" cookie ──
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters { /* issuer, audience, key */ };
        options.Events = new JwtBearerEvents {
            OnMessageReceived = ctx => { ctx.Token = ctx.Request.Cookies["jwt"]; return Task.CompletedTask; }
        };
    });

builder.Services.AddAuthorization();

// ── 2. Database: register EF Core with the SQL Server connection string ──
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

// ── 3. Our own services (the DI registrations) ──
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<JwtServicescs>();

// ── 4. Framework services ──
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(/* allow the Next.js frontend on :3000, with credentials */);

var app = builder.Build();

// ── 5. The middleware pipeline — ORDER MATTERS, runs top to bottom per request ──
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseStaticFiles();      // serve uploaded files from wwwroot (e.g. /uploads/...)
app.UseAuthentication();   // WHO are you? (reads & validates the cookie)
app.UseAuthorization();    // are you ALLOWED here? (checks [Authorize])
app.MapControllers();      // route the request to a controller action
app.Run();
```

The pipeline order matters: `UseAuthentication` must come before `UseAuthorization`, and
both before `MapControllers`, or roles won't be enforced.

---

## 6. The data model & EF Core

**Entity Framework Core** is an **ORM** — it maps C# classes to database tables so you write
LINQ instead of SQL. Each class in `Model/` becomes a table; each property becomes a column.

### Entities and their relationships

```
        ┌──────────┐                      ┌──────────┐
        │ Teacher  │                      │ Student  │
        └────┬─────┘                      └────┬─────┘
             │ creates                          │ owns
             ▼                                  ├──────────────┬──────────────┐
        ┌──────────┐  1     many ┌──────────┐  ▼              ▼              ▼
        │Assignment│────────────<│Submission│        ┌──────────┐   ┌──────────┐
        └──────────┘             └────┬─────┘        │StudentGPA│   │Schedules │
                                      │ many          └──────────┘   └──────────┘
                                      └──> belongs to a Student
```

- A **Student** has many `StudentGPA`, `Schedules`, and `Submissions`.
- A **Teacher** creates `Assignment`s.
- An **Assignment** has many `Submission`s (one per student who turns it in).
- A **Submission** links one Student to one Assignment + the file they uploaded.

### How relationships are configured — `Data/AppDbContext.cs`

Two ways relationships are defined in this project:

**1. Data annotations** (attributes on the model):
```csharp
public Guid StudentId { get; set; }
[ForeignKey(nameof(StudentId))]      // marks StudentId as the FK to Student
public Student Student { get; set; }
```

**2. Fluent API** (in `OnModelCreating`), for the more complex rules:
```csharp
// One assignment → many submissions
modelbuilder.Entity<Assignment>()
    .HasMany(a => a.Submissions).WithOne(s => s.Assignment)
    .HasForeignKey(s => s.AssignmentId);

// A student's submissions, with Restrict delete to avoid SQL Server's
// "multiple cascade paths" error (since Submission is reachable from both
// Assignment and Student).
modelbuilder.Entity<Student>()
    .HasMany(s => s.Submissions).WithOne(sub => sub.Student)
    .HasForeignKey(sub => sub.StudentId)
    .OnDelete(DeleteBehavior.Restrict);

// A student can submit a given assignment only once.
modelbuilder.Entity<Submission>()
    .HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
```

The `DbSet<T>` properties are the tables you can query:
```csharp
public DbSet<Student> Students { get; set; }
public DbSet<Assignment> Assignments { get; set; }
public DbSet<Submission> Submissions { get; set; }
// ...
```

### `Course` / `CourseEnrollment`
These entities exist in `Model/` (course catalog + per-student grades) and there are DTOs
for grade cards (`GradeCardDto`, `SemesterResultDto`, `CourseGradeDto`), but they are **not
yet wired to endpoints**. They're scaffolding for a future "transcript/grade card" feature.

---

## 7. DTOs

A **DTO (Data Transfer Object)** is a plain class that defines the *exact shape* of data
crossing the API boundary — separate from your database entities.

**Why not just return entities?**
- **Security**: a `Student` entity has a `Password` field. Returning it raw would leak the
  hash. A DTO exposes only what's safe.
- **Shape control**: `StudentAssignmentDto` merges data from *two* entities (Assignment +
  Submission) into one tidy object the frontend can use directly.
- **Validation**: input DTOs carry `[Required]`, `[EmailAddress]`, `[Range]` attributes
  that ASP.NET checks automatically before your code runs.

| DTO | Direction | Used by |
|-----|-----------|---------|
| `StudentDto` | in | student registration |
| `RegisterTeacherDto` | in | teacher registration |
| `LoginDto` | in | login (both roles) |
| `AddGpaDto` | in | teacher records GPA |
| `AddScheduleDto` | in | teacher adds schedule |
| `AddAssignmentDto` | in | teacher uploads assignment (has an `IFormFile`) |
| `StudentAssignmentDto` | out | student's assignment list + submission status |
| `GradeCardDto` / `SemesterResultDto` / `CourseGradeDto` | out | (future) grade card |

---

## 8. Authentication & authorization

**Authentication** = "who are you?" **Authorization** = "what are you allowed to do?"

### The flow
1. **Login** (`StudentController.Login` / `TeacherController.Login`):
   - `Service.Login` looks up the user and verifies the password with
     `PasswordHelper.Verify` (SHA-256 hash comparison).
   - `JwtServicescs.GenerateJwtToken` builds a signed token with three **claims**:
     `NameIdentifier` (id), `Email`, and `Role`.
   - The token is written to an **HTTP-only, Secure, SameSite=None** cookie named `jwt`.
     HTTP-only means JavaScript can't read it → protects against XSS token theft.
2. **Subsequent requests**: `Program.cs`'s `OnMessageReceived` copies the token out of the
   `jwt` cookie so the JWT middleware can validate it and populate `User`.
3. **Authorization**: attributes guard each action:
   ```csharp
   [Authorize(Roles = "Student")]   // only tokens with role=Student pass
   [Authorize(Roles = "Teacher")]   // only tokens with role=Teacher pass
   ```
4. **Reading the current user** inside an action:
   ```csharp
   var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
   Guid.TryParse(raw, out var id);   // the logged-in user's id
   ```

`Helpers/Roles.cs` defines the role strings as constants (`Roles.Student`, `Roles.Teacher`)
to avoid typos — though the controllers currently use the literal strings.

> 🔒 **Security note for learning vs production:** `PasswordHelper` uses plain SHA-256
> with no salt. Fast unsalted hashes are vulnerable to rainbow-table / brute-force attacks.
> In production use **BCrypt**, **Argon2**, or ASP.NET's built-in `PasswordHasher<T>`.

---

## 9. Migrations

EF Core is **code-first**: you change C# model classes, then generate a **migration** that
records the matching SQL schema changes, then **apply** it to the database.

```bash
# After editing anything in Model/ or AppDbContext.cs:
dotnet ef migrations add DescribeYourChange   # creates a file in Migrations/
dotnet ef database update                      # runs it against the DB
```

Existing migration history (in `Migrations/`):
| Migration | What it did |
|-----------|-------------|
| `InitialCleanSetup` | first schema |
| `AddTeacherRoleSystem` | added teachers + roles |
| `RoleBasedCleanup` | tidied role-based structure |
| `AddAssignmentFilesAndSubmissions` | added `Assignment.FilePath`/`TeacherId`, the `Submissions` table, dropped `Assignment.StudentId` |

Each migration has an `Up()` (apply) and `Down()` (rollback) method. The
`AppDbContextModelSnapshot.cs` file is EF's record of the *current* model — don't edit it
by hand.

---

## 10. Folder-by-folder reference

| Folder / file | Responsibility |
|---------------|----------------|
| `Program.cs` | Startup: DI registrations + middleware pipeline |
| `Controllers/` | HTTP endpoints. One controller per area (Student, Teacher, Auth) |
| `Services/` | Business logic + entity↔DTO mapping. Interfaces + implementations |
| `Services/JwtServicescs.cs` | Builds signed JWT tokens |
| `Services/FileStorageService.cs` | Saves uploaded files to `wwwroot/uploads`, validates them |
| `Repositories/` | Data access. Only layer that touches `AppDbContext` |
| `Model/` | Entities → database tables |
| `DTO/` | Request/response shapes with validation attributes |
| `Data/AppDbContext.cs` | EF Core context: `DbSet`s + relationships + indexes |
| `Helpers/PasswordHelper.cs` | Password hashing/verification |
| `Helpers/Roles.cs` | Role name constants |
| `Migrations/` | EF Core schema history (auto-generated) |
| `wwwroot/uploads/` | Saved files on disk (gitignored) |
| `Properties/launchSettings.json` | Local run profiles & ports |
| `Dockerfile` | Container build (currently stale — see README Known Issues) |
| `.github/workflows/dotnet.yml` | CI: restore + build on push/PR (has a typo to fix) |

---

## 11. Glossary

- **REST API** — a web service that exposes resources over HTTP using JSON.
- **ORM** — Object-Relational Mapper; maps C# objects to DB tables (here: EF Core).
- **Entity** — a C# class that maps to a database table.
- **DTO** — Data Transfer Object; the shape of data going in/out of the API.
- **DI (Dependency Injection)** — the framework supplies a class's dependencies via its
  constructor, instead of the class creating them with `new`.
- **Middleware** — components that process each HTTP request in a pipeline (auth, CORS, …).
- **Claim** — a piece of identity info inside a JWT (id, email, role).
- **JWT** — JSON Web Token; a signed, tamper-proof token proving who the user is.
- **Migration** — a recorded set of database schema changes generated from model changes.
- **Scoped / Singleton / Transient** — DI lifetimes (per-request / per-app / per-use).
- **Fluent API** — configuring EF Core relationships in code (`OnModelCreating`) instead of
  with attributes.

---

*For the assignment-upload feature specifically, continue to
[`AssignmentFeature.md`](AssignmentFeature.md).*
