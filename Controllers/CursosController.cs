using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;

namespace parcial.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CursosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CursoCatalogoFiltros filtros)
    {
        var query = _context.Cursos
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Codigo)
            .AsQueryable();

        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                var nombre = filtros.Nombre.Trim();
                query = query.Where(c => c.Nombre.Contains(nombre));
            }

            if (filtros.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            }

            if (filtros.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            }

            if (filtros.HorarioInicio.HasValue)
            {
                query = query.Where(c => c.HorarioInicio >= filtros.HorarioInicio.Value);
            }

            if (filtros.HorarioFin.HasValue)
            {
                query = query.Where(c => c.HorarioFin <= filtros.HorarioFin.Value);
            }
        }

        var model = new CatalogoCursosViewModel
        {
            Filtros = filtros,
            Cursos = await query.ToListAsync()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var curso = await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

        if (curso is null)
        {
            return NotFound();
        }

        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscribirse(int id)
    {
        var curso = await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

        if (curso is null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["ErrorMessage"] = "Debes iniciar sesion para inscribirte.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["ErrorMessage"] = "No se pudo identificar al usuario autenticado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var yaMatriculado = await _context.Matriculas
            .AsNoTracking()
            .AnyAsync(m => m.CursoId == id && m.UsuarioId == userId);

        if (yaMatriculado)
        {
            TempData["ErrorMessage"] = "Ya tienes una matricula registrada para este curso.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var ocupados = await _context.Matriculas
            .AsNoTracking()
            .CountAsync(m => m.CursoId == id && m.Estado != EstadoMatricula.Cancelada);

        if (ocupados >= curso.CupoMaximo)
        {
            TempData["ErrorMessage"] = "No hay cupos disponibles para este curso.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var solapeHorario = await _context.Matriculas
            .AsNoTracking()
            .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
            .Join(
                _context.Cursos.AsNoTracking(),
                matricula => matricula.CursoId,
                cursoMatriculado => cursoMatriculado.Id,
                (_, cursoMatriculado) => new { cursoMatriculado.HorarioInicio, cursoMatriculado.HorarioFin })
            .AnyAsync(h => curso.HorarioInicio < h.HorarioFin && h.HorarioInicio < curso.HorarioFin);

        if (solapeHorario)
        {
            TempData["ErrorMessage"] = "El horario del curso se solapa con otra matricula activa.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var matricula = new Matricula
        {
            CursoId = curso.Id,
            UsuarioId = userId,
            FechaRegistro = DateTime.UtcNow,
            Estado = EstadoMatricula.Pendiente
        };

        _context.Matriculas.Add(matricula);

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Inscripcion registrada en estado Pendiente.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "No fue posible registrar la inscripcion. Intenta nuevamente.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
