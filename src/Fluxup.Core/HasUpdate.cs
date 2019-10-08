using System.Reflection;
using SemVersion;

namespace Fluxup.Core
{
    internal static class HasUpdate
    {
        public static bool ApplicationHasUpdate(SemanticVersion version)
        {
            return version.SemanticToSystem() > Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}