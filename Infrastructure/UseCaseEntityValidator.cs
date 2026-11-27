using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseEntityValidator<TRequest> : IUseCaseEntityValidator<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task Validate(TRequest request, CancellationToken cancellationToken);
    }
}
