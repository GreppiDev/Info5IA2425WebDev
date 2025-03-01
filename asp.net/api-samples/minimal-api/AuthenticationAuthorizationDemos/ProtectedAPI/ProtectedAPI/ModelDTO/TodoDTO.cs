using System;
using ProtectedAPI.Model;

namespace ProtectedAPI.ModelDTO;

public class TodoDTO
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool IsCompleted { get; set; }
	public string? OwnerId { get; set; }
	public DateTime CreatedAt { get; set; }

	public TodoDTO()
	{

	}
	public TodoDTO(Todo todo)
	{
		Id = todo.Id;
		Title = todo.Title;
		Description = todo.Description;
		IsCompleted = todo.IsComplete;
		OwnerId = todo.OwnerId;
		CreatedAt = todo.CreatedAt;
	}
}
