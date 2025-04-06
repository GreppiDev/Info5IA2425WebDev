using System;

namespace BackendApp.Models;

public class PasswordChangeRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

