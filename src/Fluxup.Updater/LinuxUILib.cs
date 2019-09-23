namespace Fluxup.Updater
{
    public enum LinuxUILib
    {
        /// <summary>
        /// Not using a linux only UI lib
        /// </summary>
        None,
        
        /// <summary>
        /// Application based on KDE libraries
        /// </summary>
        KDE,
        
        /// <summary>
        /// Application based on GNOME libraries
        /// </summary>
        GNOME,
        
        /// <summary>
        /// Application based on XFCE libraries
        /// </summary>
        XFCE,
        
        /// <summary>
        /// Application based on GTK+ libraries
        /// </summary>
        GTK,
        
        /// <summary>
        /// Application based on Qt libraries
        /// </summary>
        Qt,
        
        /// <summary>
        /// Application based on Motif libraries
        /// </summary>
        Motif,
        
        /// <summary>
        /// Application based on Java GUI libraries, such as AWT or Swing
        /// </summary>
        Java,
    }
}