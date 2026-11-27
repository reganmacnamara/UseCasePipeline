using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseRuleEnforcer<TRequest> : IUseCaseRuleEnforcer<TRequest> where TRequest : IUseCaseRequest
    {
        public virtual int Order { get; set; } = 40;

        public abstract Task Enforce(TRequest request, CancellationToken cancellationToken);
    }
}
