namespace Fluxup.Core.Exceptions
{
    /// <summary>
    /// The OS the user is using can not be found
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class OSUnknownException : System.Exception
    {
        /// <inheritdoc cref="System.Exception.Message"/>
        public override string Message { get; } = "Can't find what OS this device is using!!!";
    }
}