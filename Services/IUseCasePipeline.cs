namespace UseCasePipeline.Services
{
    /// <summary>
    /// Runs the pipes registered for a request.
    /// </summary>
    public interface IUseCasePipeline
    {
        /// <summary>
        /// Invokes every pipe registered for the request in ascending <see cref="IUseCasePipe{TRequest}.Order"/>.
        /// </summary>
        /// <param name="request">The request being processed by the pipeline.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        /// <exception cref="UseCasePipeline.Exceptions.UseCaseConfigurationException">
        /// No <see cref="IUseCaseHandler{TRequest}"/> is registered for the request. No pipe is invoked.
        /// </exception>
        Task InvokeAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest;

        /// <summary>
        /// Invokes the pipes registered for the request in ascending <see cref="IUseCasePipe{TRequest}.Order"/>
        /// until it reaches the <see cref="IUseCasePipe{TRequest, TResponse}"/> that handles it, and returns
        /// that pipe's response.
        /// </summary>
        /// <param name="request">The request being processed by the pipeline.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        /// <exception cref="UseCasePipeline.Exceptions.UseCaseConfigurationException">No handler is registered for the request.</exception>
        Task<TResponse> InvokeWithResponseAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IUseCaseRequest<TResponse>
            where TResponse : IUseCaseResponse;
    }
}
