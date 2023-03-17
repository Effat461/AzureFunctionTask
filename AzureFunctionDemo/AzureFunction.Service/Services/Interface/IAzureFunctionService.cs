using Azure.Storage.Blobs.Models;
using AzureFunction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunction.Service.Services.Interface
{
    public interface IAzureFunctionService
    {
        Task<MessageEntity> GetMessageLogs(string partitionKey, DateTime from, DateTime to);
        Task<IEnumerable<BlobItem>> GetAllLogsByBlobItemName(string name);
    }
}
