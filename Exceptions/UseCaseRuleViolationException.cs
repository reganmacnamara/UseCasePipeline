namespace UseCasePipeline.Exceptions
{
    /// <summary>
    /// Thrown by a <c>IUseCaseRuleEnforcer</c> when a request violates a business rule.
    /// Indicates the request is structurally valid, but cannot be completed under current domain rules.
    /// </summary>
    public class UseCaseRuleViolationException : Exception
    {
        /// <summary>Field-keyed rule violations, e.g. { "Order": ["Customer is on credit hold"] }.</summary>
        public IReadOnlyList<(string property, string error)> Errors { get; }

        /// <summary>
        /// Creates a rule violation exception with a single message.
        /// </summary>
        public UseCaseRuleViolationException(string message)
            : base(message)
        {
            Errors = [];
        }

        /// <summary>
        /// Creates a rule violation exception with one or more field-keyed errors.
        /// </summary>
        public UseCaseRuleViolationException(IReadOnlyList<(string property, string error)> errors)
            : base("One or more rule violations occurred.")
        {
            Errors = errors;
        }
    }
}
