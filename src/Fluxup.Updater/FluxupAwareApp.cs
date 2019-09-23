using System;

namespace Fluxup.Updater
{
    public static class FluxupAwareApp
    {
        public static event EventHandler OnFirstRun
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public static event EventHandler<Version> OnUpdate
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }
    }
}
