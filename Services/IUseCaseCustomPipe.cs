namespace UseCasePipeline.Services
{
    /// <summary>
    /// A custom pipeline stage for a specific UseCase request.
    /// </summary>
    /// <remarks>
    /// Custom pipes run after the built-in authorisation, validation, entity validation,
    /// and rule enforcement stages, and before the request handler. Multiple custom pipes
    /// for the same request are executed sequentially in dependency injection registration order.
    /// </remarks>
    /// <typeparam name="TRequest">The concrete UseCase request type handled by this pipe.</typeparam>
    public interface IUseCaseCustomPipe<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest { }
}
