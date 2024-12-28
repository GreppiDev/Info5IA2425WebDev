namespace TodoApiV2;

using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

public class TodoItemDTO //: IEndpointParameterMetadataProvider
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) =>
    (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);

    // public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    // {
    //    builder.Metadata.Add(new ConsumesAttribute(typeof(TodoItemDTO),"application/xml"));
    // }
}

