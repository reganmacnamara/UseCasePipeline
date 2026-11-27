using System.Reflection;
using UseCasePipeline.Services;

namespace UseCasePipeline.Infrastructure
{
    internal sealed class ApplicationAssemblyLocator : IUseCaseAssemblyLocator
    {
        private static readonly string[] s_frameworkAssemblyNames = ["mscorlib", "netstandard", "System"];
        private static readonly string[] s_frameworkAssemblyPrefixes = ["Microsoft.", "System."];
        private static readonly string? s_pipelineAssemblyName = typeof(IUseCasePipe).Assembly.GetName().Name;

        public IReadOnlyList<Assembly> LocateAssemblies()
            => LoadApplicationAssemblies().Where(ReferencesPipelineAssembly).ToList();

        private static IEnumerable<Assembly> LoadApplicationAssemblies()
        {
            var _Loaded = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
            var _Pending = new Stack<Assembly>(GetStartingAssemblies());

            while (_Pending.Count > 0)
            {
                var _Assembly = _Pending.Pop();
                if (!_Loaded.TryAdd(_Assembly.GetName().Name!, _Assembly))
                    continue;

                foreach (var _Reference in _Assembly.GetReferencedAssemblies())
                {
                    if (!IsFrameworkAssembly(_Reference) && !_Loaded.ContainsKey(_Reference.Name!) && TryLoad(_Reference) is { } _Referenced)
                        _Pending.Push(_Referenced);
                }
            }

            return _Loaded.Values;
        }

        private static IEnumerable<Assembly> GetStartingAssemblies()
        {
            var _AlreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Where(_Assembly => !_Assembly.IsDynamic && !IsFrameworkAssembly(_Assembly.GetName()));
            var _EntryAssembly = Assembly.GetEntryAssembly();

            return _EntryAssembly is null ? _AlreadyLoaded : _AlreadyLoaded.Append(_EntryAssembly);
        }

        private static bool IsFrameworkAssembly(AssemblyName assemblyName)
        {
            var _Name = assemblyName.Name ?? string.Empty;

            return s_frameworkAssemblyNames.Contains(_Name, StringComparer.OrdinalIgnoreCase)
                || s_frameworkAssemblyPrefixes.Any(_Prefix => _Name.StartsWith(_Prefix, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ReferencesPipelineAssembly(Assembly assembly)
            => assembly.GetReferencedAssemblies().Any(_Reference => string.Equals(_Reference.Name, s_pipelineAssemblyName, StringComparison.OrdinalIgnoreCase));

        private static Assembly? TryLoad(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception exception) when (exception is FileNotFoundException or FileLoadException or BadImageFormatException)
            {
                // A reference that isn't deployed with the app can't contain its pipes, so it's skipped rather than failing startup.
                return null;
            }
        }
    }
}
