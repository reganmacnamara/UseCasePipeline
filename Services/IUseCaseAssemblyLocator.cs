using System.Reflection;

namespace UseCasePipeline.Services
{
    /// <summary>
    /// Finds the assemblies to scan for UseCase pipeline components when none are given explicitly.
    /// </summary>
    public interface IUseCaseAssemblyLocator
    {
        /// <summary>
        /// Returns every assembly that references UseCasePipeline and so may contain pipes and handlers.
        /// </summary>
        IReadOnlyList<Assembly> LocateAssemblies();
    }
}
