using System;

namespace Fluxup.Updater.Exceptions
{
    public class FolderLocationUnavailable : Exception
    {
        public override string Message { get; } = "The folder with the update contents can not be found.";
    }
}