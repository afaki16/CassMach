namespace CassMach.Domain.Constants
{
    public static class Permissions
{
    // User permissions
    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    // Role permissions
    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    // Permission permissions
    public static class Permission
    {
        public const string Read = "Permissions.Read";
        public const string Create = "Permissions.Create";
        public const string Update = "Permissions.Update";
        public const string Delete = "Permissions.Delete";
    }

    // Tenant permissions
    public static class Tenants
    {
        public const string Read = "Tenants.Read";
        public const string Create = "Tenants.Create";
        public const string Update = "Tenants.Update";
        public const string Delete = "Tenants.Delete";
    }

    // Error assistant permissions
    public static class Errors
    {
        public const string Read = "Errors.Read";
        public const string Create = "Errors.Create";
        public const string Update = "Errors.Update";
        public const string Delete = "Errors.Delete";
    }

    // Machine catalog permissions (admin only)
    public static class Machines
    {
        public const string Read = "Machines.Read";
        public const string Create = "Machines.Create";
        public const string Update = "Machines.Update";
        public const string Delete = "Machines.Delete";
    }

    // User machine assignment permissions
    public static class UserMachines
    {
        public const string Read = "UserMachines.Read";
        public const string Create = "UserMachines.Create";
        public const string Delete = "UserMachines.Delete";
    }

    // Admin panel permissions
    public static class AdminPanel
    {
        public const string Read = "AdminPanel.Read";
        public const string Create = "AdminPanel.Create";
        public const string Update = "AdminPanel.Update";
        public const string Delete = "AdminPanel.Delete";
    }

    // Helper methods
    public static class Helper
    {
        public static string[] GetAllPermissions()
        {
            return typeof(Permissions)
                .GetNestedTypes()
                .SelectMany(t => t.GetFields())
                .Where(f => f.IsStatic && f.IsLiteral && !f.IsInitOnly)
                .Select(f => f.GetValue(null)?.ToString())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray()!;
        }

        public static string[] GetPermissionsByResource(string resource)
        {
            var resourceType = typeof(Permissions).GetNestedType(resource);
            if (resourceType == null)
                return Array.Empty<string>();

            return resourceType.GetFields()
                .Where(f => f.IsStatic && f.IsLiteral && !f.IsInitOnly)
                .Select(f => f.GetValue(null)?.ToString())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray()!;
        }

        public static string[] GetResources()
        {
            return typeof(Permissions)
                .GetNestedTypes()
                .Where(t => t.IsClass && t.IsNestedPublic)
                .Select(t => t.Name)
                .ToArray();
        }
    }
}
} 
