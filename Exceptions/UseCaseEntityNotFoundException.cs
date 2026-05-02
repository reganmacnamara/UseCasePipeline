namespace UseCasePipeline.Exceptions
{
    /// <summary>
    /// Thrown by a <c>IUseCaseEntityValidator</c> when a required entity does not exist.
    /// Indicates the request referenced something that could not be found.
    /// </summary>
    public class UseCaseEntityNotFoundException : Exception
    {
        /// <summary>The type name of the entity that was not found.</summary>
        public string EntityName { get; }

        /// <summary>The identifier that was looked up.</summary>
        public object? EntityId { get; }

        public UseCaseEntityNotFoundException(string entityName, object? entityId)
            : base($"{entityName} with id '{entityId}' was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public UseCaseEntityNotFoundException(string message)
            : base(message)
        {
            EntityName = string.Empty;
        }
    }
}
