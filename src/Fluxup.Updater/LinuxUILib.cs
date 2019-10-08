// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace Fluxup.Updater
{
    /// <summary>
    /// UI libraries made for linux
    /// </summary>
    public enum LinuxUILib
    {
        /// <summary>
        /// Not using a linux only UI library
        /// </summary>
        None,
        
        /// <summary>
        /// Application based on Kde library
        /// </summary>
        Kde,
        
        /// <summary>
        /// Application based on Gnome library
        /// </summary>
        Gnome,
        
        /// <summary>
        /// Application based on Xfce library
        /// </summary>
        Xfce,
        
        /// <summary>
        /// Application based on Gtk+ library
        /// </summary>
        GtkPlus,
        
        /// <summary>
        /// Application based on Qt library
        /// </summary>
        Qt,
        
        /// <summary>
        /// Application based on Motif library
        /// </summary>
        Motif,
        
        /// <summary>
        /// Application based on Java library, such as AWT or Swing
        /// </summary>
        Java,
    }
}