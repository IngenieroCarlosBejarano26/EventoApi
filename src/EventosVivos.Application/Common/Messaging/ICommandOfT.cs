using EventosVivos.Domain.Common;
using MediatR;

namespace EventosVivos.Application.Common.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
