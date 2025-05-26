using System.ComponentModel.DataAnnotations;

namespace MyWebApiApp.Models;
public class TestEntry
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Message { get; set; }=null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}