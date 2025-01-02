namespace TodoApi.Models;

public class Todo
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required Visibility Visibility { get; set; }

    public string? Attachment { get; set; }
}

public enum Visibility
{
    Public,
    Private
}
