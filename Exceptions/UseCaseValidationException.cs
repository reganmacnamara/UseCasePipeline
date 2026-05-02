namespace UseCasePipeline.Exceptions
{
    /// <summary>
    /// Thrown by a <c>IUseCaseRequestValidator</c> when the incoming request fails validation.
    /// Indicates a client error — the request data itself is invalid.
    /// </summary>
    public class UseCaseValidationException : Exception
    {
        /// <summary>Field-keyed validation errors, e.g. { "Email": ["Must not be empty"] }.</summary>
        public IReadOnlyList<(string property, string error)> Errors { get; }

        public UseCaseValidationException(string message)
            : base(message)
        {
            Errors = [];
        }

        public UseCaseValidationException(IReadOnlyList<(string property, string error)> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
