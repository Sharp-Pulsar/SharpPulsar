namespace SharpPulsar.Trino.Trino
{
    public static class Hashing
    {
        public static int Hash(params object[] args)
        {
            unchecked // Overflow is fine, just wrap
            {
                var Hash = 17;

                foreach (var Item in args)
                {
                    if (Item != null)
                    {
                        Hash = Hash * 23 + Item.GetHashCode();
                    }
                }

                return Hash;
            }
        }
    }
}
