using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;

namespace parcial.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinadorController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Coordinador")]
    public async Task<IActionResult> Index(int? cursoId)
    {
        var cursos = await _context.Cursos
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Codigo)
            .ToListAsync();

        var matriculasQuery = _context.Matriculas
            .AsNoTracking()
            .Include(m => m.Curso)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (cursoId.HasValue)
        {
            matriculasQuery = matriculasQuery.Where(m => m.CursoId == cursoId.Value);
        }

        var matriculas = await matriculasQuery
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MatriculaResumenViewModel
            {
                Id = m.Id,
                CursoId = m.CursoId,
                CursoNombre = m.Curso != null ? m.Curso.Nombre : "Curso",
                UsuarioId = m.UsuarioId,
                UsuarioEmail = m.Usuario != null
                    ? (m.Usuario.Email ?? m.Usuario.UserName ?? m.UsuarioId)
                    : m.UsuarioId,
                FechaRegistro = m.FechaRegistro,
                Estado = m.Estado
            })
            .ToListAsync();

        var model = new CoordinadorPanelViewModel
        {
            CursoIdFiltro = cursoId,
            Cursos = cursos,
            Matriculas = matriculas
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateCurso()
    {
        return View(new CursoFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCurso(CursoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existeCodigo = await _context.Cursos.AnyAsync(c => c.Codigo == model.Codigo);
        if (existeCodigo)
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe un curso con ese codigo.");
            return View(model);
        }

        var curso = new Curso
        {
            Codigo = model.Codigo.Trim(),
            Nombre = model.Nombre.Trim(),
            Creditos = model.Creditos,
            CupoMaximo = model.CupoMaximo,
            HorarioInicio = model.HorarioInicio,
            HorarioFin = model.HorarioFin,
            Activo = model.Activo
        };

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Curso creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditCurso(int id)
    {
        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
        {
            return NotFound();
        }

        var model = new CursoFormViewModel
        {
            Id = curso.Id,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Creditos = curso.Creditos,
            CupoMaximo = curso.CupoMaximo,
            HorarioInicio = curso.HorarioInicio,
            HorarioFin = curso.HorarioFin,
            Activo = curso.Activo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCurso(int id, CursoFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
        {
            return NotFound();
        }

        var existeCodigo = await _context.Cursos.AnyAsync(c => c.Id != id && c.Codigo == model.Codigo);
        if (existeCodigo)
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe un curso con ese codigo.");
            return View(model);
        }

        curso.Codigo = model.Codigo.Trim();
        curso.Nombre = model.Nombre.Trim();
        curso.Creditos = model.Creditos;
        curso.CupoMaximo = model.CupoMaximo;
        curso.HorarioInicio = model.HorarioInicio;
        curso.HorarioFin = model.HorarioFin;
        curso.Activo = model.Activo;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Curso actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarCurso(int id)
    {
        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
        {
            TempData["ErrorMessage"] = "No se encontro el curso.";
            return RedirectToAction(nameof(Index));
        }

        curso.Activo = false;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Curso desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarMatricula(int id, int? cursoId)
    {
        var matricula = await _context.Matriculas.FirstOrDefaultAsync(m => m.Id == id);
        if (matricula is null)
        {
            TempData["ErrorMessage"] = "No se encontro la matricula.";
            return RedirectToAction(nameof(Index), new { cursoId });
        }

        matricula.Estado = EstadoMatricula.Confirmada;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Matricula confirmada.";
        return RedirectToAction(nameof(Index), new { cursoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarMatricula(int id, int? cursoId)
    {
        var matricula = await _context.Matriculas.FirstOrDefaultAsync(m => m.Id == id);
        if (matricula is null)
        {
            TempData["ErrorMessage"] = "No se encontro la matricula.";
            return RedirectToAction(nameof(Index), new { cursoId });
        }

        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Matricula cancelada.";
        return RedirectToAction(nameof(Index), new { cursoId });
    }
}
