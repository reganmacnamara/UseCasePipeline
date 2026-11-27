using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    public abstract class UseCaseRequestValidator<TRequest> : IUseCaseRequestValidator<TRequest> where TRequest : IUseCaseRequest
    {
        public abstract Task Validate(TRequest request, CancellationToken cancellationToken);
    }
}
