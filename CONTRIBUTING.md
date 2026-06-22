# Contributing to HotelStay

Thank you for contributing. This document describes the repository standards, how to run tests locally, and the pull request process.

## Code style

- Follow the `.editorconfig` rules (4-space indentation, CRLF on Windows for code files).
- Use `var` when the type is apparent. Avoid `var` elsewhere.
- Private fields should be prefixed with an underscore (e.g. `_userStore`).
- Keep public API surface small and well-documented.

## Branching & commits

- Main branches:
  - `main` — protected, production-ready.
  - `dev` — active development branch.
- Create feature branches from `dev` using the pattern `feature/<short-description>`.
- Commit message style: `type(scope): short description` (e.g. `fix(auth): handle null refresh token`).

## Pull requests

- Open PR to `dev` when ready for review.
- Include a short description of the change and any relevant issue numbers.
- Add screenshots or recordings for UI changes where helpful.
- Ensure all checks pass (build + tests) before requesting review.

## Testing

### Backend

- From repository root:

  ```bash
  cd HotelStay.Api
  dotnet test
  ```

- Tests target .NET 8.

### Frontend

- From `hotel-stay-ui`:

  ```bash
  npm install
  npm start
  npm test
  ```

## CI expectations

- PRs must run `dotnet test` for the backend and `npm ci && npm test` for the frontend.
- Aim for high unit and integration test coverage before merging substantial changes.

## Local setup

- .NET SDK 8.0+, Node.js 20+, Angular CLI 17+. See `README.md` for more.

## Reviewing

- Prefer small PRs (~200–400 lines changed) for easier review.
- Reviewers should verify tests and run the app locally when behaviour changes are non-trivial.

## Questions

If you are unsure about any guideline, open an issue or ask a maintainer on the PR.
