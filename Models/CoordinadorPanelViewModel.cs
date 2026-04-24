using System.ComponentModel.DataAnnotations;

namespace parcial.Models;

public class CoordinadorPanelViewModel
{
    public int? CursoIdFiltro { get; set; }
    public IReadOnlyList<Curso> Cursos { get; set; } = [];
    public IReadOnlyList<MatriculaResumenViewModel> Matriculas { get; set; } = [];
}

public class MatriculaResumenViewModel
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string CursoNombre { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public EstadoMatricula Estado { get; set; }
}

public class CursoFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Creditos")]
    public int Creditos { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Cupo maximo")]
    public int CupoMaximo { get; set; }

    [Display(Name = "Horario inicio")]
    [DataType(DataType.Time)]
    public TimeSpan HorarioInicio { get; set; }

    [Display(Name = "Horario fin")]
    [DataType(DataType.Time)]
    public TimeSpan HorarioFin { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HorarioInicio >= HorarioFin)
        {
            yield return new ValidationResult(
                "No permitir HorarioFin anterior o igual a HorarioInicio.",
                [nameof(HorarioInicio), nameof(HorarioFin)]);
        }
    }
}
