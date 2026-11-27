using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseAuthoriser<TRequest> : IUseCaseAuthoriser<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task Authorise(TRequest request, CancellationToken cancellationToken);
    }
}
