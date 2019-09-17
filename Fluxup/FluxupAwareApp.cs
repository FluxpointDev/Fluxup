using System;

namespace Fluxup.Updater
{
    public class FluxupAwareApp
    {
        public static event EventHandler OnFirstRun;
        public static event EventHandler<Version> OnUpdate;
    }
}
