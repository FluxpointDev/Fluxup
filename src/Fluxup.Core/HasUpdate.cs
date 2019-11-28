using System.Reflection;
using SemVersion;

namespace Fluxup.Core
{
    internal static class HasUpdate
    {
        private static AssemblyName assembly => Assembly.GetEntryAssembly().GetName();
        
        public static bool ApplicationHasUpdate(SemanticVersion version)
        {
            return version.SemanticToSystem() > assembly.Version;
        }
    }
}