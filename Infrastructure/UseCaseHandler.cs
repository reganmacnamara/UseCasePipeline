using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseHandler<TRequest> : IUseCaseHandler<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    public abstract class UseCaseHandler<TRequest, TResponse> : IUseCaseHandler<TRequest, TResponse>
        where TRequest : IUseCaseRequest<TResponse>
        where TResponse : IUseCaseResponse
    {
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
