using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Storage.Blobs;
using AzureFunctionDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureFunctionDemo
{
    public class AzureFunction
    {
        private readonly ILogger _logger;

        public AzureFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AzureFunction>();
        }

        [Function("AzureFunction")]
        public async Task<string> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {

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
                string logFileName = "payloadlogFile";
                await WriteLog(containerName, logFileName, blobClient, resp);
                await InsertMessageToAzureTable("Operation compeleted successfully", "success");
                return resp;
            }
            catch (Exception ex)
            {
                await InsertMessageToAzureTable(ex.Message, "failure");
                throw;
            }

        }
        static async Task WriteLog(string nameContainer, string nameLogFile, CloudBlobClient objBlobClient, string newValue)
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

        static async Task InsertMessageToAzureTable(string msg, string messageType)
        {
            var connectString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var account = CloudStorageAccount.Parse(connectString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("tblsuccessfaliuremessagelog");
            MessageEntity messageEntity = new MessageEntity(msg,messageType)
            {
                PartitionKey = msg,
                RowKey = messageType,
            };
            TableOperation insertOperation = TableOperation.Insert(messageEntity);
            await table.ExecuteAsync(insertOperation);


        }


        public async Task<bool> CreateNewTable(CloudTable table)
        {
            if (!(await table.CreateIfNotExistsAsync()))
            {
                Console.WriteLine("Table {0} already exists", table.Name);
                return true;
            }
            Console.WriteLine("Table {0} created", table.Name);
            return true;
        }
        
      
    }
}

