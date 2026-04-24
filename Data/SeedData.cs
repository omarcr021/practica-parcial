using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using parcial.Models;

namespace parcial.Data;

public static class SeedData
{
    private const string CoordinadorRole = "Coordinador";
    private const string CoordinadorEmail = "coordinador@universidad.edu";
    private const string CoordinadorPassword = "Coord!2026";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync(CoordinadorRole))
        {
            await roleManager.CreateAsync(new IdentityRole(CoordinadorRole));
        }

        var coordinador = await userManager.FindByEmailAsync(CoordinadorEmail);
        if (coordinador is null)
        {
            coordinador = new ApplicationUser
            {
                UserName = CoordinadorEmail,
                Email = CoordinadorEmail,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(coordinador, CoordinadorPassword);
            if (!userResult.Succeeded)
            {
                var errors = string.Join("; ", userResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario coordinador: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(coordinador, CoordinadorRole))
        {
            await userManager.AddToRoleAsync(coordinador, CoordinadorRole);
        }

        if (await context.Cursos.AnyAsync())
        {
            return;
        }

        context.Cursos.AddRange(
            new Curso
            {
                Codigo = "MAT101",
                Nombre = "Matematica I",
                Creditos = 4,
                CupoMaximo = 30,
                HorarioInicio = new TimeSpan(8, 0, 0),
                HorarioFin = new TimeSpan(10, 0, 0),
                Activo = true
            },
            new Curso
            {
                Codigo = "PRO201",
                Nombre = "Programacion I",
                Creditos = 5,
                CupoMaximo = 25,
                HorarioInicio = new TimeSpan(10, 30, 0),
                HorarioFin = new TimeSpan(12, 30, 0),
                Activo = true
            },
            new Curso
            {
                Codigo = "FIS110",
                Nombre = "Fisica General",
                Creditos = 3,
                CupoMaximo = 35,
                HorarioInicio = new TimeSpan(14, 0, 0),
                HorarioFin = new TimeSpan(16, 0, 0),
                Activo = true
            });

        await context.SaveChangesAsync();
    }
}
