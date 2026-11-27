namespace UseCasePipeline.Services
{
    /// <summary>
    /// Marker interface for built-in and custom UseCase pipeline stages.
    /// </summary>
    public interface IUseCasePipe
    {

    }
    /// <summary>
    /// Marker interface for built-in and custom UseCase pipeline stages.
    /// </summary>
    public interface IUseCasePipe<TRequest> : IUseCasePipe where TRequest : IUseCaseRequest
    {
        /// <summary>
        /// The order the Pipes of the Pipeline should be Invoked
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Executes this pipe's logic for the request.
        /// </summary>
        /// <param name="request">The request being processed by the pipeline.</param>
        /// <param name="cancellationToken">A token used to cancel the pipeline operation.</param>
        Task InvokeAsync(TRequest request, CancellationToken cancellationToken);
    }
}
