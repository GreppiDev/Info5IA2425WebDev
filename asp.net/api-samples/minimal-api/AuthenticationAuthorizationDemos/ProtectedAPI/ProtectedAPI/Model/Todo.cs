using System;

namespace ProtectedAPI.Model;

public class Todo
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool IsComplete { get; set; }
	public string? Secret { get; set; }
}

