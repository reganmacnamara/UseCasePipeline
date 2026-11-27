using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal sealed class UseCasePipeProvider(IServiceProvider serviceProvider) : IUseCasePipeProvider
    {
        public IReadOnlyList<IUseCasePipe<TRequest>> GetOrderedPipes<TRequest>() where TRequest : IUseCaseRequest
        {
            // DI can hand every caller the same cached array when all the pipes are singletons, so sort a copy.
            var _Pipes = serviceProvider.GetServices<IUseCasePipe<TRequest>>().ToArray();
            StableSortByOrder(_Pipes);
            return _Pipes;
        }

        private static void StableSortByOrder<TRequest>(IUseCasePipe<TRequest>[] pipes) where TRequest : IUseCaseRequest
        {
            for (var _Next = 1; _Next < pipes.Length; _Next++)
            {
                var _Pipe = pipes[_Next];
                var _Slot = _Next;

                while (_Slot > 0 && pipes[_Slot - 1].Order > _Pipe.Order)
                {
                    pipes[_Slot] = pipes[_Slot - 1];
                    _Slot--;
                }

                pipes[_Slot] = _Pipe;
            }
        }
    }
}
