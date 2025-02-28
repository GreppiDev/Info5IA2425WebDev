namespace ProtectedAPI.Model;

public class IdentitySettings
{
    public bool RequireEmailConfirmation { get; set; } = true;
    public bool UseCustomIdentityEndpoints { get; set; } = false;
}