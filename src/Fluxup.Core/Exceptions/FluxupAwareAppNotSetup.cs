using System;

namespace Fluxup.Core.Exceptions
{
    /// <summary>
    /// The FluxupAwareApp class has not been set up
    /// </summary>
    internal class FluxupAwareAppNotSetup : Exception
    {
        /// <inheritdoc cref="System.Exception.Message"/>
        public override string Message { get; } = "FluxupAwareApp has not been setup, have you called FluxupAwareApp.Setup() in your Fluxup update package?";
    }
}