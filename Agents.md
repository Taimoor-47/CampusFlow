# CampusFlow development workflow

## Source of truth

- Read `FEATURES_AND_AI_ROADMAP.md` before proposing or implementing features.
- Inspect the relevant frontend, backend, database model, migrations, and API contracts before planning.
- Verify documented claims against the current code because the roadmap may be outdated.
- Preserve unrelated user changes.

## Feature workflow

Work on exactly one approved feature at a time.

For every feature:

1. Inspect the current implementation without editing.
2. Explain:
   - what currently works;
   - what is missing or unsafe;
   - why the feature should be implemented;
   - frontend, backend, database, authorization, and testing impact.
3. Propose the smallest useful implementation chunks.
4. Stop and wait for the user's approval before implementation.
5. Implement only the approved chunk.
6. Run relevant builds, tests, type checks, lint, migration checks, and security checks.
7. Review the resulting diff for correctness, security, unnecessary complexity, and contract mismatches.
8. Update `FEATURES_AND_AI_ROADMAP.md` with verified results.
9. Stop for user review. Do not automatically begin the next feature.

## Git and review policy

- Never run `git add`, `git commit`, `git push`, `git merge`, `git rebase`, `git tag`, create a pull request, or modify remote branches unless the user explicitly requests that exact Git action in the current message.
- “Implement,” “fix,” “continue,” or “complete the feature” does not authorize committing or pushing.
- All code must remain as an uncommitted local diff until the user reviews it.
- Before requesting review, report:
  - files changed;
  - important design decisions;
  - tests and commands run;
  - test results;
  - migrations created or changed;
  - known limitations;
  - the exact next recommended action.
- A reviewed feature may be committed only after the user explicitly says to commit it.
- Push only after the user explicitly says: “Push the reviewed changes.”
- Never push directly to the main or master branch.

## Implementation standards

- Prefer the smallest change that completely satisfies the approved acceptance criteria.
- Do not rewrite unrelated code.
- Do not add speculative abstractions, frameworks, dependencies, or generic helper layers.
- Reuse established project patterns where they are sound.
- Follow SOLID principles where they improve cohesion, testability, or dependency boundaries.
- Keep classes and methods focused on one responsibility.
- Prefer composition and dependency injection over hidden global/static dependencies.
- Keep business rules in services or domain components, not controllers or UI components.
- Keep database access in repositories/data services and use explicit DTO projections.
- Never return EF entities, password hashes, secrets, navigation graphs, SQL errors, or stack traces.
- Obtain user identity from the authenticated server context, never from a client-supplied user ID when authorization depends on identity.
- Keep frontend and backend types, routes, status codes, and multipart/JSON contracts aligned.
- Avoid duplicated logic and premature optimization.
- Optimize database queries by selecting only required columns and avoiding unnecessary tracking or queries.
- Write readable code first; optimize based on an identifiable cost or performance problem.
- Add comments only when they explain a non-obvious decision.
- Do not introduce breaking API or database changes without highlighting them before implementation.

## Definition of done

A feature is complete only when:

- its frontend, backend, DTO, authorization, and database contracts align;
- expected success and failure states are handled;
- relevant automated or manual verification passes;
- no sensitive or unauthorized information is exposed;
- the roadmap is updated with verified evidence;
- the user has reviewed the local diff.

Code completion does not authorize commit or push.