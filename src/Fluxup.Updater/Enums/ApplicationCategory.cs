// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Fluxup.Updater.Enums
{
    //Based on https://specifications.freedesktop.org/menu-spec/latest/apas02.html
    /// <summary>
    /// Categories that your application is in.
    /// </summary>
    public enum ApplicationCategory
    {
        /////////////////////
        // Main Categories //
        /////////////////////
        
        /// <summary>
        /// Application for presenting, creating, or processing multimedia (audio/video)
        /// </summary>
        AudioVideo,

        /// <summary>
        /// An audio application
        /// </summary>
        Audio,
        
        /// <summary>
        /// A video application
        /// </summary>
        Video,
        
        /// <summary>
        /// An application for development
        /// </summary>
        Development,
        
        /// <summary>
        /// Educational software
        /// </summary>
        Education,
        
        /// <summary>
        /// A game
        /// </summary>
        Game,
        
        /// <summary>
        /// Application for viewing, creating, or processing graphics
        /// </summary>
        Graphics,
        
        /// <summary>
        /// Network application such as a web browser
        /// </summary>
        Network,
        
        /// <summary>
        /// An office type application
        /// </summary>
        Office,
        
        /// <summary>
        /// Scientific software
        /// </summary>
        Science,
        
        /// <summary>
        /// Settings applications
        /// </summary>
        Settings,
        
        /// <summary>
        /// System application, "System Tools" such as say a log viewer or network monitor
        /// </summary>
        System,
        
        /// <summary>
        /// Small utility application, "Accessories"
        /// </summary>
        Utility,
        
        ////////////////////
        // Sub-Categories //
        ////////////////////
        //  Development   //
        ////////////////////
        
        /// <summary>
        /// A tool to build applications
        /// </summary>
        Building,
        
        /// <summary>
        /// A tool to debug applications
        /// </summary>
        Debugger,
        
        /// <summary>
        /// IDE application
        /// </summary>
        IDE,
        
        /// <summary>
        /// A GUI designer application
        /// </summary>
        GUIDesigner,
        
        /// <summary>
        /// A profiling tool
        /// </summary>
        Profiling,
        
        /// <summary>
        /// Applications like cvs or subversion
        /// </summary>
        RevisionControl,
        
        /// <summary>
        /// A translation tool
        /// </summary>
        Translation,
        
        /// <summary>
        /// A tool for web developers
        /// </summary>
        WebDevelopment,
        
        /////////////
        // Office  //
        /////////////
        
        /// <summary>
        /// Calendar application
        /// </summary>
        Calendar,
        
        /// <summary>
        /// E.g. an address book
        /// </summary>
        ContactManagement,
        
        /// <summary>
        /// Application to manage a database
        /// </summary>
        Database,
        
        /// <summary>
        /// A dictionary
        /// </summary>
        Dictionary,
        
        /// <summary>
        /// Chart application
        /// </summary>
        Chart,
        
        /// <summary>
        /// Email application
        /// </summary>
        Email,
        
        /// <summary>
        /// Application to manage your finance
        /// </summary>
        Finance,
        
        /// <summary>
        /// A flowchart application
        /// </summary>
        FlowChart,
        
        /// <summary>
        /// Tool to manage your PDA
        /// </summary>
        PDA,
        
        /// <summary>
        /// Project management application
        /// </summary>
        ProjectManagement,
        
        /// <summary>
        /// Presentation software
        /// </summary>
        Presentation,
        
        /// <summary>
        /// A spreadsheet
        /// </summary>
        Spreadsheet,
        
        /// <summary>
        /// A word processor
        /// </summary>
        WordProcessor,
        
        //////////////
        // Graphics //
        //////////////
        
        /// <summary>
        /// 2D based graphical application
        /// </summary>
        Graphics2D,
        
        /// <summary>
        /// Application for viewing, creating, or processing vector graphics
        /// </summary>
        VectorGraphics,
        
        /// <summary>
        /// Application for viewing, creating, or processing raster (bitmap) graphics
        /// </summary>
        RasterGraphics,
        
        /// <summary>
        /// Application for viewing, creating, or processing 3-D graphics
        /// </summary>
        Graphics3D,
        
        /// <summary>
        /// Tool to scan a file/text
        /// </summary>
        Scanning,
        
        /// <summary>
        /// Optical character recognition application
        /// </summary>
        OCR,
        
        /// <summary>
        /// Camera tools, etc.
        /// </summary>
        Photography,
        
        /// <summary>
        /// Desktop Publishing applications and Color Management tools
        /// </summary>
        Publishing,
        
        /// <summary>
        /// Tool to view e.g. a graphic or pdf file
        /// </summary>
        Viewer,
        
        /////////////
        // Utility //
        /////////////
        
        /// <summary>
        /// A text tool utility
        /// </summary>
        TextTools,
        
        /// <summary>
        /// Telephony tools, to dial a number, manage PBX, ...
        /// </summary>
        TelephonyTools,
        
        /// <summary>
        /// Software for viewing maps, navigation, mapping, GPS
        /// </summary>
        Maps,
        
        /// <summary>
        /// A tool to archive/backup data
        /// </summary>
        Archiving,
        
        /// <summary>
        /// A tool to manage compressed data/archives
        /// </summary>
        Compression,
        
        /// <summary>
        /// A file tool utility
        /// </summary>
        FileTools,
        
        /// <summary>
        /// A calculator
        /// </summary>
        Calculator,
        
        /// <summary>
        /// A clock application/applet
        /// </summary>
        Clock,
        
        /// <summary>
        /// A text editor
        /// </summary>
        TextEditor,
        
        //////////////
        // Settings //
        //////////////
        
        /// <summary>
        /// Configuration tool for the GUI
        /// </summary>
        DesktopSettings,
        
        /// <summary>
        /// A tool to manage hardware components, like sound cards, video cards or printers
        /// </summary>
        HardwareSettings,
        
        /// <summary>
        /// A tool to manage printers
        /// </summary>
        Printing,
        
        /// <summary>
        /// A package manager application
        /// </summary>
        PackageManager,
        
        /////////////
        // Network //
        /////////////
        
        /// <summary>
        /// A dial-up program
        /// </summary>
        Dialup,
        
        /// <summary>
        /// An instant messaging client
        /// </summary>
        InstantMessaging,
        
        /// <summary>
        /// A chat client
        /// </summary>
        Chat,
        
        /// <summary>
        /// An IRC client
        /// </summary>
        IRCClient,
        
        /// <summary>
        /// RSS, podcast and other subscription based contents
        /// </summary>
        Feed,
        
        /// <summary>
        /// Tools like FTP or P2P programs
        /// </summary>
        FileTransfer,
        
        /// <summary>
        /// HAM radio software
        /// </summary>
        HamRadio,
        
        /// <summary>
        /// A news reader or a news ticker
        /// </summary>
        News,
        
        /// <summary>
        /// A P2P program
        /// </summary>
        P2P,
        
        /// <summary>
        /// A tool to remotely manage your PC
        /// </summary>
        RemoteTools,
        
        /// <summary>
        /// Telephony via PC
        /// </summary>
        Telephony,
        
        /// <summary>
        /// Video Conference software
        /// </summary>
        VideoConference,
        
        /// <summary>
        /// A web browser
        /// </summary>
        WebBrowser,
        
        ///////////
        // Audio //
        ///////////
        
        /// <summary>
        /// An app related to MIDI
        /// </summary>
        Midi,
        
        /// <summary>
        /// Just a mixer
        /// </summary>
        Mixer,
        
        /// <summary>
        /// A sequencer
        /// </summary>
        Sequencer,
        
        /// <summary>
        /// A tuner
        /// </summary>
        Tuner,
        
        ///////////
        // Video //
        ///////////
        
        /// <summary>
        /// A TV application
        /// </summary>
        TV,
        
        /////////////////////
        // Audio and Video //
        /////////////////////
        
        /// <summary>
        /// Application to edit audio/video files
        /// </summary>
        AudioVideoEditing,
        
        /// <summary>
        /// Application to play audio/video files
        /// </summary>
        Player,
        
        /// <summary>
        /// Application to record audio/video files
        /// </summary>
        Recorder,
        
        /// <summary>
        /// Application to burn a disc
        /// </summary>
        DiscBurning,
        
        //////////
        // Game //
        //////////
        
        /// <summary>
        /// An action game
        /// </summary>
        ActionGame,
        
        /// <summary>
        /// Adventure style game
        /// </summary>
        AdventureGame,
        
        /// <summary>
        /// Arcade style game
        /// </summary>
        ArcadeGame,
        
        /// <summary>
        /// A board game
        /// </summary>
        BoardGame,
        
        /// <summary>
        /// Falling blocks game
        /// </summary>
        BlocksGame,
        
        /// <summary>
        /// A card game
        /// </summary>
        CardGame,
        
        /// <summary>
        /// A game for kids
        /// </summary>
        KidsGame,
        
        /// <summary>
        /// Logic games like puzzles, etc
        /// </summary>
        LogicGame,
        
        /// <summary>
        /// A role playing game
        /// </summary>
        RolePlaying,
        
        /// <summary>
        /// A shooter game
        /// </summary>
        Shooter,
        
        /// <summary>
        /// A simulation game
        /// </summary>
        Simulation,
        
        /// <summary>
        /// A sports game
        /// </summary>
        SportsGame,
        
        /// <summary>
        /// A strategy game
        /// </summary>
        StrategyGame,
        
        ///////////////
        // Education //
        ///////////////
        
        /// <summary>
        /// Software to teach arts
        /// </summary>
        Art,
        
        /// <summary>
        /// 
        /// </summary>
        Construction,
        
        /// <summary>
        /// Musical software
        /// </summary>
        Music,
        
        /// <summary>
        /// Software to learn foreign languages
        /// </summary>
        Languages,
        
        /// <summary>
        /// Artificial Intelligence software
        /// </summary>
        ArtificialIntelligence,
        
        /// <summary>
        /// Astronomy software
        /// </summary>
        Astronomy,
        
        /// <summary>
        /// Biology software
        /// </summary>
        Biology,
        
        /// <summary>
        /// Chemistry software
        /// </summary>
        Chemistry,
        
        /// <summary>
        /// Computer Science software
        /// </summary>
        ComputerScience,
        
        /// <summary>
        /// Data visualization software
        /// </summary>
        DataVisualization,
        
        /// <summary>
        /// Economy software
        /// </summary>
        Economy,
        
        /// <summary>
        /// Electricity software
        /// </summary>
        Electricity,
        
        /// <summary>
        /// Geography software
        /// </summary>
        Geography,
        
        /// <summary>
        /// Geology software
        /// </summary>
        Geology,
        
        /// <summary>
        /// Geoscience software, GIS
        /// </summary>
        Geoscience,
        
        /// <summary>
        /// History software
        /// </summary>
        History,
        
        /// <summary>
        /// Software for philosophy, psychology and other humanities
        /// </summary>
        Humanities,
        
        /// <summary>
        /// Image Processing software
        /// </summary>
        ImageProcessing,
        
        /// <summary>
        /// Literature software
        /// </summary>
        Literature,
        
        /// <summary>
        /// Math software
        /// </summary>
        Math,
        
        /// <summary>
        /// Numerical analysis software
        /// </summary>
        NumericalAnalysis,
        
        /// <summary>
        /// Medical software
        /// </summary>
        MedicalSoftware,
        
        /// <summary>
        /// Physics software
        /// </summary>
        Physics,
        
        /// <summary>
        /// Robotics software
        /// </summary>
        Robotics,
        
        /// <summary>
        /// Religious and spiritual software, theology
        /// </summary>
        Spirituality,
        
        /// <summary>
        /// Sports software
        /// </summary>
        Sports,
        
        /// <summary>
        /// Parallel computing software
        /// </summary>
        ParallelComputing,
        
        /////////
        // ??? //
        /////////
        
        /// <summary>
        /// A simple amusement
        /// </summary>
        Amusement,
        
        /// <summary>
        /// Electronics software, e.g. a circuit designer
        /// </summary>
        Electronics,
        
        /// <summary>
        /// Engineering software, e.g. CAD programs
        /// </summary>
        Engineering,
        
        /// <summary>
        /// Emulator of another platform, such as a DOS emulator
        /// </summary>
        Emulator,
        
        /// <summary>
        /// Help or documentation
        /// </summary>
        Documentation,
        
        /// <summary>
        /// Application handles adult or explicit material
        /// </summary>
        Adult,
        
        /// <summary>
        /// Application that only works inside a terminal (text-based or command line application)
        /// </summary>
        ConsoleOnly,
        
        ////////////
        // System //
        ////////////
        
        /// <summary>
        /// A file manager
        /// </summary>
        FileManager,
        
        /// <summary>
        /// A terminal emulator application
        /// </summary>
        TerminalEmulator,
        
        /// <summary>
        /// A file system tool
        /// </summary>
        Filesystem,
        
        /// <summary>
        /// Monitor application/applet that monitors some resource or activity
        /// </summary>
        Monitor,
        
        /// <summary>
        /// A security tool
        /// </summary>
        Security,
        
        /// <summary>
        /// Accessibility
        /// </summary>
        Accessibility,
    }
}