using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using parcial.Caching;
using parcial.Data;
using parcial.Models;

namespace parcial.Services;

public interface ICursoCatalogoCacheService
{
    Task<IReadOnlyList<Curso>> GetCursosActivosAsync(CancellationToken cancellationToken = default);
    Task InvalidateCursosActivosAsync(CancellationToken cancellationToken = default);
}

public class CursoCatalogoCacheService : ICursoCatalogoCacheService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CursoCatalogoCacheService> _logger;

    public CursoCatalogoCacheService(
        ApplicationDbContext context,
        IDistributedCache cache,
        ILogger<CursoCatalogoCacheService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Curso>> GetCursosActivosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedPayload = await _cache.GetStringAsync(CacheKeys.CursosActivos, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedPayload))
            {
                var cachedCursos = JsonSerializer.Deserialize<List<Curso>>(cachedPayload);
                if (cachedCursos is not null)
                {
                    return cachedCursos;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer cache Redis de cursos activos. Se usara la base de datos.");
        }

        var cursos = await _context.Cursos
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Codigo)
            .ToListAsync(cancellationToken);

        try
        {
            var payload = JsonSerializer.Serialize(cursos);
            await _cache.SetStringAsync(CacheKeys.CursosActivos, payload, CacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo escribir cache Redis de cursos activos.");
        }

        return cursos;
    }

    public async Task InvalidateCursosActivosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(CacheKeys.CursosActivos, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo invalidar cache Redis de cursos activos.");
        }
    }
}
