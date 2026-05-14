# GhCopilotIntegrationMicrosoftIq – Enterprise knowledge meets GitHub Copilot via MCP

This repository accompanies a two-part article that shows how to connect **WorkIQ**, **Foundry IQ**, and a custom **Model Context Protocol (MCP)** server to **GitHub Copilot** inside **VS Code**, so the agent can consult internal knowledge bases and generate code that is aligned with real enterprise standards and business rules.

The solution is composed of two projects: a custom MCP server (`Docs.mcp`) that exposes **Azure AI Search** knowledge bases as Copilot tools, and a sample **.NET 10 Minimal API** (`Order.Api`) that the agent evolves by querying those knowledge bases for discount rules and OpenTelemetry configuration guidelines.

## What's inside

- **`Docs.mcp/`** — ASP.NET Core MCP server built with the `ModelContextProtocol.Server` SDK. Exposes two tools to GitHub Copilot:
  - `QueryDevelopmentDocs` — retrieves internal coding standards, architectural patterns, and technical guidelines from the development knowledge base.
  - `QueryBusinessLogicDocs` — retrieves domain policies, pricing tiers, discount rules, and process decisions from the business logic knowledge base.
- **`Order.Api/`** — .NET 10 Minimal API with endpoints for orders and products. Used as the target application that GitHub Copilot evolves during the article walkthrough. Includes discount logic and full OpenTelemetry instrumentation (traces, metrics, logs) with OTLP export.
- **`docker-compose.yaml`** — Runs the **.NET Aspire Dashboard** on port `18888`, which receives OTLP telemetry from `Order.Api` on port `4317`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/products/docker-desktop) (for the Aspire Dashboard)
- [VS Code](https://code.visualstudio.com/) with the **GitHub Copilot** extension
- An **Azure AI Search** resource with two Foundry IQ knowledge bases provisioned:
  - A development/coding standards knowledge base
  - A business logic knowledge base
- A **Microsoft 365** tenant with **WorkIQ** enabled (for the MCP discovery flow shown in Part 1)

## Configure credentials

The `Docs.mcp` project reads its configuration from `appsettings.json`. Fill in the four values below before running the server.

| Setting | Description |
|---|---|
| `AzureAISearch:Endpoint` | URL of your Azure AI Search service (e.g. `https://<name>.search.windows.net`) |
| `AzureAISearch:ApiKey` | Admin or query API key for the search service |
| `AzureAISearch:DevelopmentKnowledgeBaseName` | Name of the Foundry IQ knowledge base for coding standards |
| `AzureAISearch:BusinessLogicKnowledgeBaseName` | Name of the Foundry IQ knowledge base for business rules |

Edit `Docs.mcp/appsettings.json`:

```json
{
  "AzureAISearch": {
    "Endpoint": "<YOUR_SEARCH_SERVICE_URL>",
    "ApiKey": "<YOUR_SEARCH_API_KEY>",
    "DevelopmentKnowledgeBaseName": "<YOUR_DEVELOPMENT_KNOWLEDGE_BASE_NAME>",
    "BusinessLogicKnowledgeBaseName": "<YOUR_BUSINESS_LOGIC_KNOWLEDGE_BASE_NAME>"
  }
}
```

Or use `appsettings.Development.json` to override values locally without committing secrets.

## Run

### 1. Start the Aspire Dashboard (telemetry UI)

```bash
docker compose up -d
```

The dashboard will be available at `http://localhost:18888`.

### 2. Run the MCP server

```bash
cd Docs.mcp
dotnet run
```

The server starts on `http://localhost:5000` (or the port configured in `Properties/launchSettings.json`) and registers the two MCP tools automatically via `WithToolsFromAssembly()`.

### 3. Register the MCP server in VS Code

Add the following entry to your VS Code `settings.json` (or `.vscode/mcp.json`) so GitHub Copilot discovers the server:

```json
{
  "mcp": {
    "servers": {
      "enterprise-docs": {
        "type": "http",
        "url": "http://localhost:5000/mcp"
      }
    }
  }
}
```

### 4. Run the Order API

```bash
cd Order.Api
dotnet run
```

OpenAPI is available at `http://localhost:<port>/openapi/v1.json` in development mode. Traces, metrics, and logs are exported via OTLP to `http://localhost:4317` and visible in the Aspire Dashboard.

## Customizing

- **Add a new knowledge base** — create an additional `[McpServerTool]` method in `Docs.mcp/Tools/EnterpriseDocsTool.cs`, add the corresponding name to `appsettings.json`, and re-register the server in VS Code.
- **Change the OTLP endpoint** — set the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable before running `Order.Api`, or configure it in `appsettings.json` under `OpenTelemetry`.
- **Swap the in-memory database** — replace `options.UseInMemoryDatabase("OrderDb")` in `Order.Api/Extensions/ExtensionConfiguration.cs` with a real EF Core provider.

## References

Part 1 article: https://www.azurebrasil.cloud/blog/do-conhecimento-corporativo-ao-codigo-evoluindo-aplicacoes-com-work-iq-foundry-iq-e-github-copilot-parte-1/  
Part 2 article: https://www.azurebrasil.cloud/blog/do-conhecimento-corporativo-ao-codigo-evoluindo-aplicacoes-com-work-iq-foundry-iq-e-github-copilot-parte-2/  
WorkIQ overview: https://learn.microsoft.com/en-us/microsoft-365/copilot/extensibility/workiq-overview  
Foundry IQ overview: https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/what-is-foundry-iq  
Azure AI Search: https://learn.microsoft.com/en-us/azure/search/search-what-is-azure-search  
Model Context Protocol: https://modelcontextprotocol.io  
.NET Aspire Dashboard: https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview  
OpenTelemetry .NET: https://opentelemetry.io/docs/languages/net  
