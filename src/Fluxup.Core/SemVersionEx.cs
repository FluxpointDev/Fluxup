using System;
using SemVersion;

namespace Fluxup.Core
{
    public static class SemVersionEx
    {
        public static SemanticVersion ParseVersion(this string versionString) 
        {
            SemanticVersion.TryParse(versionString, out var version);
            return version;
        }

        public static Version SemanticToSystem(this SemanticVersion semanticVersion) 
        {
            return semanticVersion != null ? new Version(semanticVersion.Major.GetValueOrDefault(), semanticVersion.Minor.GetValueOrDefault(),
                semanticVersion.Patch.GetValueOrDefault(), 0) :
                null;
        }
    }
}
