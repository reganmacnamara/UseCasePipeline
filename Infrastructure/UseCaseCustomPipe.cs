using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseCustomPipe<TRequest> : IUseCaseCustomPipe<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task InvokeAsync(TRequest request, CancellationToken cancellationToken);
    }
}
