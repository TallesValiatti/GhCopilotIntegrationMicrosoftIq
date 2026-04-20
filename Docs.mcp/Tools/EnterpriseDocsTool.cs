using ModelContextProtocol.Server;
using System.ComponentModel;
using Azure;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Core;

namespace Docs.mcp.Tools;

[McpServerToolType]
public static class EnterpriseDocsTool
{
    private static string GetSearchEndpoint(IConfiguration configuration) => configuration["AzureAISearch:Endpoint"]!;
    private static string GetSearchApiKey(IConfiguration configuration) => configuration["AzureAISearch:ApiKey"]!;
    private static string GetKnowledgeBaseName(IConfiguration configuration) => configuration["AzureAISearch:KnowledgeBaseName"]!;

    [McpServerTool, Description("Retrieves enterprise documentation from the Azure AI Search Knowledge Base. Use this tool to find information about internal coding standards, architectural patterns, and technical guidelines that developers must follow. Provide a natural language query to get consolidated, relevant content from the knowledge base.")]
    public static async Task<string> QueryEnterpriseDocs(
        IConfiguration configuration,
        [Description("The query to retrieve consolidated documentation data for.")] string query)
    {
        var searchEndpoint = GetSearchEndpoint(configuration);
        var searchApiKey = GetSearchApiKey(configuration);
        var knowledgeBaseName = GetKnowledgeBaseName(configuration);

        if (string.IsNullOrWhiteSpace(searchEndpoint) || string.IsNullOrWhiteSpace(searchApiKey) || string.IsNullOrWhiteSpace(knowledgeBaseName))
        {
            return "Azure AI Search configuration is missing. Please check Endpoint, ApiKey, and KnowledgeBaseName settings.";
        }

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
