using Mango.Services.Identity.DbContext;
using Mango.Services.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Identity.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
                    ApplicationDbContext db,
                    UserManager<ApplicationUser> userManager,
                    RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch
            {
                throw;
            }

            if (_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                return;
            if (_roleManager.RoleExistsAsync("Customer").GetAwaiter().GetResult())
                return;

            _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole("Customer")).GetAwaiter().GetResult();

            var adminUser = new ApplicationUser
            {
                UserName = "admin@mango.com",
                Email = "admin@mango.com",
                EmailConfirmed = true,
                PhoneNumber = "123456789",
                FirstName = "Jack",
                LastName = "Admin"
            };

            _userManager.CreateAsync(adminUser, "Admin123!").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();

            var customerUser = new ApplicationUser
            {
                UserName = "customer@mango.com",
                Email = "customer@mango.com",
                EmailConfirmed = true,
                PhoneNumber = "123456789",
                FirstName = "Alan",
                LastName = "Customer"
            };

            _userManager.CreateAsync(customerUser, "Customer123!").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(customerUser, "Customer").GetAwaiter().GetResult();
        }
    }
}
