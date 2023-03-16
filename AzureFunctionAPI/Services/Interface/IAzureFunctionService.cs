using Azure.Storage.Blobs.Models;
using AzureFunction.Models;

namespace AzureFunctionAPI.Services.Interface
{
    public interface IAzureFunctionService
    {
         Task<IEnumerable<BlobItem>> GetAllLogsByName(string name);
         Task<MessageEntity> GetMessageLogs(string partitionKey, DateTimeOffset from, DateTimeOffset to);
    }
}
