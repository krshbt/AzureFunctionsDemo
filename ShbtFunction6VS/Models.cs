using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShbtFunction6VS
{
    class Models
    {
    }

    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;
    }

    public class TodoCreateModel
    {
        public string Description { get; set; }
    }

    public class TodoUpdateModel
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoTableEntity : TableEntity
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }

    public static class Mapper
    {
        public static TodoTableEntity ToTableEntity(Todo todo)
        {
            return new TodoTableEntity()
            {
                PartitionKey = "Todo",
                RowKey = todo.Id,
                Created = todo.Created,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted
            };
        }

        public static Todo ToTodo(TodoTableEntity todoRow)
        {
            return new Todo()
            {
                Id = todoRow.RowKey,
                Created = todoRow.Created,
                Description = todoRow.Description,
                IsCompleted = todoRow.IsCompleted
            };
        }
    }
}
