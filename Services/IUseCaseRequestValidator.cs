namespace UseCasePipeline.Services
{
    /// <summary>
    /// The Request Validator for a UseCase.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseRequestValidator<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest
    {
        Task Validate(TRequest request, CancellationToken cancellationToken);

        Task IUseCasePipe<TRequest>.InvokeAsync(TRequest request, CancellationToken cancellationToken)
            => Validate(request, cancellationToken);
    }
}
