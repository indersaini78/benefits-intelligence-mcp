# Copilot Instructions — BenefitsIntelligence MCP

**Tech Stack:** .NET 10, C# 13, EF Core 10.
**Formatting:** Nullable enabled, file-scoped namespaces.
**Database:** (localdb)\MSSQLLocalDB. DB Name: BenefitsIntelligenceDb. Connection string key: `ConnectionStrings:BenefitsDb`.

**Architecture & Patterns:**
* Each MCP uses `ModelContextProtocol.AspNetCore`. Tools must be defined as `[McpServerTool]` methods.
* Writes must use MediatR commands. Reads must use direct EF Core queries.
* Always return DTOs from `Shared.Contracts`. NEVER return raw EF entities.

**Testing:**
* Stack: xUnit + FluentAssertions + Moq.
* Integration tests must use `Testcontainers.MsSql`, seeded exactly from `/db/MCP_Database_Schema.sql`.

**Security (CRITICAL):** 
* Redact `MemberId` beyond the last 4 characters in ALL logs, exceptions, and console outputs.