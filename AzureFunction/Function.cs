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
        private  readonly ILogger _logger;

        public Function(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function>();
        }

        [Function("AzureFunction")]
        public async Task<string>  Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            try
            {
                var client = new HttpClient();
                var request = GetURL();
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();
                await WriteLog(GetContainerName(), SetFileName(), GetBlobClient(), resp);
                await InsertMessageToAzureTable(Guid.NewGuid().ToString(), "success");
                return resp;
            }
            catch (Exception ex)
            {
                await InsertMessageToAzureTable(Guid.NewGuid().ToString(), ex.Message);
            }
            return string.Empty;
        }
        static async Task WriteLog(string nameContainer, string nameLogFile, CloudBlobClient objBlobClient, string payload)
        {
            try
            {
                var container = objBlobClient.GetContainerReference(nameContainer.ToString());
                var blob = container.GetAppendBlobReference(nameLogFile);
                bool isPresent = await blob.ExistsAsync();
                if (!isPresent)
                {
                    await blob.CreateOrReplaceAsync();
                }
                await blob.AppendTextAsync($"{payload} \n");
            }
            catch (Exception ex)
            {
              
            }
        }

        public static  async Task InsertMessageToAzureTable(string guid, string message)
        {
            try
            {
                var table = GetTableClient();
                CreateTableLog(guid,message,table);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private static async Task CreateTableLog(string guid, string message, CloudTable table) 
        {
            MessageEntity messageEntity = new MessageEntity(guid, message)
            {
                PartitionKey = guid,
                RowKey = message,
                CreatedDate = DateTime.Now.ToString("MM/dd/yyyy")
            };
            TableOperation insertOperation = TableOperation.Insert(messageEntity);
            await table.ExecuteAsync(insertOperation);
        }
        private async Task<HttpResponseMessage> GetAPIResponse() 
        {
            var client = new HttpClient();
            var request = GetURL();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
        private static string GetConnectionString() {
            string connectString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            return connectString;
        }
        private static string GetContainerName()
        {
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            return containerName;
        }
        private HttpRequestMessage GetURL() {
            var request = new HttpRequestMessage(HttpMethod.Get, Environment.GetEnvironmentVariable("APIURL"));
            return request;
        }
        private string SetFileName() {
            string logFileName = "payloadlogFile" + DateTime.Now.ToString("yyyyMMddHHmmss");
            return logFileName;
        }
        private CloudBlobClient GetBlobClient() {
            string connectString = GetConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }
        private static CloudTable GetTableClient() {
            var connectString = GetConnectionString();
            var account = CloudStorageAccount.Parse(connectString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("tblsuccessfaliuremessagelog");
            return table;
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
