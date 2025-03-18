using System;

namespace BasicTokenDemo.Models;

public class RefreshTokenInfo
{
    public required string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
