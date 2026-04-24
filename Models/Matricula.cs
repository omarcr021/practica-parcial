using System.ComponentModel.DataAnnotations;

namespace parcial.Models;

public class Matricula
{
    public int Id { get; set; }

    public int CursoId { get; set; }

    public Curso? Curso { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    public ApplicationUser? Usuario { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
}
