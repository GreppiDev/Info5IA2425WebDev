using System;
using System.ComponentModel.DataAnnotations;

namespace EducationalGames.Models;

public class EmailSettings
{
    [Required]
    public string SmtpServer { get; set; } = string.Empty;

    [Required]
    [Range(1, 65535)]
    public int Port { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    public string SenderName { get; set; } = string.Empty;
}
