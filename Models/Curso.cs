using System.ComponentModel.DataAnnotations;

namespace parcial.Models;

public class Curso
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Creditos { get; set; }

    [Range(1, int.MaxValue)]
    public int CupoMaximo { get; set; }

    public TimeSpan HorarioInicio { get; set; }

    public TimeSpan HorarioFin { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
