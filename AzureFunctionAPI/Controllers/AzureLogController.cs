using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using AzureFunction.Models;
using Azure.Storage.Blobs.Models;
using AzureFunction.Service.Services.Interface;

namespace AzureFunctionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureLogController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAzureFunctionService _azureFunctionService;
        public AzureLogController(IConfiguration configuration, IAzureFunctionService azureFunctionService)
        {
            _configuration = configuration;
            _azureFunctionService = azureFunctionService;
        }
     
        [Route("/GetMessageLogs")]
        [HttpGet]
        public async Task<MessageEntity> GetMessageLogs(string partitionKey, DateTime from, DateTime to)
        {

            return await _azureFunctionService.GetMessageLogs(partitionKey,from,to);
        }
        [Route("/GetAllLogsByBlobItemName")]
        [HttpGet]
        public async Task<IEnumerable<BlobItem>> GetAllLogsByBlobItemName(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name)) {
                    return await _azureFunctionService.GetAllLogsByBlobItemName(name);
                }
                return new List<BlobItem>();


            }
            catch (Exception)
            {

                throw;
            }
            
        }
        

    }
}
