using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseEntityValidator<TRequest> : IUseCaseEntityValidator<TRequest> where TRequest : IUseCaseRequest
    {
        public virtual int Order { get; set; } = 30;

        public abstract Task Validate(TRequest request, CancellationToken cancellationToken);
    }
}
