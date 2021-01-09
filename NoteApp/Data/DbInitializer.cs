using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NoteApp.Models;

namespace NoteApp.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            //Initialize roles
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Admin", "Default" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                // ensure that the role does not exist
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // find the admin user
            var _user = await UserManager.FindByNameAsync("admin@email.com");

            // check if the user exists
            if (_user == null)
            {
                var poweruser = new ApplicationUser
                {
                    UserName = "admin@email.com",
                    Email = "admin@email.com",
                    EmailConfirmed = true
                };
                string adminPassword = "Admin1!";

                var createPowerUser = await UserManager.CreateAsync(poweruser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    // tie new user to roles
                    await UserManager.AddToRoleAsync(poweruser, "Default");
                    await UserManager.AddToRoleAsync(poweruser, "Admin");
                    await UserManager.SetLockoutEnabledAsync(poweruser, false);
                }
            }

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                && await UserManager.FindByNameAsync("matt@matt.com") == null)
            {
                await SeedDatabase(serviceProvider);
            }
        }

        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var users = new ApplicationUser[]
            {
                new ApplicationUser
                {
                    UserName = "matt@matt.com",
                    Email = "matt@matt.com",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "jane@jane.com",
                    Email = "jane@jane.com",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "bob@bob.com",
                    Email = "bob@bob.com",
                    EmailConfirmed = true
                }
            };

            string userPassword = "Password1!";

            foreach (ApplicationUser user in users)
            {
                var createUser = await UserManager.CreateAsync(user, userPassword);
                if (createUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user, "Default");
                }
            }

            var matt = UserManager.FindByNameAsync("matt@matt.com").Result;
            var jane = UserManager.FindByNameAsync("jane@jane.com").Result;
            var bob = UserManager.FindByNameAsync("bob@bob.com").Result;

            await UserManager.SetLockoutEndDateAsync(bob, DateTimeOffset.Now.AddYears(10));

            var notes = new Note[]
            {
                new Note
                {
                    Title = "Fix Car",
                    Priority = Priority.Normal,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = matt
                },
                new Note
                {
                    Title = "Get Prescription",
                    Priority = Priority.High,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = matt
                },
                new Note
                {
                    Title = "Vac House",
                    Priority = Priority.Low,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = matt
                },
                new Note
                {
                    Title = "Do Washing",
                    Priority = Priority.Normal,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = jane
                },
                new Note
                {
                    Title = "Cook Dinner",
                    Priority = Priority.High,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = jane
                },
                new Note
                {
                    Title = "Clean Windows",
                    Priority = Priority.Low,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = bob
                },
                new Note
                {
                    Title = "Get Shopping",
                    Priority = Priority.High,
                    Comments = "",
                    CreatedDate = DateTime.Now,
                    User = bob
                }
            };

            foreach (Note n in notes)
            {
                context.Notes.Add(n);
            }
            await context.SaveChangesAsync();
        }
    }
}
