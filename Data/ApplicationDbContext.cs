using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using parcial.Models;

namespace parcial.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    public override int SaveChanges()
    {
        ValidateCupoMaximo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateCupoMaximo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateCupoMaximo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateCupoMaximo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Curso>(entity =>
        {
            entity.HasIndex(c => c.Codigo).IsUnique();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Cursos_Creditos", "Creditos > 0");
                table.HasCheckConstraint("CK_Cursos_CupoMaximo", "CupoMaximo > 0");
                table.HasCheckConstraint("CK_Cursos_Horario", "HorarioInicio < HorarioFin");
            });
        });

        builder.Entity<Matricula>(entity =>
        {
            entity.HasIndex(m => new { m.UsuarioId, m.CursoId }).IsUnique();
            entity.Property(m => m.Estado).HasConversion<string>();
            entity.Property(m => m.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(m => m.Curso)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ValidateCupoMaximo()
    {
        var nuevasMatriculas = ChangeTracker.Entries<Matricula>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        if (nuevasMatriculas.Count == 0)
        {
            return;
        }

        var porCurso = nuevasMatriculas
            .GroupBy(m => m.CursoId)
            .ToDictionary(g => g.Key, g => g.Count());

        var cursoIds = porCurso.Keys.ToList();

        var cupos = Cursos
            .Where(c => cursoIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CupoMaximo })
            .ToDictionary(c => c.Id, c => c.CupoMaximo);

        var ocupados = Matriculas
            .Where(m => cursoIds.Contains(m.CursoId) && m.Estado != EstadoMatricula.Cancelada)
            .GroupBy(m => m.CursoId)
            .Select(g => new { CursoId = g.Key, Total = g.Count() })
            .ToDictionary(x => x.CursoId, x => x.Total);

        foreach (var (cursoId, nuevasEnCurso) in porCurso)
        {
            if (!cupos.TryGetValue(cursoId, out var cupoMaximo))
            {
                throw new InvalidOperationException($"El curso {cursoId} no existe.");
            }

            var ocupadosActuales = ocupados.TryGetValue(cursoId, out var totalActual) ? totalActual : 0;
            if (ocupadosActuales + nuevasEnCurso > cupoMaximo)
            {
                throw new InvalidOperationException($"Se excedio el cupo maximo del curso {cursoId}.");
            }
        }
    }
}
