using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal abstract class RequestWithResponseDispatch<TResponse> where TResponse : IUseCaseResponse
    {
        public abstract Task<TResponse> DispatchAsync(IUseCaseRequest<TResponse> request, IUseCasePipeline pipeline, CancellationToken cancellationToken);
    }

    internal sealed class RequestWithResponseDispatch<TRequest, TResponse> : RequestWithResponseDispatch<TResponse>
        where TRequest : IUseCaseRequest<TResponse>
        where TResponse : IUseCaseResponse
    {
        public override Task<TResponse> DispatchAsync(IUseCaseRequest<TResponse> request, IUseCasePipeline pipeline, CancellationToken cancellationToken)
            => pipeline.InvokeWithResponseAsync<TRequest, TResponse>((TRequest)request, cancellationToken);
    }
}
