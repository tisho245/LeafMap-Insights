using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Models;

namespace LeafMap_Insights.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Users and Admins
            await SeedUsersAsync(userManager);

            // Seed Taxonomy
            await SeedTaxonomyAsync(context);

            // Seed Trees
            await SeedTreesAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
        {
            // Admin users
            var admins = new[]
            {
                new { Email = "admin@leafmap.com", Password = "Admin@123", Name = "Главен администратор" },
                new { Email = "teacher@leafmap.com", Password = "Teacher@123", Name = "Учител" }
            };

            foreach (var admin in admins)
            {
                var user = await userManager.FindByEmailAsync(admin.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = admin.Email,
                        Email = admin.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, admin.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }

            // Regular users
            var users = new[]
            {
                new { Email = "student1@leafmap.com", Password = "Student@123", Name = "Студент 1" },
                new { Email = "student2@leafmap.com", Password = "Student@123", Name = "Студент 2" },
                new { Email = "user@leafmap.com", Password = "User@123", Name = "Потребител" }
            };

            foreach (var userData in users)
            {
                var user = await userManager.FindByEmailAsync(userData.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
        }

        private static async Task SeedTaxonomyAsync(ApplicationDbContext context)
        {
            // Check if data already exists
            if (await context.Families.AnyAsync())
                return;

            // Families
            var fagaceae = new Family { Name = "Букови", Description = "Семейство Букови (Fagaceae)" };
            var rosaceae = new Family { Name = "Розови", Description = "Семейство Розови (Rosaceae)" };
            var pinaceae = new Family { Name = "Борови", Description = "Семейство Борови (Pinaceae)" };
            var betulaceae = new Family { Name = "Брезови", Description = "Семейство Брезови (Betulaceae)" };

            context.Families.AddRange(fagaceae, rosaceae, pinaceae, betulaceae);
            await context.SaveChangesAsync();

            // Genera
            var quercus = new Genus { Name = "Дъб", LatinName = "Quercus", FamilyId = fagaceae.Id, Description = "Род Дъб" };
            var fagus = new Genus { Name = "Бук", LatinName = "Fagus", FamilyId = fagaceae.Id, Description = "Род Бук" };
            var malus = new Genus { Name = "Ябълка", LatinName = "Malus", FamilyId = rosaceae.Id, Description = "Род Ябълка" };
            var pyrus = new Genus { Name = "Круша", LatinName = "Pyrus", FamilyId = rosaceae.Id, Description = "Род Круша" };
            var pinus = new Genus { Name = "Бор", LatinName = "Pinus", FamilyId = pinaceae.Id, Description = "Род Бор" };
            var picea = new Genus { Name = "Смърч", LatinName = "Picea", FamilyId = pinaceae.Id, Description = "Род Смърч" };
            var betula = new Genus { Name = "Бреза", LatinName = "Betula", FamilyId = betulaceae.Id, Description = "Род Бреза" };
            var alnus = new Genus { Name = "Елша", LatinName = "Alnus", FamilyId = betulaceae.Id, Description = "Род Елша" };

            context.Genera.AddRange(quercus, fagus, malus, pyrus, pinus, picea, betula, alnus);
            await context.SaveChangesAsync();

            // Species
            var species = new[]
            {
                new Species { Name = "Обикновен дъб", LatinName = "Quercus robur", GenusId = quercus.Id, Description = "Широкоразпространен вид дъб в Европа" },
                new Species { Name = "Червен дъб", LatinName = "Quercus rubra", GenusId = quercus.Id, Description = "Северноамерикански вид дъб" },
                new Species { Name = "Обикновен бук", LatinName = "Fagus sylvatica", GenusId = fagus.Id, Description = "Европейски бук" },
                new Species { Name = "Ябълка", LatinName = "Malus domestica", GenusId = malus.Id, Description = "Културна ябълка" },
                new Species { Name = "Круша", LatinName = "Pyrus communis", GenusId = pyrus.Id, Description = "Културна круша" },
                new Species { Name = "Бял бор", LatinName = "Pinus sylvestris", GenusId = pinus.Id, Description = "Обикновен бор" },
                new Species { Name = "Европейски смърч", LatinName = "Picea abies", GenusId = picea.Id, Description = "Обикновен смърч" },
                new Species { Name = "Бяла бреза", LatinName = "Betula pendula", GenusId = betula.Id, Description = "Обикновена бреза" },
                new Species { Name = "Черна елша", LatinName = "Alnus glutinosa", GenusId = alnus.Id, Description = "Обикновена елша" }
            };

            context.Species.AddRange(species);
            await context.SaveChangesAsync();
        }

        private static async Task SeedTreesAsync(ApplicationDbContext context)
        {
            // Check if data already exists
            if (await context.Trees.AnyAsync())
                return;

            var species = await context.Species.ToListAsync();
            if (!species.Any())
                return;

            // School coordinates (ПГ "Генерал Владимир Заимов")
            var schoolLat = 42.655997269349136m;
            var schoolLng = 24.74597442565678m;

            // Generate trees around the school area
            var random = new Random(42); // Fixed seed for reproducibility
            var trees = new List<Tree>();

            // Create trees with coordinates near the school
            for (int i = 0; i < 15; i++)
            {
                // Random offset within ~500m radius
                var latOffset = (decimal)(random.NextDouble() * 0.009 - 0.0045); // ~500m
                var lngOffset = (decimal)(random.NextDouble() * 0.009 - 0.0045); // ~500m

                var tree = new Tree
                {
                    SpeciesId = species[random.Next(species.Count)].Id,
                    Latitude = schoolLat + latOffset,
                    Longitude = schoolLng + lngOffset,
                    EstimatedAge = random.Next(5, 150),
                    Height = (decimal)(random.NextDouble() * 25 + 2), // 2-27 meters
                    Condition = new[] { "Добро", "Отлично", "Задоволително", "Слабо" }[random.Next(4)],
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365))
                };

                trees.Add(tree);
            }

            context.Trees.AddRange(trees);
            await context.SaveChangesAsync();

            // Generate QR codes for trees (this will be done when viewing details)
        }
    }
}
