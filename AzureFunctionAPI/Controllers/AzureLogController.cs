using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using AzureFunction.Models;
using Azure.Storage.Blobs.Models;
using AzureFunctionAPI.Services.Interface;

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
        public async Task<MessageEntity> GetMessageLogs(string partitionKey, DateTimeOffset from, DateTimeOffset to)
         {
            //var connectString =_configuration.GetValue<string>("AzureWebJobsStorage");
            //var account = CloudStorageAccount.Parse(connectString);
            //var client = account.CreateCloudTableClient();
            //var table = client.GetTableReference("tblsuccessfaliuremessagelog");
            ////string finalFilter = TableQuery.CombineFilters(
            ////                        TableQuery.CombineFilters(partitionKey, TableOperators.And, "logdatetime ge datetime'"+from.ToString("yyyy-MM-dd") +"'"), TableOperators.And, "logdatetime le datetime'"+to.ToString("yyyy-MM-dd") +"'");
            //string finalFilter = TableQuery.CombineFilters(
            //            TableQuery.CombineFilters(partitionKey, TableOperators.And, from.ToString()), TableOperators.And, to.ToString());
            //TableOperation tableOperation = TableOperation.Retrieve<MessageEntity>(partitionKey,finalFilter);
            //TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return await _azureFunctionService.GetMessageLogs(partitionKey,from,to);

        }
        [Route("/GetAllLogsByName")]
        [HttpGet]
        public async Task<IEnumerable<BlobItem>> GetAllLogsByName(string name)
        {
            try
            {
              
                return await _azureFunctionService.GetAllLogsByName(name);

            }
            catch (Exception)
            {

                throw;
            }
            
        }
        

    }
}
