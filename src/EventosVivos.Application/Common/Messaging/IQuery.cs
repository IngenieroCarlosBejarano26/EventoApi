using EventosVivos.Domain.Common;
using MediatR;

namespace EventosVivos.Application.Common.Messaging;

/// <summary>Consulta de solo lectura que retorna un valor.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
