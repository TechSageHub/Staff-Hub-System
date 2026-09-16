# Notifications Hub — Design

Date: 2026-09-16
Status: Approved design, awaiting spec review before implementation.

## Problem

StaffHub has SMTP email (`IEmailService`) but no notification records.
Announcements have no read-tracking, and leave / HR-ticket / onboarding /
offboarding / payroll events notify nobody. Users must poll each module to
discover things waiting on them.

## Goal

One hub covering all event sources with three channels: in-app bell,
instant email for urgent events, periodic digest email for the rest.

## Non-goals (v1)

- Real-time push (no SignalR).
- Per-user channel preferences UI (urgent vs. digest mapping is fixed in code).
- SendGrid migration (keep existing SMTP `IEmailService`).

## Data model

New `Notification` entity in `Data/Model/`, configuration in
`Data/Configurations/` (auto-applied), one EF migration:

- `Id` (Guid), `UserId` (string, Identity user FK, indexed)
- `Type` (enum: `Leave`, `HrTicket`, `Announcement`, `Onboarding`, `Offboarding`, `Payroll`, `Appraisal`)
- `Title`, `Body`, `LinkUrl` (relative MVC URL, nullable)
- `IsRead` (bool, default false), `DigestedAt` (nullable UTC, set when
  included in a digest email), `CreatedAt` (UTC)
- Index on `(UserId, IsRead, CreatedAt)` for the bell query.

No changes to existing tables.

## Service

`Application/Services/Notification/INotificationService.cs` (registered
`Scoped` in `ApplicationServiceExtension.AddServices()`):

- `NotifyAsync(userId, type, title, body, linkUrl)` — writes the row; if the
  type is urgent, also sends instant email via `IEmailService`.
- `GetUnreadAsync(userId, take)` / `GetPagedAsync(userId, page, size)` —
  bell count + list page.
- `MarkReadAsync(userId, id)` / `MarkAllReadAsync(userId)`.

Urgent (instant email): leave decision, ticket assigned/updated, announcement
posted, offboarding task assigned. Everything else: in-app + digest only.

Email failures are caught and logged — they never fail the triggering action.

## Fan-out points (callers of `NotifyAsync`)

- `LeaveService`: request submitted (notify admins), approved/rejected
  (notify employee).
- `HrTicketService`: ticket created (notify admins), comment/status change
  (notify requester + assignee).
- `AnnouncementService`: new post (notify all users in `User` role).
- `OnboardingService` / `OffboardingService`: step completed / task assigned.
- Payroll (`PayrollRecord`) and appraisal (`PerformanceAppraisal`) have models
  but no dedicated services today — fan-out hooks into whichever code path
  publishes those records; creating thin publish methods there is in scope,
  creating full payroll/appraisal services is not.

Recipient resolution reuses `UserManager<IdentityUser>` / role membership;
no new identity plumbing.

## In-app UI

- Bell icon with unread count in the shared layout (`_Layout.cshtml`),
  visible to authenticated users.
- `NotificationController` (inherits the global `OnboardingCompletionFilter`
  like other controllers) with Index (paged list) + mark-read POST actions
  (antiforgery tokens per global `AutoValidateAntiforgeryToken`).
- Mapping via manual extension methods in `Presentation/DtoMapping/`
  (no AutoMapper), consistent with existing convention.

## Digest job

`NotificationDigestService : BackgroundService` living in
`Application/Services/Notification/`, registered via
`builder.Services.AddHostedService<NotificationDigestService>()` in
`Presentation/Program.cs`:

- Runs on a configurable interval (default daily, `NotificationDigest:IntervalHours`).
- Per user with unread non-urgent notifications: one SMTP summary email,
  then marks included rows as digested (add `DigestedAt`, nullable, to the
  model) so they are not re-sent.
- Skipped entirely when `MailSettings` is unconfigured (logs a warning).

## Verification

- `dotnet build StaffHubSystem.sln`.
- `dotnet ef migrations add AddNotifications --project Data/Data.csproj --startup-project Presentation/Presentation.csproj`
  followed by `dotnet ef database update` locally; app also runs
  `MigrateAsync()` on startup.
- Manual pass: trigger one event per type, confirm bell, instant email
  (with local SMTP configured), and digest content.

## Rollout risks

- Announcement fan-out to all users = N rows + N emails; instant email for
  announcements should go only to urgent/pinned posts if volume is a concern.
- Digest + instant double-send: prevented by the `DigestedAt` marker and by
  excluding urgent types from the digest body (or labeling them as reminders).
