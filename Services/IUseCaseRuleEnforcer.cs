namespace UseCasePipeline.Services
{
    /// <summary>
    /// The Rule Enforcer for a UseCase. Used to enforce business rules after validation and before the handler runs.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IUseCaseRuleEnforcer<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest
    {
        /// <summary>
        /// Enforces business rules for the request.
        /// </summary>
        Task Enforce(TRequest request, CancellationToken cancellationToken);

        Task IUseCasePipe<TRequest>.InvokeAsync(TRequest request, CancellationToken cancellationToken)
            => Enforce(request, cancellationToken);
    }
}
