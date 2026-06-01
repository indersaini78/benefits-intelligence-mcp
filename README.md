# BenefitsIntelligence MCP

A suite of **Model Context Protocol (MCP)** servers for health-benefits intelligence, built on **.NET 10**, **C# 13**, and **EF Core 10**.

Each MCP server exposes domain-specific tools that AI agents can invoke to query and mutate benefits data through a standardized protocol.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      MCP Servers                            │
├──────────────┬──────────────┬──────────────┬───────────────┤
│  Eligibility │  Enrollment  │   Billing &  │  Account &    │
│              │              │   Payroll    │  Access       │
├──────────────┴──────────────┴──────────────┴───────────────┤
│                   Customer Support                          │
└─────────────────────────────────────────────────────────────┘
		│                    │                    │
		▼                    ▼                    ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────────┐
│ Shared.Contracts │ │ Shared.Infrastructure │ │  SQL Server DB  │
└───────────────┘   └───────────────┘   └───────────────────┘
```

| Layer | Purpose |
|-------|---------|
| `Shared.Contracts` | DTOs and MediatR command/request/response records |
| `Shared.Infrastructure` | EF Core DbContexts and entity definitions |
| `Mcp.*` servers | `[McpServerTool]` surfaces + MediatR write handlers |

**Key patterns:**
- **Reads** → direct EF Core queries (`AsNoTracking`)
- **Writes** → MediatR commands dispatched to handlers
- **Security** → `MemberId` redacted beyond last 4 chars in all logs/exceptions

---

## MCP Servers

| Server | Port | Domain |
|--------|------|--------|
| `Mcp.Eligibility` | 5100 | Coverage lookup, accumulator queries, plan details |
| `Mcp.Enrollment` | 5200 | Create/terminate/change enrollments, add dependents |
| `Mcp.BillingPayroll` | 5300 | Invoices, balances, payments, deductions, paycheck deltas |
| `Mcp.AccountAccess` | 5400 | Login diagnostics, unlock accounts, password resets |
| `Mcp.CustomerSupport` | 5500 | Case management, notes, interactions, escalations, complaints |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB for dev, Docker for integration tests)
- Docker (for integration tests only)

---

## Getting Started

### 1. Create the database

```bash
# Using sqlcmd against LocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -i db/MCP_Database_Schema.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d BenefitsIntelligenceDb -i db/MCP_Database_Seed.sql
```

### 2. Run a server

```bash
dotnet run --project src/Mcp.Eligibility
```

### 3. Run all tests

```bash
# Unit tests (in-memory EF)
dotnet test --filter "Project!=BenefitsIntelligence.IntegrationTests"

# Integration tests (requires Docker)
dotnet test --filter "Project=BenefitsIntelligence.IntegrationTests"
```

---

## Project Structure

```
├── db/
│   ├── MCP_Database_Schema.sql      # Full schema with constraints
│   └── MCP_Database_Seed.sql        # Realistic seed data
├── src/
│   ├── Shared.Contracts/            # DTOs, commands, requests
│   ├── Shared.Infrastructure/       # EF Core DbContexts & entities
│   ├── Mcp.Eligibility/             # Eligibility MCP server
│   ├── Mcp.Enrollment/              # Enrollment MCP server
│   ├── Mcp.BillingPayroll/          # Billing & Payroll MCP server
│   ├── Mcp.AccountAccess/           # Account & Access MCP server
│   └── Mcp.CustomerSupport/         # Customer Support MCP server
└── tests/
	├── Mcp.Eligibility.Tests/
	├── Mcp.Enrollment.Tests/
	├── Mcp.BillingPayroll.Tests/
	├── Mcp.AccountAccess.Tests/
	├── Mcp.CustomerSupport.Tests/
	└── BenefitsIntelligence.IntegrationTests/  # Testcontainers + real SQL
```

---

## Testing

| Layer | Stack | Database |
|-------|-------|----------|
| Unit | xUnit + FluentAssertions + Moq | EF Core InMemory |
| Integration | xUnit + Testcontainers.MsSql | Real SQL Server 2022 in Docker |

Integration tests validate that handlers don't drift from database constraint definitions by running against the actual schema and seed scripts.

---

## Configuration

All servers use the same connection string key:

```json
{
  "ConnectionStrings": {
	"BenefitsDb": "Server=(localdb)\\MSSQLLocalDB;Database=BenefitsIntelligenceDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

## License

Private — all rights reserved.
