using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline.Infrastructure;

namespace UseCasePipeline
{
    public class UseCaseMediator(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Sends a request through the pipeline and returns a response.
        /// <typeparamref name="TResponse"/> is inferred from the request's
        /// <see cref="IUseCaseRequest{TResponse}"/> declaration — no need to specify it explicitly.
        /// </summary>
        public Task<TResponse> Send<TResponse>(IUseCaseRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : IUseCaseResponse
        {
            // request.GetType() gives us the concrete type (e.g. CreateOrderRequest) rather than
            // the interface, which is what DI uses as the key when resolving validators etc.
            var method = GetType()
                .GetMethod(nameof(ExecuteWithResponse), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(request.GetType(), typeof(TResponse));

            return (Task<TResponse>)method.Invoke(this, [request, cancellationToken])!;
        }

        /// <summary>
        /// Sends a request through the pipeline with no response.
        /// </summary>
        public Task Send(IUseCaseRequest request, CancellationToken cancellationToken = default)
        {
            var method = GetType()
                .GetMethod(nameof(Execute), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(request.GetType());

            return (Task)method.Invoke(this, [request, cancellationToken])!;
        }

        private async Task<TResponse> ExecuteWithResponse<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IUseCaseRequest<TResponse>
            where TResponse : IUseCaseResponse
        {
            await RunPipes(request, cancellationToken);

            var handler = serviceProvider.GetRequiredService<IUseCaseHandler<TRequest, TResponse>>();
            return await handler.Handle(request, cancellationToken);
        }

        private async Task Execute<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
        {
            await RunPipes(request, cancellationToken);

            var handler = serviceProvider.GetRequiredService<IUseCaseHandler<TRequest>>();
            await handler.Handle(request, cancellationToken);
        }

        private async Task RunPipes<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
        {
            // Resolve each stage as a sequence so multiple implementations run in DI registration order.
            foreach (var authoriser in serviceProvider.GetServices<IUseCaseAuthoriser<TRequest>>())
                await authoriser.Authorise(request, cancellationToken);

            foreach (var validator in serviceProvider.GetServices<IUseCaseRequestValidator<TRequest>>())
                await validator.Validate(request, cancellationToken);

            foreach (var validator in serviceProvider.GetServices<IUseCaseEntityValidator<TRequest>>())
                await validator.Validate(request, cancellationToken);

            foreach (var validator in serviceProvider.GetServices<IUseCaseRuleEnforcer<TRequest>>())
                await validator.Enforce(request, cancellationToken);

            foreach (var customPipe in serviceProvider.GetServices<IUseCaseCustomPipe<TRequest>>())
                await customPipe.InvokeAsync(request, cancellationToken);
        }
    }
}
