using System;

// ReSharper disable InconsistentNaming
namespace Fluxup.Updater.Exceptions
{
    /// <summary>
    /// SHA1 doesn't match with hash that was excepted
    /// </summary>
    internal class SHA1MatchFailed : Exception
    {
        public SHA1MatchFailed(string fileName, string exceptedHash, string hashComputed)
        {
            Message = $"{fileName} doesn't match with the excepted SHA1 hash\r\nExcepted hash: {exceptedHash}\r\nHash computed: {hashComputed}";
        }
        
        /// <inheritdoc cref="System.Exception.Message"/>
        public override string Message { get; } = "This file doesn't match with the excepted SHA1 hash";
    }
}