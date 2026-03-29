using CassMach.Domain.Entities;
using CassMach.Domain.Common.Enums;
using CassMach.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CassMach.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Clear existing data if needed
                await ClearExistingDataAsync(context, logger);

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);

                // Seed Tenants
                await SeedTenantsAsync(context);
                
                // Seed Admin User
                await SeedAdminUserAsync(context);



                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        public static async Task SeedAsyncIfEmpty(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Check if database has any data
                var hasAnyData = await context.Users.AnyAsync() || 
                                await context.Roles.AnyAsync() || 
                                await context.Permissions.AnyAsync() ||
                                await context.Tenants.AnyAsync();
            ;

            if (hasAnyData)
                {
                    logger.LogInformation("Database already contains data. Skipping seed data.");
                    return;
                }

                logger.LogInformation("Database is empty. Starting seed data process...");

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);
                
                // Seed Tenants
                await SeedTenantsAsync(context);
            
                // Seed Admin User
                await SeedAdminUserAsync(context);

                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        private static async Task ClearExistingDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Clearing existing data...");

                // Clear in correct order to avoid foreign key constraints
                context.UserRoles.RemoveRange(context.UserRoles);
                context.RolePermissions.RemoveRange(context.RolePermissions);
                context.Users.RemoveRange(context.Users);
                context.Tenants.RemoveRange(context.Tenants);
                context.Roles.RemoveRange(context.Roles);
                context.Permissions.RemoveRange(context.Permissions);
                context.RefreshTokens.RemoveRange(context.RefreshTokens);

                await context.SaveChangesAsync();
                logger.LogInformation("Existing data cleared successfully.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error clearing existing data, continuing with seed...");
            }
        }

        public static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            // Get all permissions from static constants
            var allPermissions = Permissions.Helper.GetAllPermissions();

            // Veritabanındaki mevcut permission isimlerini al
            var existingPermissionNames = await context.Permissions
                .Select(p => p.Name)
                .ToListAsync();

            var newPermissions = new List<Permission>();

            foreach (var permissionName in allPermissions)
            {
                if (existingPermissionNames.Contains(permissionName))
                    continue; // Zaten varsa ekleme

                var parts = permissionName.Split('.');
                if (parts.Length == 2)
                {
                    var resource = parts[0];
                    var action = parts[1];
                    var permissionType = MapActionToPermissionType(action);

                    newPermissions.Add(new Permission
                    {
                        Name = permissionName,
                        Description = $"Can {action.ToLower()} {resource.ToLower()}",
                        Resource = resource,
                        Type = permissionType
                    });
                }
            }

            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Yeni eklenen permission'ları ilgili rollere bağlar (ilk seed sonrası veya migration sonrası).
        /// SuperAdmin/Admin: tüm izinler; User: Users + Errors (hata asistanı).
        /// </summary>
        public static async Task SyncRolePermissionsAsync(ApplicationDbContext context)
        {
            var allPermissions = await context.Permissions.AsNoTracking().ToListAsync();
            if (allPermissions.Count == 0)
                return;

            var superAdmin = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.SuperAdmin);
            var admin = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.User);

            if (superAdmin != null)
                AddMissingRolePermissions(context, superAdmin.Id, allPermissions.Select(p => p.Id));

            if (admin != null)
                AddMissingRolePermissions(context, admin.Id, allPermissions.Select(p => p.Id));

            if (userRole != null)
            {
                var userNames = Permissions.Helper.GetPermissionsByResource("Users");
                var errorsNames = Permissions.Helper.GetPermissionsByResource("Errors");
                var allowed = userNames.Concat(errorsNames).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var ids = allPermissions.Where(p => allowed.Contains(p.Name)).Select(p => p.Id);
                AddMissingRolePermissions(context, userRole.Id, ids);
            }

            await context.SaveChangesAsync();
        }

        private static void AddMissingRolePermissions(ApplicationDbContext context, int roleId, IEnumerable<int> permissionIds)
        {
            var existingIds = context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToList()
                .ToHashSet();

            foreach (var pid in permissionIds)
            {
                if (existingIds.Contains(pid))
                    continue;
                context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
                existingIds.Add(pid);
            }
        }

        private static PermissionType MapActionToPermissionType(string action)
        {
            return action.ToLower() switch
            {
                "read" => PermissionType.Read,
                "create" => PermissionType.Create,
                "update" => PermissionType.Update,
                "delete" => PermissionType.Delete,
                _ => PermissionType.Read // Default to Read
            };
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            var superAdminRole = new Role
            {
                Name = RoleNames.SuperAdmin,
                Description = "System super administrator with full access including sensitive operations",
                IsSystemRole = true
            };

            var adminRole = new Role
            {
                Name = RoleNames.Admin,
                Description = "System administrator with full access",
                IsSystemRole = true
            };

            var userRole = new Role
            {
                Name = RoleNames.User,
                Description = "Standard user with user management permissions only",
                IsSystemRole = true
            };

            await context.Roles.AddRangeAsync(superAdminRole, adminRole, userRole);
            await context.SaveChangesAsync();

            // Get all permissions and assign to super admin and admin roles
            var allPermissions = await context.Permissions.ToListAsync();
            var superAdminRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = superAdminRole.Id,
                PermissionId = p.Id
            });
            var adminRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            });

            await context.RolePermissions.AddRangeAsync(superAdminRolePermissions);
            await context.RolePermissions.AddRangeAsync(adminRolePermissions);

            // Get only Users permissions and assign to user role
            var userPermissionNames = Permissions.Helper.GetPermissionsByResource("Users");
            var userPermissions = allPermissions.Where(p => userPermissionNames.Contains(p.Name)).ToList();
            var userRolePermissions = userPermissions.Select(p => new RolePermission
            {
                RoleId = userRole.Id,
                PermissionId = p.Id
            });

            await context.RolePermissions.AddRangeAsync(userRolePermissions);
            await context.SaveChangesAsync();
        }

    private static async Task SeedTenantsAsync(ApplicationDbContext context)
    {
        // Check if tenants already exist
        if (await context.Tenants.AnyAsync())
        {
            return; // Tenants already seeded
        }

        var defaultTenant = new Tenant
        {
            Name = "Default Tenant",
            Description = "Default system tenant",
            Domain = "default",
            IsActive = true,
            ContactEmail = "admin@default.com",
            ContactPhone = "+905551234567",
            CreatedDate = DateTime.UtcNow
        };

        var demoTenant = new Tenant
        {
            Name = "Demo Tenant",
            Description = "Demo tenant for testing purposes",
            Domain = "demo",
            IsActive = true,
            ContactEmail = "admin@demo.com",
            ContactPhone = "+905559876543",
            CreatedDate = DateTime.UtcNow
        };

        await context.Tenants.AddRangeAsync(defaultTenant, demoTenant);
        await context.SaveChangesAsync();
    }

        private static async Task SeedAdminUserAsync(ApplicationDbContext context)
        {
            // Get default tenant
            var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Domain == "default");

            var superAdminUser = new User
            {
                FirstName = "Super",
                LastName = "Administrator",
                Email = "superadmin@cassmach.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!"), // Default password
                PhoneNumber = "+905550000001",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
                TenantId = defaultTenant?.Id,
                CreatedDate = DateTime.UtcNow
            };

            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@cassmach.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Default password
                PhoneNumber = "+905551234567",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
                TenantId = defaultTenant?.Id,
                CreatedDate = DateTime.UtcNow
            };

            var standardUser = new User
            {
                FirstName = "Standard",
                LastName = "User",
                Email = "user@cassmach.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"), // Default password
                PhoneNumber = "+905559998877",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
                TenantId = defaultTenant?.Id,
                CreatedDate = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(superAdminUser, adminUser, standardUser);
            await context.SaveChangesAsync();

            // Assign super admin role to super admin user
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.SuperAdmin);
            if (superAdminRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = superAdminUser.Id,
                    RoleId = superAdminRole.Id
                });
            }

            // Assign admin role to admin user
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);
            if (adminRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
            }

            // Assign user role to standard user
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.User);
            if (userRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = standardUser.Id,
                    RoleId = userRole.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }
} 
