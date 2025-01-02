using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Models;

public record struct NewTodoRequest([FromForm] string Name,
	[FromForm] Visibility Visibility, IFormFile? Attachment);