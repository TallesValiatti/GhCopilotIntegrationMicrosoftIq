using ModelContextProtocol.Server;
using System.ComponentModel;
using Azure;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;

namespace Docs.mcp.Tools;

[McpServerToolType]
public static class EnterpriseDocsTool
{
    private static string GetSearchEndpoint(IConfiguration configuration) => configuration["AzureAISearch:Endpoint"]!;
    private static string GetSearchApiKey(IConfiguration configuration) => configuration["AzureAISearch:ApiKey"]!;
    private static string GetDevelopmentKnowledgeBaseName(IConfiguration configuration) => configuration["AzureAISearch:DevelopmentKnowledgeBaseName"]!;
    private static string GetBusinessLogicKnowledgeBaseName(IConfiguration configuration) => configuration["AzureAISearch:BusinessLogicKnowledgeBaseName"]!;

    [McpServerTool, Description("Used to retrieve information on how code standards are implemented in our company. Retrieves enterprise documentation from the Azure AI Search Knowledge Base, including internal coding standards, architectural patterns, technical guidelines, internal development guidance, and official tool documentation. It helps developers follow approved internal coding patterns and complement them with authoritative vendor docs when needed. Provide a natural language query to get consolidated, relevant content from the knowledge base.")]
    public static async Task<string> QueryDevelopmentDocs(
        IConfiguration configuration,
        [Description("The query to retrieve consolidated documentation data for.")] string query)
    {
        var searchEndpoint = GetSearchEndpoint(configuration);
        var searchApiKey = GetSearchApiKey(configuration);
        var knowledgeBaseName = GetDevelopmentKnowledgeBaseName(configuration);

        if (string.IsNullOrWhiteSpace(searchEndpoint) || string.IsNullOrWhiteSpace(searchApiKey) || string.IsNullOrWhiteSpace(knowledgeBaseName))
        {
            return "Azure AI Search configuration is missing. Please check Endpoint, ApiKey, and KnowledgeBaseName settings.";
        }

        return await QueryKnowledgeBase(searchEndpoint, searchApiKey, knowledgeBaseName, query);
    }

    [McpServerTool, Description("Used to retrieve business logic documentation for internal rules, domain policies, pricing, discounts, workflows, and process decisions. It helps developers and analysts apply approved business behavior consistently across systems and use official internal documentation as the source of truth. Provide a natural language query to get consolidated, relevant content from the business logic knowledge base.")]
    public static async Task<string> QueryBusinessLogicDocs(
        IConfiguration configuration,
        [Description("The query to retrieve consolidated business logic documentation data for.")] string query)
    {
        var searchEndpoint = GetSearchEndpoint(configuration);
        var searchApiKey = GetSearchApiKey(configuration);
        var knowledgeBaseName = GetBusinessLogicKnowledgeBaseName(configuration);

        if (string.IsNullOrWhiteSpace(searchEndpoint) || string.IsNullOrWhiteSpace(searchApiKey) || string.IsNullOrWhiteSpace(knowledgeBaseName))
        {
            return "Azure AI Search configuration is missing. Please check Endpoint, ApiKey, and BusinessLogicKnowledgeBaseName settings.";
        }

        return await QueryKnowledgeBase(searchEndpoint, searchApiKey, knowledgeBaseName, query);
    }

    private static async Task<string> QueryKnowledgeBase(string searchEndpoint, string searchApiKey, string knowledgeBaseName, string query)
    {
        var credential = new AzureKeyCredential(searchApiKey);
        var kbClient = new KnowledgeBaseRetrievalClient(
            endpoint: new Uri(searchEndpoint),
            knowledgeBaseName: knowledgeBaseName,
            credential: credential
        );

        var retrievalRequest = new KnowledgeBaseRetrievalRequest();
        retrievalRequest.Messages.Add(
            new KnowledgeBaseMessage(
                content: new[] {
                    new KnowledgeBaseMessageTextContent(query)
                }
            ) { Role = "user" }
        );

        try
        {
            var result = await kbClient.RetrieveAsync(retrievalRequest);
            var response = result.Value.Response.FirstOrDefault();
            if (response?.Content?.FirstOrDefault() is KnowledgeBaseMessageTextContent textContent)
            {
                return textContent.Text;
            }
            return "No response content returned from Azure AI Search.";
        }
        catch (Exception ex)
        {
            return $"Error querying Azure AI Search: {ex.Message}";
        }
    }
}
