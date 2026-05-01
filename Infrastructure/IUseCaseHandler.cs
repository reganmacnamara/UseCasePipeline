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
}
