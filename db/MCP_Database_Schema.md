# MCP Server Database Schema

Single LocalDB database (`BenefitsIntelligenceDb`) — only the tables MCP tools read/write.
RAG KB lives in Azure AI Search (not SQL). Agent runtime (sessions, audit, cost, eval) lives in the orchestrator's own store, not here.

## Schemas (one per bounded context = one MCP server)

| Schema | MCP Server | Mode |
|---|---|---|
| `member` | Shared master | R (used by all) |
| `elig` | Eligibility MCP | R |
| `enroll` | Enrollment MCP | R + W |
| `billing` | Billing & Payroll MCP | R |
| `payroll` | Billing & Payroll MCP | R |
| `iam` | Account & Access MCP | R + W |
| `support` | Customer Support MCP | R + W |

---

## A. Shared / Member Master — `member`

| Table | Purpose |
|---|---|
| `EmployerGroup` | Employer groups, plan year, status |
| `Member` | Subscriber/employee demographics + employment status |
| `Dependent` | Spouse / children covered under a member |

## B. Eligibility MCP — `elig`

| Table | Purpose |
|---|---|
| `BenefitPlan` | Plan catalog (Medical/Dental/Vision/Life), deductibles, OOP, copay |
| `MemberCoverage` | Active/terminated coverage per member per plan |
| `Accumulator` | Deductible / OOP / visit-limit running totals per plan year |
| `CoverageHistory` | Append-only coverage change log |

Tools: `eligibility.check_active_coverage`, `eligibility.get_accumulators`, `eligibility.get_plan_details`.

## C. Enrollment MCP — `enroll`

| Table | Purpose |
|---|---|
| `EnrollmentTransaction` | One row per write action (NewHire / Termination / QLE / PlanChange) with JSON request + result + correlation id |
| `PlanElection` | Plan-level Add/Drop/Change rows inside a transaction |
| `QualifyingLifeEvent` | QLE window tracking |
| `OutboxEvent` | Outbox pattern → publish to Service Bus `enrollment-events` |

Tools: `enrollment.create`, `enrollment.terminate`, `enrollment.change_plan`, `enrollment.add_dependent`, `enrollment.get_status`.

## D. Billing & Payroll MCP — `billing` + `payroll`

| Table | Purpose |
|---|---|
| `billing.Invoice` | Member- or group-level invoices, balance, status |
| `billing.InvoiceLine` | Per-plan premium breakdown |
| `billing.Payment` | Posted payments, methods, confirmation |
| `billing.PremiumRate` | Tier × age-band × salary-band rate matrix |
| `payroll.PayrollDeduction` | Per pay period pre/post-tax deductions |
| `payroll.PayrollFile` | Inbound payroll file metadata |

Tools: `billing.get_invoice`, `billing.get_balance`, `billing.list_payments`, `billing.explain_invoice_line`, `payroll.get_deductions`, `payroll.explain_paycheck_delta`.

## E. Account & Access MCP — `iam`

| Table | Purpose |
|---|---|
| `UserAccount` | Maps Okta user → MemberId; lockout state, MFA, failed-login counter |
| `LoginAttempt` | Every login attempt with reason code → **login issue resolution** |
| `PasswordResetRequest` | Reset tickets — channel, verification, status, expiry |
| `AccessRole` | RBAC roles with JSON scopes |
| `UserRole` | User ↔ role mapping |

Tools: `iam.diagnose_login`, `iam.unlock_account`, `iam.initiate_password_reset`, `iam.verify_identity`, `iam.list_recent_attempts`.

## F. Customer Support MCP — `support`

| Table | Purpose |
|---|---|
| `Case` | Master case record — type, status, priority, channel, assigned user/queue, SLA due, correlation id linking back to enrollment / billing / iam actions |
| `CaseNote` | Append-only notes on a case (internal vs member-visible), author type (Agent / CSR / Member) |
| `Interaction` | Every member touchpoint (phone, chat, portal, email) — handled by human or agent, intent, sentiment, transcript URI, duration |
| `Escalation` | Case escalations — reason (LowConfidence / Complaint / SLABreach / GuardrailTriggered), target queue, ack/resolve timestamps |
| `Complaint` | Formal complaints / grievances — type, severity, regulatory flag, root cause, corrective action, regulator-mandated due date |

Tools: `support.create_case`, `support.get_case`, `support.add_note`, `support.log_interaction`, `support.escalate`, `support.file_complaint`, `support.list_open_by_member`.

---

## CSR Flow → Table Coverage

| Flow | Tables | Tool |
|---|---|---|
| Eligibility verification | `member.Member`, `elig.MemberCoverage`, `elig.Accumulator` | `eligibility.check_active_coverage` |
| Enrollment processing | `enroll.EnrollmentTransaction`, `enroll.PlanElection`, `enroll.OutboxEvent` | `enrollment.create` / `terminate` / `change_plan` |
| Billing inquiries | `billing.Invoice`, `billing.InvoiceLine`, `billing.Payment` | `billing.get_invoice`, `billing.explain_invoice_line` |
| Payroll questions | `payroll.PayrollDeduction`, `payroll.PayrollFile` | `payroll.get_deductions` |
| Plan / coverage Q&A | `elig.BenefitPlan` (structured) + Azure AI Search (unstructured docs — not in SQL) | `kb.search` (uses AI Search index, not this DB) |
| Password resets | `iam.UserAccount`, `iam.PasswordResetRequest` | `iam.initiate_password_reset` |
| Login issue resolution | `iam.LoginAttempt`, `iam.UserAccount` | `iam.diagnose_login`, `iam.unlock_account` |

**Total: 27 tables across 7 schemas** (member 3, elig 4, enroll 4, billing 4, payroll 2, iam 5, support 5).
