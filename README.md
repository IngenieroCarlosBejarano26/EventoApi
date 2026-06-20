# EventosVivos · API (.NET 10)

Backend del sistema de gestión de eventos y reservas: controla el aforo en tiempo real, evita la
sobreventa, gestiona conflictos de horarios por venue y valida reservas/pagos. Implementado con
**Arquitectura Hexagonal + CQRS**, **PostgreSQL** y actualizaciones en vivo con **SignalR**.

> Frontend (Angular 21 + PrimeNG): https://github.com/IngenieroCarlosBejarano26/EventoFront

## Aplicación desplegada (Azure, capa gratuita)

| Recurso | URL |
|---------|-----|
| **API** (Azure App Service) | https://eventosvivos-api-t37ke2.azurewebsites.net |
| **Documentación interactiva** (Scalar / OpenAPI) | https://eventosvivos-api-t37ke2.azurewebsites.net/scalar |
| **Frontend** (Azure Static Web Apps) | https://ambitious-moss-06713740f.7.azurestaticapps.net |
| Base de datos | PostgreSQL gestionado (Supabase) |

> El plan **F1 (Free)** suspende la app tras ~20 min de inactividad: la primera petición puede tardar
> unos segundos (cold start).

---

## Arquitectura

```
EventosVivos.slnx
src/
├─ EventosVivos.Domain          # Entidades, Value Objects, Domain Events, reglas de negocio
├─ EventosVivos.Application     # Casos de uso (CQRS con MediatR), puertos, validadores, behaviors
├─ EventosVivos.Infrastructure  # EF Core (PostgreSQL), repositorios, cache, servicios
└─ EventosVivos.Api             # Controllers, middleware, seguridad, SignalR, OpenAPI
tests/EventosVivos.Tests        # Pruebas unitarias (dominio, validadores, handlers)
```

- **CQRS con MediatR** por *vertical slices* (Command/Query + Handler + Validator + DTO).
- **Result Pattern**: los errores de negocio se modelan como `Result`/`Error`, sin excepciones.
- **Pipeline Behaviors**: validación (FluentValidation) y logging estructurado, transversales.
- **PostgreSQL + `xmin`**: concurrencia optimista con la columna de sistema `xmin`; previene la
  **sobreventa** ante reservas concurrentes (con reintento en el handler).
- **Domain Events** vía `IPublisher` (auditoría, logging, notificaciones simuladas y *push* en vivo);
  sustituibles por RabbitMQ / Azure Service Bus sin tocar el dominio.
- **Tiempo real con SignalR**: hub en `/hubs/events` (`EventUpdated`, `EventCreated`).
- Seguridad y robustez: **API Key** (`X-API-KEY`), **idempotencia** (`X-Idempotency-Key`),
  **rate limiting** nativo, `ProblemDetails` global y un `Hosted Service` que completa eventos
  automáticamente.

---

## Ejecutar localmente

**Requisitos:** .NET 10 SDK y PostgreSQL (local o gestionado, p. ej. Supabase).

```bash
# La cadena de conexión (formato Npgsql) se lee de ConnectionStrings:Default.
# En appsettings.json viene un valor local por defecto:
#   Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres
# Para una instancia gestionada (Supabase) sobreescríbela SIN commitearla, por ejemplo:
dotnet user-secrets --project src/EventosVivos.Api set "ConnectionStrings:Default" "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
#   o vía variable de entorno:  $env:ConnectionStrings__Default="..."

dotnet ef database update --project src/EventosVivos.Infrastructure --startup-project src/EventosVivos.Api
dotnet run --project src/EventosVivos.Api
# Scalar UI en http://localhost:5090/scalar (o el puerto asignado)
```

Las migraciones de EF Core y el seed de venues también se aplican automáticamente al arrancar.

### Pruebas

```bash
dotnet test
```

45 pruebas: entidades de dominio, value objects, validadores y handlers, con foco en **sobreventa**,
**horario nocturno (RN03)**, **reserva tardía (RN04)**, **límite por precio (RN05)**,
**confirmación de pago (RF-04)** y **cancelación con penalización (RN07)**.

---

## API

| Método | Endpoint | Descripción | Seguridad |
|--------|----------|-------------|-----------|
| POST | `/api/events` | Crear evento (RF-01) | `X-API-KEY` |
| GET | `/api/events` | Listar con filtros (RF-02) | — |
| GET | `/api/events/{id}/occupancy-report` | Reporte de ocupación (RF-06) | — |
| POST | `/api/reservations` | Crear reserva (RF-03) | `X-Idempotency-Key` |
| POST | `/api/reservations/{id}/confirm` | Confirmar pago (RF-04) | `X-API-KEY` |
| POST | `/api/reservations/{id}/cancel` | Cancelar (RF-05) | — |
| GET | `/api/venues` | Listar venues | — |

---

## Despliegue

### CI/CD (GitHub Actions)

El workflow `.github/workflows/deploy.yml` restaura, **ejecuta los tests**, publica y despliega la API
al App Service en cada push a `main`/`develop`.

**Secreto requerido** (GitHub → Settings → Secrets and variables → Actions):

| Secreto | Cómo obtenerlo |
|---------|----------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | `az webapp deployment list-publishing-profiles -g rg-eventosvivos -n eventosvivos-api-t37ke2 --xml` |

### Despliegue manual

```bash
dotnet publish src/EventosVivos.Api/EventosVivos.Api.csproj -c Release -o publish
tar -a -c -f publish.zip -C publish .   # tar usa rutas "/" (Compress-Archive falla en Kudu/Linux)
az webapp deploy -g rg-eventosvivos -n eventosvivos-api-t37ke2 --src-path publish.zip --type zip
```

### App Settings relevantes en Azure

- `ASPNETCORE_ENVIRONMENT=Development` (expone Scalar/OpenAPI).
- `ConnectionStrings__Default` (cadena Npgsql de Supabase — **no** va en el repo).
- `Cors__AllowedOrigins__0` = URL del frontend (requerido por la API y SignalR).

---

## Seguridad de secretos

No publiques credenciales reales en `appsettings.json`. La cadena de Supabase vive solo en los
*App Settings* de Azure (y, en local, en *User Secrets* o variables de entorno). Rota cualquier
contraseña que se haya expuesto.
