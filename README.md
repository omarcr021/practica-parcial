# Portal Interno Universitario - Examen MVC

Aplicacion web en ASP.NET Core MVC (.NET 10) con Identity, EF Core y SQLite para la gestion de cursos y matriculas.

## Estado del avance

- [x] Pregunta 1 - Bootstrap + Modelo de datos
- [x] Pregunta 2 - Catalogo de cursos y filtros
- [ ] Pregunta 3 - Inscripcion y validaciones de matricula
- [ ] Pregunta 4 - Sesiones y Redis
- [ ] Pregunta 5 - Panel de Coordinador
- [ ] Pregunta 6 - Despliegue en Render

## Requisitos

- .NET SDK 10
- Git
- (Para preguntas 4 y 6) Redis

## Ejecucion local

1. Restaurar paquetes:

```bash
dotnet restore
```

2. Compilar:

```bash
dotnet build
```

3. Aplicar migraciones:

```bash
dotnet ef database update
```

4. Ejecutar proyecto:

```bash
dotnet run
```

La aplicacion quedara disponible en la URL mostrada por consola (por defecto http://localhost:5277).

## Migraciones (EF Core)

Crear una nueva migracion:

```bash
dotnet ef migrations add NombreMigracion
```

Aplicar migraciones pendientes:

```bash
dotnet ef database update
```

Eliminar la ultima migracion (si aun no se aplico):

```bash
dotnet ef migrations remove
```

Migracion inicial usada en este proyecto:

- `InitialIdentityAndDominio`

## Datos semilla actuales

Se cargan automaticamente al iniciar la aplicacion:

- Rol: Coordinador
- Usuario coordinador: coordinador@universidad.edu
- Password coordinador: Coord!2026
- 3 cursos activos iniciales

## Variables de entorno

### Local (Development)

Configuradas via archivos `appsettings.json` y `appsettings.Development.json`:

- `ConnectionStrings__DefaultConnection` = `Data Source=parcial.db`

### Render (Production)

Variables minimas requeridas:

- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ASPNETCORE_URLS` = `http://0.0.0.0:${PORT}`
- `ConnectionStrings__DefaultConnection` = `<cadena_sqlite_o_bd_produccion>`
- `Redis__ConnectionString` = `<cadena_redis_render_o_redis_cloud>`

## URL de despliegue en Render

Pendiente de despliegue.

- URL Render: `PENDIENTE`

## Flujo Git del examen

Se trabaja en una rama por pregunta y cada rama se integra con PR hacia `main`.

- `feature/bootstrap-dominio` -> PR a `main`
- `feature/catalogo-cursos` -> PR a `main`
- `feature/matriculas` -> PR a `main`
- `feature/sesion-redis` -> PR a `main`
- `feature/panel-coordinador` -> PR a `main`
- `deploy/render` -> PR a `main`

Comandos base por rama:

```bash
git checkout main
git pull
git checkout -b nombre-rama
# cambios...
git add .
git commit -m "mensaje"
git push -u origin nombre-rama
```

## Registro de actualizaciones del README

- 2026-04-23: Se crea README base con setup local, migraciones, variables de entorno y seccion de Render.
- 2026-04-23: Se marca avance de Pregunta 1 y Pregunta 2.

---

Este README se actualizara al cierre de cada pregunta para mantener trazabilidad de implementacion y despliegue.
