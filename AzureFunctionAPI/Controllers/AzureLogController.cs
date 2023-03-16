using Azure.Storage.Blobs;
using AzureFunctionDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureLogController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AzureLogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("/GetMessageLogsForSpecificTimeStamp")]
        [HttpGet]
        public async Task<MessageEntity> GetMessageLogsForSpecificTimeStamp(string partitionKey, DateTimeOffset from, DateTimeOffset to)
         {
            var connectString =_configuration.GetValue<string>("AzureWebJobsStorage");
            var account = CloudStorageAccount.Parse(connectString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("tblsuccessfaliuremessagelog");
            //string finalFilter = TableQuery.CombineFilters(
            //                        TableQuery.CombineFilters(partitionKey, TableOperators.And, "logdatetime ge datetime'"+from.ToString("yyyy-MM-dd") +"'"), TableOperators.And, "logdatetime le datetime'"+to.ToString("yyyy-MM-dd") +"'");
            string finalFilter = TableQuery.CombineFilters(
                        TableQuery.CombineFilters(partitionKey, TableOperators.And, from.ToString()), TableOperators.And, to.ToString());
            TableOperation tableOperation = TableOperation.Retrieve<MessageEntity>(partitionKey,finalFilter);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as MessageEntity;

        }
        [Route("/GetAllLogsForSpecificEntry")]
        [HttpGet]
        public async Task<string> GetAllLogsForSpecificEntry(string name)
        {
            var connectString = _configuration.GetValue<string>("AzureWebJobsStorage");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectString);
            string containerName = _configuration.GetValue<string>("ContainerName");
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = containerClient.GetBlobs().Where(x => x.Name == name);

            foreach (var item in blobs)
            {
                Console.WriteLine(item.Name);
            }
            return "OK";
        }   
    }
}
