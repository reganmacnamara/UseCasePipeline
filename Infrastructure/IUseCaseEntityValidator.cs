namespace UseCasePipeline.Infrastructure
{
    /// <summary>
    /// The Entity Validator of a UseCase. Used to validate necessary entities exist before they are needed.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseEntityValidator<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        Task Validate(TRequest request, CancellationToken cancellationToken);
    }
}
