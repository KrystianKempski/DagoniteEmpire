using DA_Common;
using DA_DataAccess.Data;
using DagoniteEmpire.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DagoniteEmpire.Service
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_HeroPlayer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_DukePlayer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_GameMaster)).GetAwaiter().GetResult();
                }
                else
                {
                    return;
                }

                IdentityUser user = new()
                {
                    UserName = "krystian.kempski@gmail.com",
                    Email = "krystian.kempski@gmail.com",
                    EmailConfirmed = true,
                };

                _userManager.CreateAsync(user, "Admin123*").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

                user = new()
                {
                    UserName = "guestplayer",
                    Email = "guestplayer@gmail.com",
                    EmailConfirmed = true,
                };

                _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer).GetAwaiter().GetResult();

                user = new()
                {
                    UserName = "guestGM",
                    Email = "guestGM@gmail.com",
                    EmailConfirmed = true,
                };

                _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, SD.Role_GameMaster).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
