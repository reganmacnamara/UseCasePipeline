namespace UseCasePipeline.Services
{
    /// <summary>
    /// A custom pipeline stage for a specific UseCase request.
    /// </summary>
    /// <remarks>
    /// Custom pipes run at their <see cref="IUseCasePipe{TRequest}.Order"/>. Inheriting
    /// <see cref="UseCasePipeline.Infrastructure.UseCaseCustomPipe{TRequest}"/> gives an order of 50,
    /// which runs after the built-in authorisation, validation, entity validation and rule
    /// enforcement stages and before the request handler.
    /// </remarks>
    /// <typeparam name="TRequest">The concrete UseCase request type handled by this pipe.</typeparam>
    public interface IUseCaseCustomPipe<TRequest> : IUseCasePipe<TRequest> where TRequest : IUseCaseRequest { }
}
