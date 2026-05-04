using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UseCasePipeline.Infrastructure;

namespace UseCasePipeline.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and automatically discovers all
        /// UseCase pipeline components (handlers, validators, rule enforcers, authorisers) from every
        /// assembly in the current AppDomain that references UseCasePipeline.
        /// </summary>
        public static IServiceCollection AddUseCasePipeline(this IServiceCollection services)
        {
            var assemblies = GetReferencingAssemblies();
            return services.AddUseCasePipeline(assemblies);
        }

        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and scans the provided assemblies
        /// for all UseCase pipeline components (handlers, validators, rule enforcers, authorisers).
        /// </summary>
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
                typeof(IUseCaseRuleEnforcer<>),
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

        private static Assembly[] GetReferencingAssemblies()
        {
            var pipelineAssemblyName = typeof(IUseCasePipe).Assembly.GetName().FullName;

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetReferencedAssemblies()
                    .Any(r => r.FullName == pipelineAssemblyName))
                .ToArray();
        }
    }
}
