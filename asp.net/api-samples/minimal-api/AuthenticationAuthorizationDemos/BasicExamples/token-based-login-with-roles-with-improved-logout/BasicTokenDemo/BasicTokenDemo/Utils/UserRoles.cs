using System;

namespace BasicTokenDemo.Utils;

// Array statici per ruoli comuni
static class UserRoles
{
    public static readonly string[] ViewerRoles = ["Viewer"];
    public static readonly string[] AdminRoles = ["Administrator", "SuperAdministrator"];
    public static readonly string[] EmptyRoles = [];
}
