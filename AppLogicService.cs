using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BaselineFunctionApp
{
    public interface IAppLogicService
    {
        Task ProcessDocumentAsync(MyDocument document);
    }

    public class AppLogicService : IAppLogicService
    {
        private readonly ILogger<AppLogicService> _logger;

        public AppLogicService(ILogger<AppLogicService> logger)
        {
            _logger = logger;
        }

        public async Task ProcessDocumentAsync(MyDocument document)
        {
            _logger.LogInformation($"[App Logic] Starting processing for Document ID: {document.Id}");
            
            // Simulating application logic processing
            await Task.Delay(50);
            
            _logger.LogInformation($"[App Logic] Extracted Cosmos Data: ID={document.Id}");
            
            // Simulating further saving or side effects
            await Task.Delay(50);

            _logger.LogInformation($"[App Logic] Processing process finished for Document ID: {document.Id}");
        }
    }
}
