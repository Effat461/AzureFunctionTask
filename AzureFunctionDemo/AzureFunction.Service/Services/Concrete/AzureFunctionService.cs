using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunction.Models;
using AzureFunction.Service.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunction.Service.Services.Concrete
{
    public class AzureFunctionService : IAzureFunctionService
    {
        private readonly IConfiguration _configuration;
        public AzureFunctionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<MessageEntity> GetMessageLogs(string partitionKey, DateTime from, DateTime to)
        {


            var table = GetTableName();
            string DateFromFilter = TableQuery.GenerateFilterConditionForDate("CreatedDate", QueryComparisons.Equal, from);
            string DateUntilFilter = TableQuery.GenerateFilterConditionForDate("CreatedDate", QueryComparisons.LessThanOrEqual, to);
            string finalFilter = TableQuery.CombineFilters(
                                    TableQuery.CombineFilters(partitionKey, TableOperators.And, DateFromFilter.ToString()), TableOperators.And, DateUntilFilter.ToString());
            TableOperation tableOperation = TableOperation.Retrieve<MessageEntity>(partitionKey,finalFilter);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as MessageEntity;

        }
        public async Task<IEnumerable<BlobItem>> GetAllLogsByBlobItemName(string name)
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
