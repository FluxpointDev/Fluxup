using System;
using SemVersion;

namespace Fluxup.Core
{
    /// <summary>
    /// SemanticVersion extensions
    /// </summary>
    public static class SemVersionEx
    {
        /// <summary>
        /// makes <see cref="versionString"/> into <see cref="SemanticVersion"/>
        /// </summary>
        /// <param name="versionString">version as a string</param>
        /// <returns></returns>
        public static SemanticVersion ParseVersion(this string versionString) 
        {
            SemanticVersion.TryParse(versionString, out var version);
            return version;
        }

        /// <summary>
        /// Turns the normal <see cref="Version"/> into <see cref="SemanticVersion"/>
        /// </summary>
        /// <param name="version">Version to turn into a <see cref="SemanticVersion"/></param>
        /// <returns></returns>
        public static SemanticVersion SystemToSemantic(this Version version)
        {
            return ParseVersion(version.ToString());
        }

        /// <summary>
        /// Turns a <see cref="SemanticVersion"/> into the normal <see cref="Version"/>
        /// </summary>
        /// <param name="semanticVersion">Version to turn into a <see cref="Version"/></param>
        /// <returns></returns>
        public static Version SemanticToSystem(this SemanticVersion semanticVersion) 
        {
            return semanticVersion != null ? new Version(semanticVersion.Major.GetValueOrDefault(), semanticVersion.Minor.GetValueOrDefault(),
                semanticVersion.Patch.GetValueOrDefault(), 0) :
                null;
        }
    }
}
