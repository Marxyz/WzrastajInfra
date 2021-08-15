using System;

namespace Data
{
    internal static class GuidExtension
    {
        public static bool IsEmpty(this Guid guid)
        {
            return guid == Guid.Empty || guid.Equals(null);
        }
    }
}