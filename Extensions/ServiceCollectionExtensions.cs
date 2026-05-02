using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UseCasePipeline.Infrastructure;

namespace UseCasePipeline.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and scans the provided assemblies
        /// for all UseCase pipeline components (handlers, validators, authorisers),
        /// registering each with the DI container.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="assemblies">
        /// One or more assemblies to scan. Pass the assembly that contains your
        /// use case implementations, e.g. <c>typeof(MyHandler).Assembly</c>.
        /// </param>
        public static IServiceCollection AddUseCasePipeline(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            services.AddScoped<UseCaseMediator>();

            var concreteTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            var openGenerics = new[]
            {
                typeof(IUseCaseHandler<>),
                typeof(IUseCaseHandler<,>),
                typeof(IUseCaseRequestValidator<>),
                typeof(IUseCaseEntityValidator<>),
                typeof(IUseCaseAuthoriser<>),
            };

            foreach (var type in concreteTypes)
            {
                foreach (var openGeneric in openGenerics)
                {
                    var matchingInterfaces = type
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric);

                    foreach (var iface in matchingInterfaces)
                        services.AddTransient(iface, type);
                }
            }

            return services;
        }
    }
}
