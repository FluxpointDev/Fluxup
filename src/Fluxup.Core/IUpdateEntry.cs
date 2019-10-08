using SemVersion;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace Fluxup.Core
{
    /// <summary>
    /// Entry of a update that the application has
    /// </summary>
    public interface IUpdateEntry
    {
        /// <summary>
        /// The update filename
        /// </summary>
        string Filename { get; }
        
        /// <summary>
        /// The filesize of the update (in bytes)
        /// </summary>
        long Filesize { get; }
        
        /// <summary>
        /// If the update is a delta package
        /// </summary>
        bool IsDelta { get; }
        
        /// <summary>
        /// The SHA1 of the update file
        /// </summary>
        string SHA1 { get; }
        
        /// <summary>
        /// The version that this update will bump the application to
        /// </summary>
        SemanticVersion Version { get; }

        /// <summary>
        /// Gets the release note for this update
        /// </summary>
        /// <returns>Release note</returns>
        Task<string> FetchReleaseNote();
    }
}
