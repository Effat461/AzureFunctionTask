using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionDemo.Models
{
    public class MessageEntity : TableEntity
    {
        public MessageEntity() { }
        public MessageEntity(string message, string messageType)
        {
            this.PartitionKey = message; this.RowKey = messageType;
        }
        public string message { get; set; }
        public string messageType { get; set; }
    }
    
}
