using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;

namespace parcial.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
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
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Inscribirse(int id)
    {
        TempData["InfoMessage"] = "La inscripcion se completara en la Pregunta 3.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
