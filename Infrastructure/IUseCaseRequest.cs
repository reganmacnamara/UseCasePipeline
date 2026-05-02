namespace UseCasePipeline.Infrastructure
{
    /// <summary>
    /// Marks a class as a request for a UseCase.
    /// </summary>
    public interface IUseCaseRequest { }

    /// <summary>
    /// Marks a class as a request for a UseCase that returns a <typeparamref name="TResponse"/>
    /// </summary>
    public interface IUseCaseRequest<TResponse> : IUseCaseRequest where TResponse : IUseCaseResponse { }
}
