namespace UseCasePipeline.Infrastructure
{
    /// <summary>
    /// The Request Validator for a UseCase.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseRequestValidator<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        Task Validate(TRequest request, CancellationToken cancellationToken);
    }
}
