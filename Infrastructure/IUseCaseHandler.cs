namespace UseCasePipeline.Infrastructure
{
    /// <summary>
    /// The Handler of a UseCase. Core UseCase logic is to be implemented here.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseHandler<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// The Handler of a UseCase. The Handler returns <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IUseCaseHandler<TRequest, TResponse> : IUseCasePipe
        where TRequest : IUseCaseRequest<TResponse>
        where TResponse : IUseCaseResponse
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
