using System.Collections.Concurrent;
using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal sealed class CachedUseCaseRequestDispatcher : IUseCaseRequestDispatcher
    {
        private readonly ConcurrentDictionary<Type, RequestDispatch> m_requestDispatches = new();
        private readonly ConcurrentDictionary<(Type Request, Type Response), object> m_requestWithResponseDispatches = new();

        public Task DispatchAsync(IUseCaseRequest request, IUseCasePipeline pipeline, CancellationToken cancellationToken)
        {
            var _Dispatch = m_requestDispatches.GetOrAdd(request.GetType(), CreateRequestDispatch);
            return _Dispatch.DispatchAsync(request, pipeline, cancellationToken);
        }

        public Task<TResponse> DispatchAsync<TResponse>(IUseCaseRequest<TResponse> request, IUseCasePipeline pipeline, CancellationToken cancellationToken)
            where TResponse : IUseCaseResponse
        {
            var _Dispatch = (RequestWithResponseDispatch<TResponse>)m_requestWithResponseDispatches
                .GetOrAdd((request.GetType(), typeof(TResponse)), CreateRequestWithResponseDispatch);
            return _Dispatch.DispatchAsync(request, pipeline, cancellationToken);
        }

        private static RequestDispatch CreateRequestDispatch(Type requestType)
            => (RequestDispatch)Activator.CreateInstance(typeof(RequestDispatch<>).MakeGenericType(requestType))!;

        private static object CreateRequestWithResponseDispatch((Type Request, Type Response) types)
            => Activator.CreateInstance(typeof(RequestWithResponseDispatch<,>).MakeGenericType(types.Request, types.Response))!;
    }
}
