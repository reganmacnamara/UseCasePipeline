using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline.Exceptions;
using UseCasePipeline.Services;

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
            => await InvokePipelineWithResponse<TRequest, TResponse>(request, cancellationToken);

        private async Task Execute<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
            => await InvokePipeline(request, cancellationToken);

        private async Task InvokePipeline<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
        {
            foreach (var _Pipe in serviceProvider.GetServices<IUseCasePipe<TRequest>>().OrderBy(p => p.Order))
            {
                await _Pipe.InvokeAsync(request, cancellationToken);
            }
        }

        private async Task<TResponse> InvokePipelineWithResponse<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IUseCaseRequest<TResponse>
            where TResponse : IUseCaseResponse
        {
            foreach (var _Pipe in serviceProvider.GetServices<IUseCasePipe<TRequest>>().OrderBy(p => p.Order))
            {
                if (_Pipe is IUseCasePipe<TRequest, TResponse> _Handler)
                    return await _Handler.InvokeAsync(request, cancellationToken);

                await _Pipe.InvokeAsync(request, cancellationToken);
            }

            throw new UseCaseConfigurationException($"No handler returning {typeof(TResponse).Name} is registered for {typeof(TRequest).Name}.");
        }
    }
}
