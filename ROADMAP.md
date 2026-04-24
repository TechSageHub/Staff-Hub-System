# Roadmap

Candidate features grouped by effort and value. Items reference modules that already exist in the codebase.

## Quick wins (reuse existing data/models)

- **Analytics dashboard** — charts on headcount, leave usage, attendance trends, ticket SLAs; admin dashboard is already a natural home.
- **Org chart** — visual hierarchy from existing Department + Manager relationships.
- **Leave calendar** — team view of approved leaves so managers can see coverage.

## Natural extensions of existing modules

- **Offboarding workflow** — mirrors the Onboarding module (exit checklist, asset return, final payroll, revoke access).
- **Training & certification tracker** — courses, completion records, expiry alerts; extends the Onboarding framework.
- **Benefits enrollment** — health, pension, allowances; hangs off Payroll.
- **Timesheets / shift scheduling** — goes beyond the basic AttendanceLog.

## People-ops features

- **360° feedback & engagement surveys** — complements Performance Appraisal.
- **Recruitment / ATS-lite** — job posts, applicant tracking, interview scheduling; feeds into Onboarding on hire.
- **Policy library with acknowledgement tracking** — employees sign off on versioned policies.
- **Recognition / kudos wall** — complements Announcements.

## Technical / UX upgrades

- **Notifications hub** — in-app + email digests (SendGrid is already wired).
- **Audit log** — who changed what on sensitive records.
- **Employee self-service mobile PWA** — leave requests, payslips, tickets on phone.
- **Bulk import/export** — CSV for employees, attendance, payroll.
