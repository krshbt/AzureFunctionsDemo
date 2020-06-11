using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ShbtFunction6VS
{
    public static class QueueFunction
    {
        [FunctionName("TodoQueueAdded")]
        public static async Task TodoQueueAdded([QueueTrigger("todos", Connection = "AzureWebJobsStorage")]Todo todoItem,
            [Blob("todos", Connection = "AzureWebJobsStorage")]CloudBlobContainer container,
            ILogger log)
        {
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference($"{todoItem.Id}.txt");
            await blob.UploadTextAsync($"Created new task with id:{todoItem.Id} and decsription {todoItem.Description}");
            log.LogInformation($"C# Queue trigger function processed: {todoItem}");
        }
    }
}
