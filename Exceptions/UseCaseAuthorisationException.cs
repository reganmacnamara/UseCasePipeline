namespace UseCasePipeline.Exceptions
{
    /// <summary>
    /// Thrown by a <c>IUseCaseAuthoriser</c> when the caller lacks permission to execute the use case.
    /// </summary>
    public class UseCaseAuthorisationException : Exception
    {
        public UseCaseAuthorisationException()
            : base("You are not authorised to perform this action.") { }

        public UseCaseAuthorisationException(string message)
            : base(message) { }
    }
}
