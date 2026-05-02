namespace UseCasePipeline.Exceptions
{
    /// <summary>
    /// Thrown by a <c>IUseCaseRequestValidator</c> when the incoming request fails validation.
    /// Indicates a client error — the request data itself is invalid.
    /// </summary>
    public class UseCaseValidationException : Exception
    {
        /// <summary>Field-keyed validation errors, e.g. { "Email": ["Must not be empty"] }.</summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public UseCaseValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public UseCaseValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = new Dictionary<string, string[]>(errors);
        }
    }
}
