using System;

namespace Aiplugs.PoshApp.Exceptions
{
    public class PoshAppCanceledException : Exception
    {
        public PoshAppCanceledException() : base() { }
        public PoshAppCanceledException(string message) : base(message) { }
    }
}
