namespace UseCasePipeline.Infrastructure
{
    /// <summary>
    /// The Authorisation Handler of a UseCase
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseAuthoriser<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        Task Authorise(TRequest request, CancellationToken cancellationToken);
    }
}
