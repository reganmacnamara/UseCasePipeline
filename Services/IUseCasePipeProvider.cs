namespace UseCasePipeline.Services
{
    /// <summary>
    /// Supplies the pipes registered for a request in the order they run.
    /// </summary>
    public interface IUseCasePipeProvider
    {
        /// <summary>
        /// Returns the pipes registered for <typeparamref name="TRequest"/> in ascending
        /// <see cref="IUseCasePipe{TRequest}.Order"/>, keeping registration order for pipes with the same order.
        /// </summary>
        IReadOnlyList<IUseCasePipe<TRequest>> GetOrderedPipes<TRequest>() where TRequest : IUseCaseRequest;
    }
}
