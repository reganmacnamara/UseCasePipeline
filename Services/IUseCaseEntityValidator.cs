namespace UseCasePipeline.Services
{
    /// <summary>
    /// The Entity Validator of a UseCase. Used to validate necessary entities exist before they are needed.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseEntityValidator<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest
    {
        Task Validate(TRequest request, CancellationToken cancellationToken);

        Task IUseCasePipe<TRequest>.InvokeAsync(TRequest request, CancellationToken cancellationToken)
            => Validate(request, cancellationToken);
    }
}
