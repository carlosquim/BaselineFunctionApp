/*
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BaselineFunctionApp
{
    public class SecondaryCosmosFunction
    {
        private readonly ILogger _logger;
        private readonly IAppLogicService _appLogicService;

        public SecondaryCosmosFunction(ILoggerFactory loggerFactory, IAppLogicService appLogicService)
        {
            _logger = loggerFactory.CreateLogger<SecondaryCosmosFunction>();
            _appLogicService = appLogicService;
        }

        [Function("SecondaryCosmosFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "AnotherDB",
            containerName: "SecondaryContainer",
            Connection = "CosmosDbConnectionString",
            LeaseContainerName = "leases_secondary",
            CreateLeaseContainerIfNotExists = false)] IReadOnlyList<MyDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation($"Secondary Cosmos DB Trigger fired! Processing {input.Count} documents from AnotherDB/SecondaryContainer.");
                foreach (var doc in input)
                {
                    // Reusing the same simulated application logic for the test
                    await _appLogicService.ProcessDocumentAsync(doc);
                }
            }
        }
    }
}
*/
