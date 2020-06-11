using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;

namespace ShbtFunction6VS
{
    public static class MyFunction1
    {
        //[FunctionName("MyFunction1")]
        //public static async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
        [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
        [Queue("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue,
        ILogger log)
        {
            log.LogInformation("Creating a new todo list item.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);
            Todo newTodoItem = new Todo() { Description = input.Description };
            await todoTable.AddAsync(Mapper.ToTableEntity(newTodoItem));
            await todoQueue.AddAsync(newTodoItem);
            return new OkObjectResult(newTodoItem);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodosAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Get all todo list item.");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(x => Mapper.ToTodo(x)));
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "Todo", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todoItem,
            ILogger log, string id)
        {
            log.LogInformation("Get todo list item by id.");
            if (todoItem == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(Mapper.ToTodo(todoItem));
        }

        [FunctionName("UpdateTodoById")]
        public static async Task<IActionResult> UpdateTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            log.LogInformation("Get todo list item by id.");
            var findOperation = TableOperation.Retrieve<TodoTableEntity>("Todo", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            var todoItem = (TodoTableEntity)findResult.Result;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            todoItem.IsCompleted = input.IsCompleted;
            todoItem.Description = !string.IsNullOrEmpty(input.Description) ? input.Description : todoItem.Description;
            var replaceOpration = TableOperation.Replace(todoItem);
            await todoTable.ExecuteAsync(replaceOpration);
            return new OkObjectResult(Mapper.ToTodo(todoItem));
        }

        [FunctionName("DeleteTodoById")]
        public static async Task<IActionResult> DeleteTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            log.LogInformation("Get todo list item by id.");
            try
            {
                var deleteOperation = TableOperation.Delete(new TodoTableEntity()
                {
                    PartitionKey = "Todo",
                    RowKey = id,
                    ETag = "*"
                });
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
