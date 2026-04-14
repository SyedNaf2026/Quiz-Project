# QuizApp — Complete Concepts & Feature Guide

---

## 1. Role-Based Access Control

**Operation:** Frontend + Backend

**What:** 4 roles — QuizTaker, PremiumTaker, QuizCreator, GroupManager.

**Why:** Different users have different responsibilities. A QuizTaker should not be able to create quizzes. A QuizCreator should not be able to take quizzes. Separating roles ensures each user only sees and does what they're supposed to. This follows the **Principle of Least Privilege**.

**Frontend:** `authGuard` checks if JWT token exists in localStorage. `roleGuard` checks the role against allowed roles. Applied on every route in `app.routes.ts`. Navbar shows different links per role using `isCreator()`, `isTaker()`, `isGroupManager()`.

**Backend:** `[Authorize]` attribute on controllers. `[Authorize(Roles = "QuizCreator,GroupManager")]` restricts specific endpoints. JWT claims carry the role — `User.FindFirst(ClaimTypes.Role)`.

---

## 2. JWT Authentication

**Operation:** Backend (token generation) + Frontend (token storage & sending)

**What:** After login, a JWT token is issued and stored in localStorage. Every API request sends this token in the Authorization header.

**Why:** The server needs to know who is making the request without storing session data. JWT is stateless — the token itself contains the user's ID and role. The `auth-interceptor` automatically attaches it to every HTTP call so you don't repeat it everywhere.

**Frontend:** `auth-interceptor.ts` clones every HTTP request and adds `Authorization: Bearer {token}`. `AuthService` stores token in localStorage after login.

**Backend:** `AuthService.GenerateJwtToken()` creates the token with claims (Id, Email, Role). `Program.cs` configures JWT validation with `AddJwtBearer`.

---

## 3. Generic Repository Pattern

**Operation:** Backend

**What:** One `GenericRepository<T>` class handles `Add`, `Update`, `Delete`, `GetById`, `FindAsync` for any model.

**Why:** Without this, every service would write the same database code repeatedly. The repository centralizes data access. If you change the database later, you only change the repository. It also makes unit testing easy — you mock the repository instead of the real database.

**Where:** `GenericRepository.cs` — used in `AuthService`, `QuizService`, `CategoryService`, `QuestionService`, `UserService`.

---

## 4. Service Layer

**Operation:** Backend

**What:** All business logic lives in services, not controllers.

**Why:** Controllers should only receive requests and return responses. If you put logic in controllers, it becomes hard to test and maintain. Services are reusable — multiple controllers can call the same service method.

**Where:** `QuizService`, `AuthService`, `GroupService`, `NotificationService`, `CategoryService`, `LeaderboardService`, `QuizAttemptService`, `UserService`, `QuestionService`.

---

## 5. DTOs (Data Transfer Objects)

**Operation:** Backend (definition) + Frontend (matching interfaces)

**What:** Separate classes for input (`CreateQuizDTO`) and output (`QuizDTO`). Never expose the database model directly.

**Why:** The `User` model has `PasswordHash` — you never want that sent to the frontend. DTOs let you control exactly what data goes in and out. They also decouple your API from your database structure.

**Backend:** `Models/DTO/` folder — `CreateQuizDTO`, `QuizDTO`, `AuthResponseDTO`, `GroupDTOs`, `NotificationDTO` etc.

**Frontend:** `models/models.ts` — matching TypeScript interfaces for every DTO.

---

## 6. In-App Notifications with SignalR

**Operation:** Backend (hub + service) + Frontend (connection + display)

**What:** Real-time push notifications using WebSockets. When a quiz is created, all QuizTakers and PremiumTakers get notified instantly without refreshing.

**Why:** Polling (checking the server every few seconds) wastes resources. SignalR maintains a persistent connection and pushes data only when something happens. This gives a real-time experience like WhatsApp notifications.

**Backend:** `NotificationHub.cs` — SignalR hub. `NotificationService.cs` — persists to DB and pushes via `IHubContext`. Triggers: quiz created, updated, deactivated, group quiz assigned, leaderboard changed, group submission received.

**Frontend:** `notification.service.ts` — connects to hub, listens for `ReceiveNotification`, updates `BehaviorSubject`. Navbar subscribes and shows bell badge with unread count. Popups appear on login for unread notifications with redirect on click.

---

## 7. Group Manager Feature

**Operation:** Backend (service + controller) + Frontend (components)

**What:** GroupManager creates groups, adds QuizTaker members, assigns quizzes, and validates submissions.

**Why:** In real organizations, a manager needs to assign specific tasks (quizzes) to their team and review results. The validation feature means the manager can approve or reject a submission — useful for behavioral or subjective assessments.

**Backend:** `GroupService.cs`, `GroupController.cs` — 12 endpoints covering group CRUD, member management, quiz assignment, submission validation.

**Frontend:** `group-manager/dashboard` — list/create/delete groups. `group-manager/group-detail` — 4 tabs: Members, Quizzes, Create Quiz, Submissions.

**Key design decision:** Validation (`RequiresValidation = true`) only for quizzes the GroupManager created. Existing assigned quizzes show "Auto" — no manual review needed since answers are predefined.

---

## 8. Premium Upgrade with Simulated Payment

**Operation:** Backend (role change + new token) + Frontend (payment modal)

**What:** QuizTaker can upgrade to PremiumTaker via a payment modal. After payment, a new JWT token is issued with the updated role.

**Why:** Normal QuizTakers get one attempt per quiz — this prevents cheating by memorizing answers. PremiumTakers pay for unlimited attempts. The new JWT token is necessary because the old token still says `QuizTaker` — the role is embedded in the token, so a fresh token is required.

**Backend:** `UserService.UpgradeToPremiumAsync()` — changes role, generates new JWT. `PUT /api/user/upgrade-to-premium`.

**Frontend:** `profile.ts` — payment modal with card form, 2-second simulated processing, saves new token to localStorage, navigates to force navbar reload.

---

## 9. Excel Question Bank Upload

**Operation:** Backend (file parsing) + Frontend (file input)

**What:** QuizCreator and GroupManager can upload an `.xlsx` file with multiple questions at once.

**Why:** Adding 50 questions one by one through the UI is time-consuming. Excel upload lets you prepare questions offline and import them in one click. This is a real-world requirement for educational platforms.

**Backend:** `QuestionController` — `POST /api/question/bulk/{quizId}`. Uses **EPPlus** library to read Excel rows. Validates question type, minimum 2 options, at least one correct answer.

**Frontend:** `quiz-questions.html` and `gm-quiz-questions.html` — file input with `.xlsx` filter, upload button, template download button. Column format shown as a grid.

---

## 10. Leaderboard Date Filter

**Operation:** Backend (query filter) + Frontend (date picker UI)

**What:** Filter leaderboard by Today, This Week, This Month, This Year, All Time, or Custom Range.

**Why:** If the app runs for years, the leaderboard gets dominated by old scores. A new user who scores 100% today would never appear at the top. Date filtering makes the leaderboard fair and relevant.

**Backend:** `LeaderboardService.GetLeaderboardAsync(categoryId, fromDate, toDate)` — adds `.Where(r => r.CompletedAt >= fromDate)` and `.Where(r => r.CompletedAt <= toDate)` conditionally.

**Frontend:** `leaderboard.ts` — `getDateRange()` method calculates from/to dates using JavaScript `Date` object. `selectedPeriod` drives the dropdown. Custom range shows two date pickers.

---

## 11. Answer Review Feature

**Operation:** Backend (new endpoint) + Frontend (expandable panel)

**What:** In My Results, each quiz has a "📖 Review" button that shows which questions were wrong, what the user answered, and what the correct answer was.

**Why:** Learning from mistakes is the core purpose of a quiz app. Without this, users just see a score and move on. Showing wrong answers helps them understand what they need to study.

**Backend:** `QuizAttemptService.GetResultByQuizAsync()` — fetches `QuizResult`, loads `UserAnswers` and `Questions` with `Options`, rebuilds full answer breakdown. `GET /api/quizattempt/review/{quizId}`.

**Frontend:** `my-results.ts` — `toggleReview()` calls the API on demand. `my-results.html` — inline expandable row below each result showing only wrong answers with ❌ your answer and ✅ correct answer.

---

## 12. Form Validation

**Operation:** Frontend

**What:** Email must have a proper domain (`syed@g` is rejected). Username cannot contain spaces.

**Why:** `syed@g` is not a real email — it would cause issues with communication. Spaces in usernames cause problems in URLs and display. Validation at the frontend gives instant feedback without a server round trip.

**Frontend:** `register.ts` — `Validators.pattern(/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/)` for email. `Validators.pattern(/^\S+$/)` for no spaces. Error messages show per-error-type using `errors?.['pattern']`, `errors?.['required']`.

---

## 13. Quiz Delete Cleanup

**Operation:** Backend

**What:** When a quiz is deleted, all related data is removed first — GroupQuizResults, GroupQuizzes, QuizResults, UserAnswers — then the quiz.

**Why:** SQL Server enforces foreign key constraints with `OnDelete(DeleteBehavior.Restrict)`. If you delete a quiz that has results referencing it, the database throws a `DbUpdateException`. You must delete child records before parent records. The order matters — GroupQuizResults before GroupQuizzes, QuizResults before the Quiz.

**Backend:** `QuizService.DeleteQuizAsync()` — manually removes all dependent rows using `_context.RemoveRange()` before calling `_quizRepo.DeleteAsync(quiz)`.

---

## 14. Category Delete Protection

**Operation:** Backend

**What:** You cannot delete a category if any quiz is using it. A creator can only edit/delete their own categories.

**Why:** If you delete a category that quizzes depend on, those quizzes lose their category reference — breaking the data. Ownership check prevents one creator from modifying another's categories.

**Backend:** `CategoryService.DeleteCategoryAsync()` — checks `_context.Quizzes.AnyAsync(q => q.CategoryId == id)`. Checks `category.CreatedBy != userId`. `Category` model has `CreatedBy` (nullable int).

**Frontend:** `admin-categories.ts` — `getMyCategories()` loads only the logged-in creator's categories. Error messages from backend shown via `err?.error?.message`.

---

## 15. Newest Quiz on Top

**Operation:** Backend

**What:** Browse Quizzes shows the most recently created quiz first.

**Why:** Users want to see what's new. If a quiz creator just published something, it should be visible immediately at the top — not buried at the bottom after hundreds of older quizzes.

**Backend:** `QuizService.GetActiveQuizzesAsync()` — `OrderByDescending(q => q.CreatedAt)`.

---

## 16. Submit Button UX

**Operation:** Frontend

**What:** Submit button only appears on the last question. Confirmation dialog if questions are unanswered. 5-second answer review before result card.

**Why:** Accidental submission is a real problem in quiz apps. Showing the button only at the end prevents premature submission. The 5-second review gives users a moment to see their answers highlighted before the final score appears — better learning experience.

**Frontend:** `take-quiz.html` — submit button rendered only when `currentIndex === questions.length - 1`. `take-quiz.ts` — `submit(autoSubmit = false)` — confirmation skipped for timer auto-submit. `reviewCountdown = 5` drives the countdown interval before `showResult = true`.

---

## Additional Topics Used in the Project

---

### Code First Approach

**Operation:** Backend

You write C# model classes first, then EF Core generates the database tables from them using migrations. No manual SQL `CREATE TABLE` needed.

**Where:** All models in `Models/` folder → `AppDbContext.cs` registers them → `Add-Migration` + `Update-Database` creates the tables.

---

### Proper HTTP Methods

**Operation:** Backend

| Method | Purpose | Example in your project |
|--------|---------|------------------------|
| GET | Read data | Get quizzes, get profile, get leaderboard |
| POST | Create new | Register, create quiz, submit quiz |
| PUT | Full update | Update quiz, update profile, upgrade to premium |
| PATCH | Partial update | Toggle quiz active/inactive |
| DELETE | Remove | Delete quiz, remove group member |

---

### HTTP Status Codes

**Operation:** Backend

| Code | Meaning | Where used |
|------|---------|------------|
| 200 OK | Success | All successful responses |
| 400 Bad Request | Invalid input | Category in use, wrong role |
| 401 Unauthorized | Not logged in | Missing JWT token |
| 403 Forbidden | No permission | Wrong role on endpoint |
| 404 Not Found | Resource missing | Quiz not found |
| 500 Internal Server Error | Server crash | Caught by ExceptionMiddleware |

---

### SOLID Principles

**Operation:** Backend

- **S — Single Responsibility:** Each service does one thing. `AuthService` only handles auth. `QuizService` only handles quizzes.
- **O — Open/Closed:** `GenericRepository<T>` works for any model without modification.
- **L — Liskov Substitution:** Any mock can replace `IQuizService` in tests.
- **I — Interface Segregation:** Each service has its own focused interface — `IQuizService`, `IGroupService` etc.
- **D — Dependency Inversion:** Controllers depend on `IQuizService` (interface), not `QuizService` (concrete class).

---

### Dependency Injection

**Operation:** Backend

Services are registered in `Program.cs` with `AddScoped` and injected via constructors automatically by ASP.NET.

```csharp
builder.Services.AddScoped<IQuizService, QuizService>();
```

**Why:** You never write `new QuizService()` manually. ASP.NET creates and manages the lifetime. Makes testing easy — inject mocks instead of real services.

---

### Async/Await

**Operation:** Backend

Every method that touches the database uses `async/await`. This keeps the server thread free while waiting for database responses, allowing it to handle other requests simultaneously.

**Why:** Without async, one slow database query blocks the entire thread. With async, the thread is released while waiting and can serve other users.

---

### Swagger

**Operation:** Backend

Auto-generated interactive API documentation at `/swagger`. Reads controller attributes and generates documentation automatically. JWT support configured so you can test protected endpoints directly.

**Where:** `Program.cs` — `builder.Services.AddSwaggerGen()`, `app.UseSwagger()`.

---

### Exception Middleware

**Operation:** Backend

`ExceptionMiddleware.cs` wraps the entire request pipeline. Any unhandled exception is caught, logged, and returns a clean 500 response instead of crashing the server.

**Where:** `app.UseMiddleware<ExceptionMiddleware>()` — first in the pipeline in `Program.cs`.

---

### Angular Standalone Components

**Operation:** Frontend

Every component has `standalone: true` and manages its own `imports`. No `NgModule` needed.

**Why:** Simpler architecture. Each component is self-contained. Lazy loading works directly with `loadComponent` in routes.

---

### Reactive Forms

**Operation:** Frontend

Used in: Login, Register, Create Quiz, Create Group, Admin Categories.

Form structure defined in TypeScript using `FormBuilder`. Validators attached in code. Full programmatic control.

```typescript
this.form = this.fb.group({
  email: ['', [Validators.required, Validators.pattern(...)]],
  password: ['', Validators.required]
});
```

---

### Template-driven Forms

**Operation:** Frontend

Used in: Quiz Questions (question text input), Group Detail (member search), Leaderboard (date pickers).

Simpler forms controlled directly in HTML using `[(ngModel)]`. No `FormBuilder` needed.

---

### RxJS Observables

**Operation:** Frontend

All HTTP calls return `Observable<T>`. Components subscribe with `next`, `error` callbacks. `BehaviorSubject` in `NotificationService` holds the notification list and emits updates to all subscribers (navbar).

---

### Angular Guards

**Operation:** Frontend

- `authGuard` — checks `localStorage.getItem('JWT-token')`. Redirects to `/login` if missing.
- `roleGuard` — checks `localStorage.getItem('user-role')` against allowed roles. Redirects to `/home` if unauthorized.

---

### Lazy Loading

**Operation:** Frontend

Routes use `loadComponent` — components are only downloaded when the user navigates to that route, not at startup. Makes the initial load faster.

```typescript
loadComponent: () => import('./taker/browse/browse-quizzes').then(m => m.BrowseQuizzes)
```

---

### Inter-Component Communication

**Operation:** Frontend

- **`@Input()`** — Parent passes data to child. Used in `ConfirmDialogComponent` — `[visible]`, `[title]`, `[message]`.
- **`@Output()` + `EventEmitter`** — Child fires event to parent. `(confirm)="doDelete()"`, `(cancel)="showConfirm = false"`.
- **Service as shared state** — `NotificationService.notifications$` is a `BehaviorSubject`. Navbar subscribes to it. When SignalR pushes a notification, the bell badge updates everywhere automatically.

---

### ES6 Features Used

**Operation:** Frontend

| Feature | Where used |
|---------|-----------|
| Arrow functions `() =>` | All subscribe callbacks, map, filter |
| Destructuring | `const { from, to } = this.getDateRange()` |
| Spread operator | `[newCategory, ...this.categories]` |
| Template literals | Toast messages, notification messages |
| Optional chaining `?.` | `res.data?.title`, `err?.error?.message` |
| `Array.filter()` | Filtering quizzes, wrong answers |
| `Array.map()` | Transforming data for display |
| `Array.find()` | Finding category by id |
| `Set` | Multi-answer option tracking in take-quiz |
| `new Date()` methods | Leaderboard date range calculation |

---

### Unit Testing

**Operation:** Backend

**Tools:** xUnit, Moq, EF Core InMemory database.

**Total: 106 tests, all passing, ~94% coverage on services.**

Services tested: AuthService, CategoryService, LeaderboardService, QuestionService, QuizAttemptService, QuizService, UserService, GroupService, NotificationService.

**Pattern:**
```csharp
[Fact]
public async Task CreateQuiz_CategoryNotFound_ReturnsFail()
{
    // Arrange — set up
    // Act — run the code
    // Assert — verify result
}
```

---

### Dark Mode

**Operation:** Frontend

CSS variables switch when `data-theme="dark"` is set on `<html>`. Navbar toggle button calls `applyTheme()` which sets the attribute. All colors reference `var(--primary)`, `var(--card-bg)`, `var(--text)` etc.

---

### Certificate Generation

**Operation:** Frontend

After completing a quiz, users can download a PDF certificate. Built using **jsPDF** library — draws borders, text, and layout programmatically. Opens in a new browser window and auto-prints.

**Where:** `take-quiz.ts` — `downloadCertificate()`. Also available in `my-results.ts`.

---

### Naming Conventions

**Operation:** Backend + Frontend

| Thing | Convention | Example |
|-------|-----------|---------|
| C# Classes | PascalCase | `QuizService`, `AppDbContext` |
| C# Interfaces | I + PascalCase | `IQuizService`, `IGroupService` |
| C# Methods | PascalCase | `CreateQuizAsync`, `GetByIdAsync` |
| C# Private fields | _camelCase | `_quizRepo`, `_context` |
| DTOs | PascalCase + DTO | `CreateQuizDTO`, `QuizResultDTO` |
| Angular Components | PascalCase | `BrowseQuizzes`, `MyResults` |
| Angular Services | camelCase file | `quiz.service.ts`, `group.service.ts` |
| Angular Variables | camelCase | `selectedPeriod`, `reviewLoading` |

---

### Premium Retake Cooldown (24-Hour Rule)

**Operation:** Backend

**What:** PremiumTakers can retake any quiz but must wait 24 hours between attempts on the same quiz.

**Why:** Without this, a PremiumTaker could retake a quiz immediately after seeing the answers, score 100% every time, and permanently dominate the leaderboard. The 24-hour cooldown prevents memorization abuse while still giving the premium benefit of multiple attempts. The retake feature is for learning — not for gaming the leaderboard.

**How it works:** The `QuizResult` model already stores `CompletedAt` timestamp for every attempt. When a PremiumTaker submits a quiz they've already taken, the service checks the time difference between now and their last attempt:

```csharp
var hoursSince = (DateTime.UtcNow - lastAttempt.CompletedAt).TotalHours;
if (hoursSince < 24)
{
    var hoursLeft = (int)Math.Ceiling(24 - hoursSince);
    return (false, $"You can retake this quiz after {hoursLeft} more hour(s).", null);
}
```

If less than 24 hours have passed, the submission is blocked with a clear message showing exactly how many hours remain.

**Where:** `QuizAttemptService.SubmitQuizAsync()` — checked before processing answers. No model change, no migration needed — `CompletedAt` already existed.

**Frontend:** No change needed — the error message from the backend automatically shows as a toast notification via the existing error handler.
