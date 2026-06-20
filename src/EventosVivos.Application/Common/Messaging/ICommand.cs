using EventosVivos.Domain.Common;
using MediatR;

namespace EventosVivos.Application.Common.Messaging;

/// <summary>Comando sin valor de retorno (solo Result).</summary>
public interface ICommand : IRequest<Result>;

/// <summary>Comando que retorna un valor en caso de éxito.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
