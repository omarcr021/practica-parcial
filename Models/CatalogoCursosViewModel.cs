using System.ComponentModel.DataAnnotations;

namespace parcial.Models;

public class CatalogoCursosViewModel
{
    public CursoCatalogoFiltros Filtros { get; set; } = new();
    public IReadOnlyList<Curso> Cursos { get; set; } = [];
}

public class CursoCatalogoFiltros : IValidatableObject
{
    public string? Nombre { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "No se aceptan creditos negativos.")]
    public int? CreditosMin { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "No se aceptan creditos negativos.")]
    public int? CreditosMax { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan? HorarioInicio { get; set; }

    [DataType(DataType.Time)]
    public TimeSpan? HorarioFin { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreditosMin.HasValue && CreditosMax.HasValue && CreditosMin > CreditosMax)
        {
            yield return new ValidationResult(
                "El rango de creditos es invalido: minimo no puede ser mayor que maximo.",
                [nameof(CreditosMin), nameof(CreditosMax)]);
        }

        if (HorarioInicio.HasValue && HorarioFin.HasValue && HorarioFin < HorarioInicio)
        {
            yield return new ValidationResult(
                "No se permite HorarioFin anterior a HorarioInicio.",
                [nameof(HorarioInicio), nameof(HorarioFin)]);
        }
    }
}
