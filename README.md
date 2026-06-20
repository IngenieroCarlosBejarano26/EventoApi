# EventosVivos · API

Backend de reservas y eventos. Resuelve sobreventa de entradas, choques de horario entre venues y
validación manual de pagos. Expone REST + SignalR.

**Demo:** https://eventosvivos-api-t37ke2.azurewebsites.net  
**OpenAPI (Scalar):** https://eventosvivos-api-t37ke2.azurewebsites.net/scalar  
**Frontend:** https://github.com/IngenieroCarlosBejarano26/EventoFront · https://ambitious-moss-06713740f.7.azurestaticapps.net

> En el plan F1 de Azure la app se duerme tras ~20 min sin tráfico. La primera petición tarda unos
> segundos en responder.

---

## Por qué esta arquitectura

El problema central no es CRUD: es **concurrencia** (dos personas reservando la última entrada al
mismo tiempo) y **reglas de negocio** repartidas en varios flujos. Por eso separé el código en
capas hexagonales en lugar de un monolito anémico donde la lógica vive en los controllers.

```
EventosVivos.slnx
src/
├─ EventosVivos.Domain          # Reglas de negocio, entidades, value objects
├─ EventosVivos.Application     # Casos de uso (commands/queries + handlers)
├─ EventosVivos.Infrastructure  # EF Core, repositorios, cache, adaptadores
└─ EventosVivos.Api             # HTTP, middleware, SignalR, OpenAPI
tests/EventosVivos.Tests        # 50 tests (unitarios + integración HTTP)
```

**Hexagonal:** el dominio no sabe de PostgreSQL ni de HTTP. Si mañana cambio la base o meto una cola
de mensajes, no toco las reglas de reserva. Eso también hace que los tests de dominio sean rápidos y
sin infraestructura.

**CQRS con MediatR:** cada feature (crear evento, reservar, confirmar pago…) vive en su propia carpeta
con command/query, handler y validador. Evita un `EventService` de 800 líneas y encaja con la prueba
técnica, donde cada RF es un caso de uso acotado.

**Result pattern:** los errores de negocio (`sin entradas`, `venue ocupado`, `reserva tardía`) no
lanzan excepciones. Vuelven como `Result<T>` y el controller los traduce a HTTP. Las excepciones quedan
para lo inesperado.

Detalle de flujos, diagramas y reglas RN/RF: [`docs/ARQUITECTURA.md`](docs/ARQUITECTURA.md)

---

## Por qué estas herramientas

**.NET 10 + ASP.NET Core**  
Es el stack pedido en la prueba y el que mejor conozco para APIs con reglas complejas. El pipeline de
middleware, DI y hosting está maduro; no necesité reinventar nada para rate limiting, ProblemDetails o
hosted services.

**PostgreSQL + EF Core**  
Necesitaba concurrencia optimista real. PostgreSQL expone `xmin` como token de versión nativo; con
SQL Server habría usado `rowversion`, pero Postgres encaja bien con Supabase (free tier) y el pooler
IPv4 evita problemas de conectividad en local. EF Core me da migraciones y repositorios sin escribir SQL
a mano para el 95 % de los casos.

**MediatR + FluentValidation**  
MediatR desacopla el controller del handler. FluentValidation corre en un pipeline behavior antes de
entrar al dominio, así las validaciones de formato (longitud del título, email válido) no se mezclan
con las reglas de negocio (capacidad del venue, solapamiento de horarios).

**SignalR**  
El enunciado pide control de aforo en tiempo real. Polling cada X segundos funciona, pero es ruido
innecesario. Con SignalR el frontend recibe `EventUpdated` cuando alguien reserva o confirma, sin
recargar la lista.

**IMemoryCache**  
Los listados de eventos y venues cambian poco entre peticiones. Cache en memoria es suficiente para
esta escala; Redis sería overkill en una prueba de 3 días. Invalido el cache cuando hay writes.

**OpenAPI nativo + Scalar**  
Swashbuckle cumple, pero .NET 10 trae generación OpenAPI integrada. Scalar da una UI más limpia para
probar endpoints. Documenté los controllers con XML para que el evaluador vea RF/RN directo en la UI.

**xUnit + FluentAssertions + WebApplicationFactory**  
Tests unitarios en dominio y handlers (rápidos, sin BD). Cinco tests de integración levantan la API
real con EF InMemory para validar el flujo HTTP completo. No monté Testcontainers porque el tiempo
era limitado y el foco era la lógica de negocio.

**API Key + idempotencia + rate limit**  
No es OAuth porque la prueba no lo pide. La API Key cubre operaciones admin (crear evento, confirmar
pago). La idempotencia en reservas evita duplicados si el cliente reintenta por timeout. Rate limit en
POST reservas protege de abuso básico.

---

## Ejecutar localmente

Necesitas .NET 10 SDK y PostgreSQL (local o Supabase).

```bash
# ConnectionStrings:Default en appsettings.json apunta a localhost por defecto.
# Para Supabase u otra instancia, usa user-secrets (no commitees credenciales):
dotnet user-secrets --project src/EventosVivos.Api set "ConnectionStrings:Default" "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"

dotnet ef database update --project src/EventosVivos.Infrastructure --startup-project src/EventosVivos.Api
dotnet run --project src/EventosVivos.Api
```

Al arrancar aplica migraciones y seed de venues. Scalar queda en `http://localhost:5090/scalar`.

**Tests:**

```bash
dotnet test
```

50 tests: dominio, handlers, validadores y 5 de integración HTTP (anti-sobreventa, RN03–RN07, RF-04).

---

## Endpoints

| Método | Ruta | Qué hace | Headers |
|--------|------|----------|---------|
| POST | `/api/events` | Crear evento | `X-API-KEY` |
| GET | `/api/events` | Listar con filtros | — |
| GET | `/api/events/{id}/occupancy-report` | Reporte de ocupación | — |
| POST | `/api/reservations` | Reservar entradas | `X-Idempotency-Key` (recomendado) |
| POST | `/api/reservations/{id}/confirm` | Confirmar pago | `X-API-KEY` |
| POST | `/api/reservations/{id}/cancel` | Cancelar reserva | — |
| GET | `/api/venues` | Catálogo de venues | — |

---

## Despliegue

**CI/CD:** `.github/workflows/deploy.yml` — en cada push a `main` corre tests, publica y despliega a
Azure App Service (F1).

Secreto en GitHub Actions: `AZURE_WEBAPP_PUBLISH_PROFILE`

```bash
az webapp deployment list-publishing-profiles -g rg-eventosvivos -n eventosvivos-api-t37ke2 --xml
```

**Manual** (ojo con el zip en Windows — usa `tar`, no `Compress-Archive`, o Kudu en Linux falla):

```bash
dotnet publish src/EventosVivos.Api/EventosVivos.Api.csproj -c Release -o publish
tar -a -c -f publish.zip -C publish .
az webapp deploy -g rg-eventosvivos -n eventosvivos-api-t37ke2 --src-path publish.zip --type zip
```

**App Settings en Azure:**

- `ConnectionStrings__Default` — Npgsql hacia Supabase
- `Cors__AllowedOrigins__0` — URL del Static Web App (CORS + SignalR)
- `ASPNETCORE_ENVIRONMENT=Development` — si quieres Scalar en producción

No subas credenciales al repo. Usa User Secrets en local y App Settings en Azure.
