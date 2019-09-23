namespace Fluxup.Updater.Exceptions
{
    public class OSUnknownException : System.Exception
    {
        public override string Message { get; } = "Can't find what OS this device is using!!!";
    }
}