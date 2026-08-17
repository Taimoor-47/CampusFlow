# Assignment Upload Feature — Documentation

A beginner-friendly guide to how the "teacher uploads assignments / students submit
files" feature works in CampusFlow.

---

## 1. The big picture

The feature lets:

- **Teachers** create an assignment for the class and optionally attach a brief file (PDF/Word/etc.).
- **Students** see all assignments and upload their own file as a *submission*.
- **Teachers** view every student's submitted file for an assignment.

Files are saved on the server's **local disk** (under `wwwroot/uploads/`). The database
only stores the **URL** of each file — not the file bytes. This keeps the database small
and fast.

```
Teacher ──uploads brief──►  /uploads/assignments/abc.pdf   (file on disk)
                            Assignments table stores "/uploads/assignments/abc.pdf"

Student ──uploads work──►   /uploads/submissions/xyz.pdf   (file on disk)
                            Submissions table stores "/uploads/submissions/xyz.pdf"
```

---

## 2. The layered architecture (very important to understand)

This project uses a classic **3-layer architecture**. Each layer only talks to the one
directly below it. This is the single most important concept in the codebase.

```
   HTTP request (from browser / Postman)
        │
        ▼
┌──────────────────┐
│   Controller     │   Handles HTTP: routes, status codes, reads the logged-in user.
│  (StudentController,│   Knows NOTHING about the database.
│   TeacherController)│
└──────────────────┘
        │ calls
        ▼
┌──────────────────┐
│    Service       │   Business rules: "has this student already submitted?",
│ (StudentService, │   "save the file before writing the DB row".
│  TeacherService) │   Knows NOTHING about HTTP.
└──────────────────┘
        │ calls
        ▼
┌──────────────────┐
│   Repository     │   The ONLY place that touches the database (AppDbContext).
│ (StudentRepository)│   Pure data in / data out.
└──────────────────┘
        │ uses
        ▼
┌──────────────────┐
│  AppDbContext    │   Entity Framework Core — maps C# classes to SQL tables.
└──────────────────┘
        │
        ▼
   SQL Server database
```

**Why bother with layers?** So each piece has one job. If you want to change *how* data
is stored, you only touch the Repository. If you want to change a *rule*, you only touch
the Service. The Controller never has to change.

> Note: `TeacherService` currently talks to `AppDbContext` directly (it doesn't use a
> repository yet). `StudentService` uses a repository. Both styles work; the student side
> is the "cleaner" example to learn from.

---

## 3. The data model (the C# classes that become DB tables)

### `Assignment` — created by a teacher, for the whole class
```csharp
public class Assignment
{
    public Guid Id { get; set; }            // primary key
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string? FilePath { get; set; }   // URL of the teacher's brief, e.g. "/uploads/assignments/abc.pdf"
                                             // The "?" means it can be null (no file attached)
    public Guid? TeacherId { get; set; }     // which teacher created it
    public List<Submission> Submissions { get; set; }  // all student submissions for this assignment
}
```

### `Submission` — one student's uploaded file for one assignment
```csharp
public class Submission
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }   // which assignment this answers (foreign key)
    public Guid StudentId { get; set; }      // who submitted it (foreign key)
    public string FilePath { get; set; }     // URL of the student's file
    public DateTime SubmittedAt { get; set; }
}
```

### The relationship
```
   Assignment (1) ───────< (many) Submission (many) >─────── (1) Student
       "one assignment has many submissions"        "one student has many submissions"
```

A **unique index** on `(AssignmentId, StudentId)` enforces the rule:
*a student can submit a given assignment only once.* This is set up in `AppDbContext.cs`.

---

## 4. How a file actually gets saved: `FileStorageService`

Rather than scatter file-saving code everywhere, it lives in one reusable service.

**`IFileStorageService`** (the interface — the "contract"):
```csharp
Task<string> SaveAsync(IFormFile file, string subFolder);
// Give it an uploaded file + a folder name → it returns the public URL.
```

**`FileStorageService`** (the implementation) does 4 things:
1. **Validates** the file: not empty, under 25 MB, and an allowed extension (.pdf, .docx, …).
2. **Creates** the target folder if it doesn't exist.
3. **Renames** the file to a random GUID (e.g. `a1b2c3.pdf`) so two uploads never collide
   and nobody can overwrite someone else's file by guessing names.
4. **Returns** the URL (e.g. `/uploads/submissions/a1b2c3.pdf`).

> **Why an interface?** If you later move to cloud storage (Azure/AWS), you write a new
> class implementing `IFileStorageService` and change ONE line in `Program.cs`. No other
> code changes. This is called *dependency inversion*.

It's registered in `Program.cs`:
```csharp
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
```
`AddSingleton` = one shared instance for the whole app (fine, because it holds no per-user state).

---

## 5. Walkthrough: Teacher uploads an assignment

**Endpoint:** `POST /api/teacher/upload-assignment`
**Body:** `multipart/form-data` (because it carries a file) with fields `Title`,
`Description`, `DueDate`, and optional `File`.

Step by step:

1. **`TeacherController.UploadAssignment`** receives the form via `[FromForm] AddAssignmentDto dto`.
   - `[FromForm]` (not `[FromBody]`) is required because files can't be sent as JSON.
   - It reads the teacher's ID from the login token: `GetTeacherId()`.
2. It calls **`_teacherService.AddAssignment(dto, teacherId)`**.
3. **`TeacherService.AddAssignment`**:
   - If a file was attached, calls `_fileStorage.SaveAsync(dto.File, "uploads/assignments")`
     → file written to disk, URL returned.
   - Builds an `Assignment` object and saves it to the DB.
4. Controller returns `200 OK` with the new assignment's details.

```
Browser ──(form + file)──► Controller ──► Service ──┬─► FileStorageService (writes file)
                                                     └─► AppDbContext (writes DB row)
```

If the file is too big or the wrong type, `SaveAsync` throws `ArgumentException`, and the
controller turns that into a `400 Bad Request`. **The file is saved before the DB row**, so
we never end up with a database row pointing at a file that doesn't exist.

---

## 6. Walkthrough: Student submits their work

**Endpoint:** `POST /api/student/assignments/{assignmentId}/submit`
**Body:** `multipart/form-data` with a single `file` field.

1. **`StudentController.SubmitAssignment`** reads the logged-in student's ID from the token.
2. Calls **`_studentService.SubmitAssignment(assignmentId, studentId, file)`**.
3. **`StudentService.SubmitAssignment`** applies the business rules:
   - Does the assignment exist? If not → `KeyNotFoundException` → `404 Not Found`.
   - Has this student already submitted? If yes → `InvalidOperationException` → `409 Conflict`.
   - Save the file, then create the `Submission` row.
4. Returns `200 OK` with the submission details.

Notice how the **Service decides the rules** and the **Controller decides the HTTP status
code** for each kind of failure. That separation is the whole point of the layers.

---

## 7. Walkthrough: Student views assignments (with submission status)

**Endpoint:** `GET /api/student/my-assignments`

This one is interesting because it **combines two data sources**. The student should see
*every* assignment, plus whether *they personally* have submitted it.

`StudentService.GetMyAssignments`:
1. Get **all** assignments.
2. Get **this student's** submissions.
3. For each assignment, look up whether the student has a matching submission, and build a
   `StudentAssignmentDto` that merges both.

```csharp
return new StudentAssignmentDto
{
    Id = a.Id, Title = a.Title, DueDate = a.DueDate,
    FilePath = a.FilePath,                  // teacher's brief
    Submitted = submission is not null,     // did I submit?
    SubmissionFilePath = submission?.FilePath, // my file (if any)
    SubmittedAt = submission?.SubmittedAt
};
```

> **What's a DTO?** *Data Transfer Object.* It's a class shaped exactly for what the API
> returns, instead of exposing raw database entities. Here, `StudentAssignmentDto` is a
> clean "view" combining assignment + submission info.

---

## 8. Serving the files back to the browser

In `Program.cs`:
```csharp
app.UseStaticFiles();   // serve uploaded files from wwwroot
```
This line lets the browser open a file directly by its URL. Because files live under
`wwwroot`, a saved URL like `/uploads/submissions/a1b2c3.pdf` becomes downloadable at
`https://yourserver/uploads/submissions/a1b2c3.pdf`.

---

## 9. The database migration

Changing the C# model classes does **not** change the SQL database by itself. You create a
*migration* (a recorded set of schema changes) and then apply it.

A migration named `AddAssignmentFilesAndSubmissions` was already created. To apply it to
your database, run from the `CampusFlow/CampusFlow` folder:

```bash
dotnet ef database update
```

⚠️ This migration is **destructive**: it drops the old `Assignments.StudentId` column
(assignments are now class-wide, not per-student) and creates the new `Submissions` table.
If you have old assignment data you care about, back it up first.

---

## 10. Endpoint cheat-sheet

| Who | Method & route | Purpose |
|-----|----------------|---------|
| Teacher | `POST /api/teacher/upload-assignment` | Create an assignment (+ optional brief file) |
| Teacher | `GET /api/teacher/assignments/{id}/submissions` | List all student submissions for an assignment |
| Student | `GET /api/student/my-assignments` | See all assignments + my submission status |
| Student | `POST /api/student/assignments/{id}/submit` | Upload my file for an assignment |

---

## 11. Files involved (where to look)

| Layer | File | What it does |
|-------|------|--------------|
| Model | `Model/Assignment.cs` | Assignment entity (now has `FilePath`, `TeacherId`) |
| Model | `Model/Submission.cs` | **New** — student's uploaded file record |
| DTO | `DTO/AddAssignmentDto.cs` | Shape of the teacher's upload form |
| DTO | `DTO/StudentAssignmentDto.cs` | **New** — what the student sees per assignment |
| Service | `Services/IFileStorageService.cs` | **New** — file-saving contract |
| Service | `Services/FileStorageService.cs` | **New** — saves files to disk, validates them |
| Service | `Services/TeacherService.cs` | `AddAssignment`, `GetSubmissions` |
| Service | `Services/StudentService.cs` | `GetMyAssignments`, `SubmitAssignment` |
| Repository | `Repositories/StudentRepository.cs` | DB queries for assignments & submissions |
| Controller | `Controllers/TeacherController.cs` | Teacher endpoints |
| Controller | `Controllers/StudentController.cs` | Student endpoints |
| Config | `Data/AppDbContext.cs` | Table definitions + relationships + unique index |
| Config | `Program.cs` | Registers services, `UseStaticFiles()` |

---

## 12. Key concepts you just learned

- **Layered architecture**: Controller → Service → Repository → DbContext, each with one job.
- **Dependency Injection**: services are registered in `Program.cs` and "injected" into
  constructors, instead of created with `new`. (See every constructor that takes an interface.)
- **DTOs**: purpose-built classes for API input/output, separate from DB entities.
- **`IFormFile`**: ASP.NET's type for an uploaded file.
- **Exceptions → HTTP status codes**: the service throws meaningful exceptions; the
  controller maps each to the right code (404 / 409 / 400).
- **EF Core migrations**: how C# model changes become real database schema changes.
