namespace UseCasePipeline.Services
{
    /// <summary>
    /// Sends a request that is known only by its interface into the pipeline as its concrete type.
    /// </summary>
    public interface IUseCaseRequestDispatcher
    {
        /// <summary>
        /// Invokes the pipeline for the request's concrete type.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="pipeline">The pipeline that runs the request's pipes.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        Task DispatchAsync(IUseCaseRequest request, IUseCasePipeline pipeline, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes the pipeline for the request's concrete type and returns the handler's response.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="pipeline">The pipeline that runs the request's pipes.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        Task<TResponse> DispatchAsync<TResponse>(IUseCaseRequest<TResponse> request, IUseCasePipeline pipeline, CancellationToken cancellationToken)
            where TResponse : IUseCaseResponse;
    }
}
