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

---

### Category Ownership — Who Can Edit/Delete

**Operation:** Backend + Frontend

**What:** Every category stores who created it (`CreatedBy` field). A QuizCreator can only edit or delete their own categories. Other creators' categories are visible but protected.

**Why:** Without ownership, any QuizCreator could delete or rename another creator's category — breaking all quizzes that use it. Ownership ensures each creator is responsible only for what they created.

**Backend:**
- `Category` model — added `CreatedBy` (nullable int) and `Creator` navigation property
- `AppDbContext` — configured relationship with `OnDelete(DeleteBehavior.SetNull)` so if a creator is deleted, the category remains but loses its owner
- `CategoryService.CreateCategoryAsync(dto, createdBy)` — saves the creator's userId
- `CategoryService.UpdateCategoryAsync(id, dto, userId)` — checks `category.CreatedBy != userId` → returns "You can only edit your own categories."
- `CategoryService.DeleteCategoryAsync(id, userId)` — same ownership check + checks if any quiz uses it
- `CategoryDTO` — includes `CreatedBy` field so frontend knows who owns each category
- Migration: `AddCategoryCreatedBy`

**Frontend:**
- `CategoryDTO` interface — added `createdBy?: number`
- `admin-categories.ts` — loads current user's profile on init to get `currentUserId`. `isOwner(cat)` method checks `cat.createdBy === currentUserId`
- `admin-categories.html` — Edit/Delete buttons only render when `isOwner(cat)` is true. Other creators' categories show `—` in the Actions column
- Error handlers read `err?.error?.message` to show the exact backend message as a toast

---

### Quiz Attempt Status on Browse Page (Cooldown Visibility)

**Operation:** Backend + Frontend

**What:** Before a user even clicks a quiz, the Browse Quizzes page shows their attempt status — "🔒 Already Attempted" for QuizTakers, "⏳ Cooldown — Xh left" for PremiumTakers in cooldown.

**Why:** Previously users had to go through the entire quiz, answer all questions, and only find out at submission time that they couldn't submit. This wasted their time. Showing the status upfront on the browse card is a much better user experience.

**Backend:**
- New DTO `QuizAttemptStatusDTO { QuizId, Status, HoursRemaining }`
- `IQuizAttemptService` — added `GetUserAttemptStatusAsync(userId, role)`
- `QuizAttemptService.GetUserAttemptStatusAsync` — queries all `QuizResults` for the user, groups by `QuizId`, checks the latest `CompletedAt`. For `QuizTaker` → status `"attempted"`. For `PremiumTaker` within 24 hours → status `"cooldown"` with hours remaining
- `QuizAttemptController` — new `GET /api/quizattempt/attempt-status` endpoint

**Frontend:**
- `QuizAttemptStatusDTO` interface added to `models.ts`
- `quiz-attempt.service.ts` — added `getAttemptStatus()` calling the new endpoint
- `browse-quizzes.ts` — loads attempt status on init for both `QuizTaker` and `PremiumTaker`. Stores in `Map<quizId, status>`. `getAttemptStatus(quizId)` helper method
- `browse-quizzes.html` — three states on each quiz card:
  - QuizTaker who attempted → 🔒 Already Attempted + ⭐ Go Premium button
  - PremiumTaker in cooldown → ⏳ Cooldown — Xh left badge
  - Available → Start Quiz button

---

### Quiz Delete — Cascade Cleanup

**Operation:** Backend

**What:** When a quiz is deleted, all dependent data is removed first in the correct order before the quiz itself is deleted.

**Why:** SQL Server enforces foreign key constraints with `OnDelete(DeleteBehavior.Restrict)`. Deleting a quiz directly would throw `DbUpdateException` because other tables still reference it. The cleanup must happen in the right order — child records before parent records.

**Order of deletion in `QuizService.DeleteQuizAsync`:**
1. `GroupQuizResults` — submissions by group members (references GroupQuizzes)
2. `GroupQuizzes` — group assignments (references Quiz)
3. `QuizResults` — individual attempt results (references Quiz)
4. `UserAnswers` — individual answers (references Quiz)
5. Quiz deleted — now safe
6. Questions + Options — cascade automatically (configured with `OnDelete(DeleteBehavior.Cascade)`)

---

### Newest Quiz Appears First

**Operation:** Backend

**What:** Browse Quizzes always shows the most recently created quiz at the top.

**Why:** When a QuizCreator publishes a new quiz, it should be immediately visible at the top of the list — not buried after hundreds of older quizzes. Users naturally expect newest content first.

**Where:** `QuizService.GetActiveQuizzesAsync()` — added `.OrderByDescending(q => q.CreatedAt)` to the LINQ query.

---

### PremiumTaker Notifications

**Operation:** Backend

**What:** PremiumTakers receive the same in-app notifications as QuizTakers — new quiz added, quiz updated, quiz deactivated, leaderboard changes.

**Why:** When a user upgrades to PremiumTaker, their role changes from `QuizTaker` to `PremiumTaker`. The original `SendToAllTakersAsync` only queried `Role == "QuizTaker"` — so PremiumTakers were silently excluded from all notifications after upgrading.

**Fix:** `NotificationService.SendToAllTakersAsync` — changed the query to:
```csharp
.Where(u => u.Role == "QuizTaker" || u.Role == "PremiumTaker")
```

---

# Deep Dive — Key Features for Evaluation

---

## SignalR — Real-Time Notification System

### What is SignalR?

SignalR is a library that enables **real-time two-way communication** between the server and the browser. Instead of the browser asking the server "any new data?" every few seconds (polling), SignalR keeps a persistent connection open. When something happens on the server, it **pushes** the data to the browser instantly — like WhatsApp messages.

Under the hood, SignalR uses **WebSockets** (and falls back to long-polling if WebSockets aren't available).

---

### Why did we use SignalR instead of polling?

| Polling | SignalR |
|---------|---------|
| Browser asks server every X seconds | Server pushes when something happens |
| Wastes bandwidth even when nothing changed | Only sends data when needed |
| Delay between event and notification | Instant — milliseconds |
| Simple to implement | Slightly more setup but much better UX |

In a quiz app, when a new quiz is created, you want QuizTakers to know **immediately** — not after 30 seconds of polling.

---

### How it works in your project — Step by Step

**Step 1 — Backend Hub**

`NotificationHub.cs` is the SignalR hub — the central connection point:

```csharp
public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }
}
```

When a user connects, they call `JoinUserGroup("5")`. SignalR adds their connection to a group called `user_5`. Now you can send messages to just that user by targeting `user_5`.

**Step 2 — Registered in Program.cs**

```csharp
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();
app.MapHub<NotificationHub>("/hubs/notifications");
```

CORS must allow credentials for SignalR:
```csharp
policy.AllowCredentials();
```

**Step 3 — NotificationService sends messages**

```csharp
// Send to one specific user
await _hub.Clients.Group($"user_{userId}")
    .SendAsync("ReceiveNotification", notificationDTO);

// Send to all QuizTakers and PremiumTakers
foreach (var userId in takerIds)
{
    await _hub.Clients.Group($"user_{userId}")
        .SendAsync("ReceiveNotification", notificationDTO);
}
```

Every notification is also **saved to the database** so users see them even after re-login.

**Step 4 — Frontend connects**

`notification.service.ts`:

```typescript
this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7220/hubs/notifications', {
        accessTokenFactory: () => this.auth.getToken() ?? ''
    })
    .withAutomaticReconnect()
    .build();

this.hubConnection.on('ReceiveNotification', (notification) => {
    // Add to the BehaviorSubject — navbar updates instantly
    this.notifications$.next([notification, ...current]);
    this.showPopup(notification); // show popup card
});

this.hubConnection.start()
    .then(() => this.hubConnection.invoke('JoinUserGroup', userId.toString()));
```

**Step 5 — Navbar subscribes**

```typescript
this.notifSub = this.notifService.notifications$.subscribe(n => {
    this.notifications = n; // bell badge count updates automatically
});
```

`BehaviorSubject` is an RxJS observable that holds the current value and emits to all subscribers whenever it changes.

---

### What triggers a notification in your project?

| Event | Who gets notified | Type |
|-------|------------------|------|
| Quiz created | All QuizTakers + PremiumTakers | `quiz_added` |
| Quiz updated | All QuizTakers + PremiumTakers | `quiz_updated` |
| Quiz deactivated | All QuizTakers + PremiumTakers | `quiz_deactivated` |
| Quiz assigned to group | All group members | `group_quiz_assigned` |
| Group member submits quiz | GroupManager | `group_submission` |
| Leaderboard #1 changes | All QuizTakers + PremiumTakers | `leaderboard_update` |
| Displaced from #1 | Previous #1 user only | `rank_lost` |

---

### On Login — Unread Notifications Popup

When a user logs in, `connect(userId)` is called from the navbar after fetching the profile. It:
1. Starts the SignalR connection
2. Calls `loadNotifications(showUnreadPopups: true)`
3. Fetches all notifications from `GET /api/notification`
4. Shows up to 3 unread ones as popup cards in the top-right corner
5. Each popup has a redirect — clicking it navigates to the relevant page

---

### Notification Persistence

Every notification is stored in the `Notifications` table:

```
Id | UserId | Message | Type | IsRead | CreatedAt
```

This means even if the user is offline when the notification is sent, they see it when they log in next time. The REST endpoint `GET /api/notification` loads their history. `PUT /api/notification/{id}/read` marks one as read. `PUT /api/notification/read-all` marks all.

---

## Group Manager Feature — End to End Flow

### What is the Group Manager role?

A GroupManager is a role that manages a team of QuizTakers. They create groups, add members, assign quizzes, and review/validate submissions. Think of it like a teacher managing a class.

---

### Complete Flow — Step by Step

**Step 1 — Register as GroupManager**

On the Register page, select "Manage Groups" from the role dropdown. This creates a user with `Role = "GroupManager"` in the database.

**Step 2 — Create a Group**

GroupManager logs in → goes to "My Groups" → clicks "+ New Group" → fills in name and description → submits.

Backend: `POST /api/group` → `GroupService.CreateGroupAsync(dto, managerId)` → saves `Group { Name, Description, CreatedBy = managerId }` to DB.

**Step 3 — Add Members**

In the group detail page → Members tab → type a name or email in the search box.

Backend: `GET /api/group/search-users?query=syed` → `GroupService.SearchUsersAsync` → queries `Users` where `Role == "QuizTaker"` and name/email contains the query → returns matching users.

Click "Add" → `POST /api/group/{id}/members/{userId}` → saves `GroupMember { GroupId, UserId }`.

**Step 4 — Assign a Quiz**

Quizzes tab → shows all active quizzes → click "Assign" on any quiz.

Backend: `POST /api/group/{id}/quizzes/{quizId}` → `GroupService.AssignQuizAsync` → saves `GroupQuiz { GroupId, QuizId, RequiresValidation = false }` → **notifies all group members** via SignalR with `group_quiz_assigned`.

**Step 5 — Create a Quiz for the Group**

"Create Quiz" tab → fill in title, category, difficulty → click "Create & Assign to Group".

Backend: `POST /api/quiz` → quiz created → `POST /api/group/{id}/quizzes/{quizId}` → quiz assigned → `PUT /api/group/{id}/quizzes/{quizId}/require-validation` → sets `RequiresValidation = true` → navigates to questions page.

**Key difference:** Assigned existing quiz → `RequiresValidation = false` (auto). Created by GM → `RequiresValidation = true` (needs approval).

**Step 6 — Member Takes the Quiz**

QuizTaker logs in → Browse Quizzes → sees "👥 Assigned to You by Group" section at top → group quizzes are **hidden from the general list** to avoid confusion → clicks "Start Quiz" → completes it → submits.

Backend on submit: `QuizAttemptService` saves `QuizResult` → checks if this quiz belongs to a group the user is in → creates `GroupQuizResult { GroupQuizId, UserId, QuizResultId, ValidationStatus = "Pending" }` → **notifies the GroupManager** with score details.

**Step 7 — GroupManager Reviews Submissions**

Submissions tab → sees all member submissions with score and status.

- If `RequiresValidation = false` → Action shows "Auto" — no review needed
- If `RequiresValidation = true` → Action shows "Approve" / "Revoke" buttons

Click "Approve" → `PUT /api/group/submissions/{resultId}/validate` → `ValidationStatus = "Approved"`.

**Step 8 — QuizTaker sees their Group Results**

My Results page → "👥 Group Results" tab → shows Group Name, Quiz Title, Score, Validation Status (Pending/Approved/N/A).

---

### Database Tables Involved

```
Groups          — group info + CreatedBy (GroupManager)
GroupMembers    — which users are in which group
GroupQuizzes    — which quizzes are assigned to which group + RequiresValidation
GroupQuizResults — member submissions with ValidationStatus
```

---

## Premium Upgrade — Complete Flow

### What is PremiumTaker?

A PremiumTaker is a QuizTaker who has paid to upgrade. They get **unlimited quiz attempts** with a **24-hour cooldown** between retakes on the same quiz. Normal QuizTakers get only one attempt per quiz.

---

### Complete Flow — Step by Step

**Step 1 — User is a normal QuizTaker**

Registered as QuizTaker. Takes a quiz. Sees "🔒 Already Attempted" on that quiz in Browse. Wants to retake.

**Step 2 — Goes to Profile**

Profile page shows:
- Current Plan: 🆓 Free Plan
- "Single attempt per quiz"
- Button: "⭐ Upgrade to Premium — ₹499"

**Step 3 — Payment Modal Opens**

Clicks the button → payment modal appears with:
- Plan summary (₹499)
- Card Number field (auto-formats as `4242 4242 4242 4242`)
- Expiry (MM/YY)
- CVV
- Name on Card

**Step 4 — Validation**

Frontend validates before calling the API:
- Card number must be 16 digits
- Expiry must match `MM/YY` pattern using `Validators.pattern`
- CVV must be 3+ digits
- Name must not be empty

**Step 5 — Simulated Payment Processing**

Click "Pay ₹499" → `paymentLoading = true` → `setTimeout(..., 2000)` → 2 second spinner → calls `PUT /api/user/upgrade-to-premium`.

**Step 6 — Backend Upgrades the Role**

`UserService.UpgradeToPremiumAsync(userId)`:
1. Finds the user by ID
2. Checks role is `QuizTaker` (not already premium)
3. Changes `user.Role = "PremiumTaker"`
4. Saves to DB via `_userRepo.UpdateAsync(user)`
5. Generates a **new JWT token** with `PremiumTaker` role in the claims
6. Returns `UpgradeResponseDTO { Token, Role, Message }`

**Why a new JWT token?** The old token still says `QuizTaker` inside it. JWT tokens are self-contained — the role is embedded. The server can't change the token after issuing it. So a fresh token with the new role must be issued.

**Step 7 — Frontend Saves New Token**

```typescript
localStorage.setItem('JWT-token', res.data.token);
localStorage.setItem('user-role', res.data.role);
```

Then navigates to `/home` and back to `/profile` — this triggers the navbar's `NavigationEnd` listener which calls `loadUserInfo()` and picks up `PremiumTaker` from localStorage.

**Step 8 — Navbar Updates**

`isTaker()` returns true for both `QuizTaker` and `PremiumTaker` — so all taker nav links remain. Username shows ⭐ prefix. Profile shows "⭐ Premium Plan" badge.

**Step 9 — Browse Quizzes Updates**

`getAttemptStatus()` is called on init. For PremiumTaker:
- If within 24 hours of last attempt → shows "⏳ Cooldown — Xh left"
- If cooldown passed → shows "Start Quiz" — can retake

**Step 10 — 24-Hour Cooldown on Retake**

When PremiumTaker submits a quiz they've already taken:

```csharp
var hoursSince = (DateTime.UtcNow - lastAttempt.CompletedAt).TotalHours;
if (hoursSince < 24)
{
    var hoursLeft = (int)Math.Ceiling(24 - hoursSince);
    return (false, $"You can retake this quiz after {hoursLeft} more hour(s).", null);
}
```

The `CompletedAt` timestamp already existed on `QuizResult` — no model change needed.

---

### Summary — QuizTaker vs PremiumTaker

| Feature | QuizTaker | PremiumTaker |
|---------|-----------|--------------|
| Attempts per quiz | 1 | Unlimited (24h cooldown) |
| Browse page status | 🔒 Already Attempted | ⏳ Cooldown or Start Quiz |
| Leaderboard | First attempt counts | First attempt counts (planned) |
| Notifications | ✅ Yes | ✅ Yes |
| Group quizzes | ✅ Yes | ✅ Yes |
| Upgrade option | ✅ Profile page | Already premium |
| JWT role claim | `QuizTaker` | `PremiumTaker` |

---

## Quiz Analytics — Creator Stats

**Operation:** Backend (existing) + Frontend (new UI)

### What is it?

A QuizCreator can view analytics for each of their quizzes directly on the "My Quizzes" page by clicking the "📊 Stats" button on any quiz card.

### What data does it show?

| Stat | Meaning |
|------|---------|
| Total Attempts | How many QuizTakers have submitted this quiz |
| Avg Score | Average percentage across all submissions |
| Highest | Best score anyone achieved |
| Lowest | Worst score anyone achieved |

### How it works — Step by Step

**Backend — already existed:**

`GET /api/quiz/{id}/stats` → `QuizService.GetQuizStatsAsync(quizId, creatorId)`:
1. Verifies the quiz belongs to the logged-in creator — access denied if not
2. Queries all `QuizResults` where `QuizId == quizId`
3. Calculates `TotalAttempts`, `AverageScore`, `HighestScore`, `LowestScore`
4. Returns as an anonymous object

**Frontend — what was added:**

`my-quizzes.ts`:
- `statsMap = new Map<number, any>()` — stores loaded stats per quiz ID so they don't reload every time
- `openStatsId` — tracks which quiz's stats panel is currently open
- `loadingStatsId` — tracks which quiz is currently loading stats
- `toggleStats(quiz)` — if already open, closes it. If not loaded yet, calls `quizService.getStats(quiz.id)` and stores result in `statsMap`. If already loaded, just opens the panel (no extra API call)

`my-quizzes.html`:
- "📊 Stats" button added to each quiz card
- Inline stats panel renders below the card when open
- Shows 4 stat boxes in a responsive grid — Total Attempts, Avg Score, Highest, Lowest
- Each box has a color: blue for attempts, green for average, amber for highest, red for lowest

### Why this approach (Map instead of array)?

Using `Map<quizId, stats>` means once a quiz's stats are loaded, they're cached. If the creator opens and closes the panel multiple times, it only makes **one API call** — not one every time. This is efficient and avoids unnecessary network requests.
