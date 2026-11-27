using UseCasePipeline.Services;

namespace UseCasePipeline
{
    public class UseCaseMediator(IUseCaseRequestDispatcher dispatcher, IUseCasePipeline pipeline)
    {
        /// <summary>
        /// Sends a request through the pipeline and returns a response.
        /// <typeparamref name="TResponse"/> is inferred from the request's
        /// <see cref="IUseCaseRequest{TResponse}"/> declaration — no need to specify it explicitly.
        /// </summary>
        public Task<TResponse> Send<TResponse>(IUseCaseRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : IUseCaseResponse
            => dispatcher.DispatchAsync(request, pipeline, cancellationToken);

        /// <summary>
        /// Sends a request through the pipeline with no response.
        /// </summary>
        public Task Send(IUseCaseRequest request, CancellationToken cancellationToken = default)
            => dispatcher.DispatchAsync(request, pipeline, cancellationToken);
    }
}
