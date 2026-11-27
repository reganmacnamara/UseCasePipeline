using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal abstract class RequestDispatch
    {
        public abstract Task DispatchAsync(IUseCaseRequest request, IUseCasePipeline pipeline, CancellationToken cancellationToken);
    }

    internal sealed class RequestDispatch<TRequest> : RequestDispatch where TRequest : IUseCaseRequest
    {
        public override Task DispatchAsync(IUseCaseRequest request, IUseCasePipeline pipeline, CancellationToken cancellationToken)
            => pipeline.InvokeAsync((TRequest)request, cancellationToken);
    }
}
