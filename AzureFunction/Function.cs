using System;
using AzureFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunction
{
    public class Function
    {
        private readonly ILogger _logger;

        public Function(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function>();
        }

        [Function("AzureFunction")]
        public async Task<string> Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            try
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Environment.GetEnvironmentVariable("APIURL"));
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();
                string connectString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var storageAccount = CloudStorageAccount.Parse(connectString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                string containerName = Environment.GetEnvironmentVariable("ContainerName");
                string logFileName = "payloadlogFile" + DateTime.Now.ToString("yyyyMMddHHmmss");
                await WriteLog(containerName, logFileName, blobClient, resp);
                await InsertMessageToAzureTable(Guid.NewGuid().ToString(), "success");
                return resp;
            }
            catch (Exception ex)
            {
                await InsertMessageToAzureTable(Guid.NewGuid().ToString(), ex.Message);
                throw;
            }
        }
        static async Task WriteLog(string nameContainer, string nameLogFile, CloudBlobClient objBlobClient, string newValue)
        {
            try
            {
                // take Container's reference
                var container = objBlobClient.GetContainerReference(nameContainer.ToString());

                // take Blob's reference to modify
                var blob = container.GetAppendBlobReference(nameLogFile);

                // verify the existance of blob
                bool isPresent = await blob.ExistsAsync();

                // if blob doesn't exist, the system will create it
                if (!isPresent)
                {
                    await blob.CreateOrReplaceAsync();
                }

                // append the new value and new line
                await blob.AppendTextAsync($"{newValue} \n");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        static async Task InsertMessageToAzureTable(string guid, string message)
        {
            try
            {
                var connectString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var account = CloudStorageAccount.Parse(connectString);
                var client = account.CreateCloudTableClient();
                var table = client.GetTableReference("tblsuccessfaliuremessagelog");
                MessageEntity messageEntity = new MessageEntity(guid, message)
                {
                    PartitionKey = guid,
                    RowKey = message,
                };
                TableOperation insertOperation = TableOperation.Insert(messageEntity);
                await table.ExecuteAsync(insertOperation);

            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
