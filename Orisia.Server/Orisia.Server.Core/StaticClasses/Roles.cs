namespace Orisia.Server.Core.StaticClasses;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string User = "User";
    public const string AdminOrEditor = Admin + "," + Editor;

    public static bool IsValid(string? role)
    {
        return role is Admin or Editor or User;
    }
}
