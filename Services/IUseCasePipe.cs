namespace UseCasePipeline.Services
{
    /// <summary>
    /// Marker interface for built-in and custom UseCase pipeline stages.
    /// </summary>
    public interface IUseCasePipe
    {

    }
    /// <summary>
    /// A stage in the UseCase pipeline for <typeparamref name="TRequest"/>. Every pipe registered
    /// for the request is invoked in ascending <see cref="Order"/>.
    /// </summary>
    public interface IUseCasePipe<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        /// <summary>
        /// The position of this pipe in the pipeline. Pipes run in ascending order, and pipes
        /// with the same order run in registration order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Executes this pipe's logic for the request.
        /// </summary>
        /// <param name="request">The request being processed by the pipeline.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        Task InvokeAsync(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// A UseCase pipeline stage that returns a <typeparamref name="TResponse"/>. When a request is
    /// sent for its response, the pipeline returns this pipe's response as soon as it reaches it,
    /// and pipes ordered after it are not invoked.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IUseCasePipe<TRequest, TResponse> : IUseCasePipe<TRequest>
        where TRequest : IUseCaseRequest<TResponse>
        where TResponse : IUseCaseResponse
    {
        /// <summary>
        /// Executes this pipe's logic for the request and returns its response.
        /// </summary>
        /// <param name="request">The request being processed by the pipeline.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        new Task<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken);

        Task IUseCasePipe<TRequest>.InvokeAsync(TRequest request, CancellationToken cancellationToken)
            => InvokeAsync(request, cancellationToken);
    }
}
