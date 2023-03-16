using Azure.Core.Extensions;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using AzureFunctionAPI.Services.Interface;
using Microsoft.Extensions.Configuration;
using AzureFunction.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Security.Principal;

namespace AzureFunctionAPI.Services.Concrete
{
    public class AzureFunctionService : IAzureFunctionService
    {
        private readonly IConfiguration _configuration;
        public AzureFunctionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<MessageEntity> GetMessageLogs(string partitionKey, DateTimeOffset from, DateTimeOffset to)
        {

            //string finalFilter = TableQuery.CombineFilters(
            //                        TableQuery.CombineFilters(partitionKey, TableOperators.And, "logdatetime ge datetime'"+from.ToString("yyyy-MM-dd") +"'"), TableOperators.And, "logdatetime le datetime'"+to.ToString("yyyy-MM-dd") +"'");
            var table = GetTableName();
            string finalFilter = TableQuery.CombineFilters(
                        TableQuery.CombineFilters(partitionKey, TableOperators.And, from.ToString()), TableOperators.And, to.ToString());
            TableOperation tableOperation = TableOperation.Retrieve<MessageEntity>(partitionKey, finalFilter);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as MessageEntity;

        }
        public async Task<IEnumerable<BlobItem>> GetAllLogsByName(string name)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(GetConnectionString());
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(GetContainerName());
            var blobs = containerClient.GetBlobs().Where(x => x.Name == name);
            return blobs.ToList();

        }

        private string GetConnectionString()
        {
            string connectString = _configuration.GetValue<string>("AzureWebJobsStorage");
            return connectString;
        }
        private string GetContainerName()
        {
            string containerName = _configuration.GetValue<string>("ContainerName");
            return containerName;
        }
        private CloudTable GetTableName() 
        {
            var account = CloudStorageAccount.Parse(GetConnectionString());
            var client = account.CreateCloudTableClient();
            return client.GetTableReference("tblsuccessfaliuremessagelog");
        }
    }
}
