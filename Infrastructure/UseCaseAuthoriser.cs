using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseAuthoriser<TRequest> : IUseCaseAuthoriser<TRequest> where TRequest : IUseCaseRequest
    {
        public virtual int Order { get; set; } = 10;

        public abstract Task Authorise(TRequest request, CancellationToken cancellationToken);
    }
}
