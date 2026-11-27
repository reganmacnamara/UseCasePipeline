namespace UseCasePipeline.Services
{
    /// <summary>
    /// The Authorisation Handler of a UseCase
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseAuthoriser<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest
    {
        Task Authorise(TRequest request, CancellationToken cancellationToken);

        Task IUseCasePipe<TRequest>.InvokeAsync(TRequest request, CancellationToken cancellationToken)
            => Authorise(request, cancellationToken);
    }
}
