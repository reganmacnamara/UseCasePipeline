using UseCasePipeline.Exceptions;
using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal sealed class OrderedUseCasePipeline(IUseCasePipeProvider pipeProvider) : IUseCasePipeline
    {
        public async Task InvokeAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IUseCaseRequest
        {
            var _Pipes = pipeProvider.GetOrderedPipes<TRequest>();
            if (!ContainsHandler(_Pipes))
                throw new UseCaseConfigurationException($"No handler is registered for {typeof(TRequest).Name}.");

            foreach (var _Pipe in _Pipes)
            {
                await _Pipe.InvokeAsync(request, cancellationToken);
            }
        }

        public async Task<TResponse> InvokeWithResponseAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IUseCaseRequest<TResponse>
            where TResponse : IUseCaseResponse
        {
            foreach (var _Pipe in pipeProvider.GetOrderedPipes<TRequest>())
            {
                if (_Pipe is IUseCasePipe<TRequest, TResponse> _Handler)
                    return await _Handler.InvokeAsync(request, cancellationToken);

                await _Pipe.InvokeAsync(request, cancellationToken);
            }

            throw new UseCaseConfigurationException($"No handler returning {typeof(TResponse).Name} is registered for {typeof(TRequest).Name}.");
        }

        private static bool ContainsHandler<TRequest>(IReadOnlyList<IUseCasePipe<TRequest>> pipes) where TRequest : IUseCaseRequest
            => pipes.Any(_Pipe => _Pipe is IUseCaseHandler<TRequest>);
    }
}
