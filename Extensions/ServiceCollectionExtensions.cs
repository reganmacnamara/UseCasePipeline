using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using UseCasePipeline.Infrastructure;
using UseCasePipeline.Services;

namespace UseCasePipeline.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and automatically discovers all
        /// UseCase pipeline components (handlers, validators, rule enforcers, authorisers, custom pipes) from every
        /// assembly the application references that references UseCasePipeline, loading any that aren't loaded yet.
        /// </summary>
        public static IServiceCollection AddUseCasePipeline(this IServiceCollection services)
            => services.AddUseCasePipeline(new ApplicationAssemblyLocator());

        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and scans the assemblies found by
        /// <paramref name="assemblyLocator"/> for all UseCase pipeline components.
        /// </summary>
        public static IServiceCollection AddUseCasePipeline(this IServiceCollection services, IUseCaseAssemblyLocator assemblyLocator)
            => services.AddUseCasePipeline(assemblyLocator.LocateAssemblies().ToArray());

        /// <summary>
        /// Registers the <see cref="UseCaseMediator"/> and scans the provided assemblies
        /// for all UseCase pipeline components (handlers, validators, rule enforcers, authorisers, custom pipes).
        /// </summary>
        public static IServiceCollection AddUseCasePipeline(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            services.TryAddScoped<UseCaseMediator>();
            services.TryAddScoped<IUseCasePipeline, OrderedUseCasePipeline>();
            services.TryAddScoped<IUseCasePipeProvider, UseCasePipeProvider>();
            services.TryAddSingleton<IUseCaseRequestDispatcher, CachedUseCaseRequestDispatcher>();

            var concreteTypes = assemblies
                .Distinct()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            var openGenerics = new[]
            {
                typeof(IUseCasePipe<>),
                typeof(IUseCaseHandler<>),
                typeof(IUseCaseHandler<,>),
                typeof(IUseCaseRequestValidator<>),
                typeof(IUseCaseEntityValidator<>),
                typeof(IUseCaseRuleEnforcer<>),
                typeof(IUseCaseAuthoriser<>),
                typeof(IUseCaseCustomPipe<>),
            };

            foreach (var type in concreteTypes)
            {
                foreach (var openGeneric in openGenerics)
                {
                    var matchingInterfaces = type
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric);

                    foreach (var iface in matchingInterfaces)
                        services.TryAddEnumerable(ServiceDescriptor.Transient(iface, type));
                }
            }

            return services;
        }
    }
}
