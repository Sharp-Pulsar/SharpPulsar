// https://github.com/bamcis-io/TimeUnit/blob/main/TimeUnit/Hashing.cs
namespace SharpPulsar.TimeUnit
{
    /// <summary>
    /// Provides a method for implementing custom GetHashCode() overrides.
    /// </summary>
    internal class Hashing
    {
        /// <summary>
        /// Computes a hash for a set of objects
        /// </summary>
        /// <param name="args">The arguments to hash</param>
        /// <returns>The hash code of the objects</returns>
        internal static int Hash(params object[] args)
        {
            unchecked // Overflow is fine, just wrap
            {
                var Hash = 17;

                foreach (var Item in args)
                {
                    if (Item != null)
                    {
                        Hash = (Hash * 23) + Item.GetHashCode();
                    }
                }

                return Hash;
            }
        }
    }
}
