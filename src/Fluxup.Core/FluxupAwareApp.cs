using System;
using SemVersion;
using Fluxup.Core.Exceptions;

// ReSharper disable InconsistentNaming
namespace Fluxup.Core
{
    /// <summary>
    /// Container for OnFirstRun and OnUpdate
    /// </summary>
    public static class FluxupAwareApp
    {
        private static bool hasOnFirstRunBeenHooked;
        private static bool hasOnUpdateBeenHooked;

        private static event EventHandler onFirstRun;
        /// <summary>
        /// Fires when this is the first time the application is being loaded.
        /// </summary>
        public static event EventHandler OnFirstRun
        {
            add
            {
                if (hasOnFirstRunBeenHooked)
                {
                    onFirstRun += value;
                }

                throw new FluxupAwareAppNotSetup();
            }
            remove
            {
                if (hasOnFirstRunBeenHooked)
                {
                    onFirstRun -= value;
                }

                throw new FluxupAwareAppNotSetup();
            }
        }

        private static event EventHandler<SemanticVersion> onUpdate;
        /// <summary>
        /// Fires when an update was applied
        /// </summary>
        public static event EventHandler<SemanticVersion> OnUpdate
        {
            add
            {
                if (hasOnUpdateBeenHooked)
                {
                    onUpdate += value;
                }

                throw new FluxupAwareAppNotSetup();
            }
            remove
            {
                if (hasOnUpdateBeenHooked)
                {
                    onUpdate -= value;
                }

                throw new FluxupAwareAppNotSetup();
            }
        }

        /// <summary>
        /// Gives you a way to trigger OnFirstRun
        /// </summary>
        internal static Action<object> TriggerOnFirstRun()
        {
            hasOnFirstRunBeenHooked = true;
            return obj => onFirstRun?.Invoke(obj, EventArgs.Empty);
        }
        
        /// <summary>
        /// Gives you a way to trigger OnUpdate
        /// </summary>
        internal static Action<object> TriggerOnUpdate(SemanticVersion version)
        {
            hasOnUpdateBeenHooked = true;
            return obj => onUpdate?.Invoke(obj, version);
        }
    }
}
