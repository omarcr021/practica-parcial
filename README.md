# Portal Interno Universitario - Examen MVC

Aplicacion web en ASP.NET Core MVC (.NET 10) con Identity, EF Core y SQLite para la gestion de cursos y matriculas.

## Estado del avance

- [x] Pregunta 1 - Bootstrap + Modelo de datos
- [x] Pregunta 2 - Catalogo de cursos y filtros
- [x] Pregunta 3 - Inscripcion y validaciones de matricula
- [x] Pregunta 4 - Sesiones y Redis
- [x] Pregunta 5 - Panel de Coordinador
- [x] Pregunta 6 - Despliegue en Render

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
- `Redis__ConnectionString` = `` (vacio para usar fallback `DistributedMemoryCache` en local)

### Render (Production)

Variables minimas requeridas:

- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ASPNETCORE_URLS` = `http://0.0.0.0:${PORT}`
- `ConnectionStrings__DefaultConnection` = `<cadena_sqlite_o_bd_produccion>`
- `Redis__ConnectionString` = `<cadena_redis_render_o_redis_cloud>`

## Funcionalidad implementada hasta P4

- Sesion:
- Se guarda el ultimo curso visitado en session al abrir el detalle.
- En el layout se muestra el enlace `Volver al curso {Nombre}` cuando existe sesion activa.
- Cache distribuida de cursos activos:
- El listado de cursos activos se cachea por 60 segundos.
- Si Redis no esta configurado en local, el sistema usa `DistributedMemoryCache` como fallback.
- Invalida cache automaticamente cuando se agrega, edita o elimina un curso (hook en `SaveChanges` del `DbContext`).

## URL de despliegue en Render

Pendiente de despliegue. (Se actualizará una vez publicado por el alumno).

- URL Render: `[A COMPLETAR POR EL ALUMNO]`

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
- 2026-04-23: Se marca avance de Pregunta 3 con inscripcion en estado Pendiente y validaciones server-side (autenticacion, cupo y solape de horario).
- 2026-04-23: Se marca avance de Pregunta 4 con session del ultimo curso y cache de cursos activos por 60s con Redis/fallback local.
- 2026-04-24: Se marca avance de Pregunta 5 con Panel de Coordinador, CRUD de cursos, y gestion de matriculas con roles.
- 2026-04-24: Se marca avance de Pregunta 6 con preparación para despliegue en Render (Dockerfile y documentación de variables/discos).

---

Este README se actualizara al cierre de cada pregunta para mantener trazabilidad de implementacion y despliegue.
