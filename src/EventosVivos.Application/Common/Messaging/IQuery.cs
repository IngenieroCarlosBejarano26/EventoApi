using EventosVivos.Domain.Common;
using MediatR;

namespace EventosVivos.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
