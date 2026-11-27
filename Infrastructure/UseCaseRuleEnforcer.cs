using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseRuleEnforcer<TRequest> : IUseCaseRuleEnforcer<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task Enforce(TRequest request, CancellationToken cancellationToken);
    }
}
