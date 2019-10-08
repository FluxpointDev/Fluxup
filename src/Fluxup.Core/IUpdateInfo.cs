using SemVersion;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    /// <summary>
    /// Information about all the updates in one place
    /// </summary>
    /// <typeparam name="TUpdateEntry"><see cref="IUpdateEntry"/> to use</typeparam>
    public interface IUpdateInfo<out TUpdateEntry>
    where TUpdateEntry : IUpdateEntry
    {
        /// <summary>
        /// If the application has a update
        /// </summary>
        bool HasUpdate { get; }
        
        /// <summary>
        /// If the update is required for the application to continue
        /// </summary>
        bool UpdateRequired { get; }
        
        /// <summary>
        /// All the updates the application has
        /// </summary>
        TUpdateEntry[] Updates { get; }
        
        /// <summary>
        /// What is the newest update out of all the updates
        /// </summary>
        SemanticVersion NewestUpdateVersion { get; }

        /// <summary>
        /// Gets all the release notes
        /// </summary>
        /// <returns></returns>
        Task<string[]> FetchReleaseNotes();
    }
}
