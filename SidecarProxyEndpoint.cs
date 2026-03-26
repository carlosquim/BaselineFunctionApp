using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.IO;

namespace BaselineFunctionApp
{
    public static class SidecarProxyEndpoint
    {
        public static void MapSidecarProxyEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/api/sidecar/{targetFunction}", async (
                string targetFunction, 
                HttpRequest request, 
                IAppLogicService appLogicService, 
                ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("SidecarProxyEndpoint");
                logger.LogInformation($"[Pod: {Environment.MachineName}] Container proxy received event for target function: {targetFunction}");

                using var reader = new StreamReader(request.Body);
                string requestBody = await reader.ReadToEndAsync();

                // Here you would normally deserialize the requestBody into the specific
                // event type required by the targetFunction wrapper. 
                if (targetFunction.Equals("blobtriggercqm01", System.StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(requestBody);
                        string blobName = payload.GetProperty("blobName").GetString() ?? "unknown";
                        logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {blobName} \n Data: [Stream handled by Sidecar, or not provided]");
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError(ex, "Failed to process blob payload.");
                    }
                    return Results.Ok();
                }

                if (targetFunction.Equals("CosmosTriggerFunction", System.StringComparison.OrdinalIgnoreCase) || 
                    targetFunction.Equals("SecondaryCosmosFunction", System.StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var documents = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<MyDocument>>(requestBody);
                        if (documents != null && documents.Count > 0)
                        {
                            logger.LogInformation($"Cosmos DB proxy processing {documents.Count} documents for {targetFunction}.");
                            foreach (var doc in documents)
                            {
                                await appLogicService.ProcessDocumentAsync(doc);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogError(ex, $"Failed to process Cosmos DB payload for {targetFunction}.");
                    }
                    return Results.Ok();
                }

                // For demonstration purposes, we invoke the shared application logic service
                // with a simulated document representing the event payload.
                var dummyDoc = new MyDocument { Id = $"Event-From-{targetFunction}" };
                await appLogicService.ProcessDocumentAsync(dummyDoc);

                logger.LogInformation($"Successfully processed event for '{targetFunction}'.");
                return Results.Ok();
            });
        }
    }
}
