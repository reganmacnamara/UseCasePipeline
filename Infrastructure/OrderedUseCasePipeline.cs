using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline.Exceptions;
using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal sealed class OrderedUseCasePipeline(IServiceProvider serviceProvider) : IUseCasePipeline
    {
        public async Task InvokeAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
        {
            foreach (var _Pipe in serviceProvider.GetServices<IUseCasePipe<TRequest>>().OrderBy(p => p.Order))
            {
                await _Pipe.InvokeAsync(request, cancellationToken);
            }
        }

        public async Task<TResponse> InvokeWithResponseAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
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
