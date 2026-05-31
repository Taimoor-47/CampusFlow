# Graph Report - .  (2026-05-31)

## Corpus Check
- Corpus is ~4,337 words - fits in a single context window. You may not need a graph.

## Summary
- 349 nodes · 411 edges · 41 communities (22 shown, 19 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Launch & Environment Config|Launch & Environment Config]]
- [[_COMMUNITY_Teacher Service & Data Access|Teacher Service & Data Access]]
- [[_COMMUNITY_Teacher API Endpoints|Teacher API Endpoints]]
- [[_COMMUNITY_Teacher Service Interface|Teacher Service Interface]]
- [[_COMMUNITY_Student API Endpoints|Student API Endpoints]]
- [[_COMMUNITY_Student Repository|Student Repository]]
- [[_COMMUNITY_EF Core Migrations|EF Core Migrations]]
- [[_COMMUNITY_Student Service Implementation|Student Service Implementation]]
- [[_COMMUNITY_Student Repository Interface|Student Repository Interface]]
- [[_COMMUNITY_Student Service Interface|Student Service Interface]]
- [[_COMMUNITY_Auth & Misc Controllers|Auth & Misc Controllers]]
- [[_COMMUNITY_App Configuration (JWT & DB)|App Configuration (JWT & DB)]]
- [[_COMMUNITY_NuGet Dependencies|NuGet Dependencies]]
- [[_COMMUNITY_Database Context|Database Context]]
- [[_COMMUNITY_EF Model Snapshot|EF Model Snapshot]]
- [[_COMMUNITY_JWT Token Service|JWT Token Service]]
- [[_COMMUNITY_Dev Logging Config|Dev Logging Config]]
- [[_COMMUNITY_Password Hashing|Password Hashing]]
- [[_COMMUNITY_Migration Designer (Initial)|Migration Designer (Initial)]]
- [[_COMMUNITY_Migration Designer (Teacher)|Migration Designer (Teacher)]]
- [[_COMMUNITY_Migration Designer (Roles)|Migration Designer (Roles)]]
- [[_COMMUNITY_Role Constants|Role Constants]]
- [[_COMMUNITY_Assignment Input DTO|Assignment Input DTO]]
- [[_COMMUNITY_GPA Input DTO|GPA Input DTO]]
- [[_COMMUNITY_Login DTO|Login DTO]]
- [[_COMMUNITY_Teacher Registration DTO|Teacher Registration DTO]]
- [[_COMMUNITY_Course Enrollment Model|Course Enrollment Model]]
- [[_COMMUNITY_Student Model|Student Model]]
- [[_COMMUNITY_Teacher Model|Teacher Model]]
- [[_COMMUNITY_Unused Template Code|Unused Template Code]]
- [[_COMMUNITY_Local Permissions Config|Local Permissions Config]]
- [[_COMMUNITY_Schedule Input DTO|Schedule Input DTO]]
- [[_COMMUNITY_Grade Response DTO|Grade Response DTO]]
- [[_COMMUNITY_Grade Card DTO|Grade Card DTO]]
- [[_COMMUNITY_Semester Result DTO|Semester Result DTO]]
- [[_COMMUNITY_Student Registration DTO|Student Registration DTO]]
- [[_COMMUNITY_Assignment Model|Assignment Model]]
- [[_COMMUNITY_Course Model|Course Model]]
- [[_COMMUNITY_Schedule Model|Schedule Model]]
- [[_COMMUNITY_Student GPA Model|Student GPA Model]]

## God Nodes (most connected - your core abstractions)
1. `StudentController` - 11 edges
2. `TeacherController` - 10 edges
3. `StudentRepository` - 10 edges
4. `TeacherService` - 9 edges
5. `IStudentRepository` - 8 edges
6. `StudentService` - 8 edges
7. `Task` - 7 edges
8. `Task` - 7 edges
9. `ITeacherService` - 7 edges
10. `IActionResult` - 6 edges

## Surprising Connections (you probably didn't know these)
- `StudentController` --inherits--> `ControllerBase`  [EXTRACTED]
  CampusFlow/Controllers/StudentController.cs →   _Bridges community 10 → community 4_
- `TeacherController` --inherits--> `ControllerBase`  [EXTRACTED]
  CampusFlow/Controllers/TeacherController.cs →   _Bridges community 10 → community 2_

## Import Cycles
- None detected.

## Communities (41 total, 19 thin omitted)

### Community 0 - "Launch & Environment Config"
Cohesion: 0.07
Nodes (31): commandName, environmentVariables, launchUrl, publishAllPorts, useSSL, ASPNETCORE_ENVIRONMENT, ASPNETCORE_HTTP_PORTS, ASPNETCORE_HTTPS_PORTS (+23 more)

### Community 1 - "Teacher Service & Data Access"
Cohesion: 0.11
Nodes (16): AddAssignmentDto, AddGpaDto, AddScheduleDto, AppDbContext, Assignment, List, LoginDto, RegisterTeacherDto (+8 more)

### Community 2 - "Teacher API Endpoints"
Cohesion: 0.18
Nodes (14): AddAssignmentDto, AddGpaDto, AddScheduleDto, Authorize, HttpGet, HttpPost, IActionResult, ITeacherService (+6 more)

### Community 3 - "Teacher Service Interface"
Cohesion: 0.12
Nodes (14): AddAssignmentDto, AddGpaDto, AddScheduleDto, Assignment, List, LoginDto, RegisterTeacherDto, Schedules (+6 more)

### Community 4 - "Student API Endpoints"
Cohesion: 0.21
Nodes (12): Authorize, Guid, HttpGet, HttpPost, IActionResult, IStudentService, JwtServicescs, LoginDto (+4 more)

### Community 5 - "Student Repository"
Cohesion: 0.17
Nodes (12): AppDbContext, Assignment, Guid, IReadOnlyList, List, Schedules, Student, StudentGPA (+4 more)

### Community 6 - "EF Core Migrations"
Cohesion: 0.12
Nodes (10): MigrationBuilder, MigrationBuilder, MigrationBuilder, Migration, CampusFlow.Migrations, InitialCleanSetup, AddTeacherRoleSystem, CampusFlow.Migrations (+2 more)

### Community 7 - "Student Service Implementation"
Cohesion: 0.16
Nodes (13): Assignment, Guid, IReadOnlyList, IStudentRepository, LoginDto, Schedules, Student, StudentDto (+5 more)

### Community 8 - "Student Repository Interface"
Cohesion: 0.20
Nodes (10): Assignment, Guid, IReadOnlyList, List, Schedules, Student, StudentGPA, Task (+2 more)

### Community 9 - "Student Service Interface"
Cohesion: 0.18
Nodes (11): Assignment, Guid, IReadOnlyList, LoginDto, Schedules, Student, StudentDto, StudentGPA (+3 more)

### Community 10 - "Auth & Misc Controllers"
Cohesion: 0.13
Nodes (11): HttpPost, IActionResult, HttpGet, string, ControllerBase, AuthController, CampusFlow.Controllers, CampusFlow.Controllers (+3 more)

### Community 11 - "App Configuration (JWT & DB)"
Cohesion: 0.15
Nodes (12): AllowedHosts, ConnectionStrings, DefaultConnection, Jwt, Audience, DurationInMinutes, Issuer, Key (+4 more)

### Community 12 - "NuGet Dependencies"
Cohesion: 0.22
Nodes (8): net8.0, Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0), Microsoft.EntityFrameworkCore (8.0.0), Microsoft.EntityFrameworkCore.SqlServer (8.0.0), Microsoft.EntityFrameworkCore.Tools (8.0.0), Microsoft.VisualStudio.Azure.Containers.Tools.Targets (1.23.0), Swashbuckle.AspNetCore (10.1.7), Microsoft.NET.Sdk.Web

### Community 13 - "Database Context"
Cohesion: 0.33
Nodes (4): ModelBuilder, AppDbContext, CampusFlow.Data, DbContext

### Community 14 - "EF Model Snapshot"
Cohesion: 0.33
Nodes (4): ModelBuilder, AppDbContextModelSnapshot, CampusFlow.Migrations, ModelSnapshot

### Community 15 - "JWT Token Service"
Cohesion: 0.33
Nodes (4): Guid, IConfiguration, CampusFlow.Services, JwtServicescs

### Community 16 - "Dev Logging Config"
Cohesion: 0.40
Nodes (4): Logging, LogLevel, Default, Microsoft.AspNetCore

### Community 18 - "Migration Designer (Initial)"
Cohesion: 0.40
Nodes (3): ModelBuilder, CampusFlow.Migrations, InitialCleanSetup

### Community 19 - "Migration Designer (Teacher)"
Cohesion: 0.40
Nodes (3): ModelBuilder, AddTeacherRoleSystem, CampusFlow.Migrations

### Community 20 - "Migration Designer (Roles)"
Cohesion: 0.40
Nodes (3): ModelBuilder, CampusFlow.Migrations, RoleBasedCleanup

### Community 21 - "Role Constants"
Cohesion: 0.50
Nodes (3): string, CampusFlow.Helpers, Roles

## Knowledge Gaps
- **161 isolated node(s):** `allow`, `Default`, `Microsoft.AspNetCore`, `Default`, `Microsoft.AspNetCore` (+156 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **19 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TeacherController` connect `Teacher API Endpoints` to `Auth & Misc Controllers`?**
  _High betweenness centrality (0.013) - this node is a cross-community bridge._
- **Why does `StudentController` connect `Student API Endpoints` to `Auth & Misc Controllers`?**
  _High betweenness centrality (0.013) - this node is a cross-community bridge._
- **What connects `allow`, `Default`, `Microsoft.AspNetCore` to the rest of the system?**
  _161 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Launch & Environment Config` be split into smaller, more focused modules?**
  _Cohesion score 0.06653225806451613 - nodes in this community are weakly interconnected._
- **Should `Teacher Service & Data Access` be split into smaller, more focused modules?**
  _Cohesion score 0.11067193675889328 - nodes in this community are weakly interconnected._
- **Should `Teacher Service Interface` be split into smaller, more focused modules?**
  _Cohesion score 0.12380952380952381 - nodes in this community are weakly interconnected._
- **Should `EF Core Migrations` be split into smaller, more focused modules?**
  _Cohesion score 0.12280701754385964 - nodes in this community are weakly interconnected._