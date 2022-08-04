namespace SharpPulsar.Common.Util
{
    public class MathUtils
    {
        /// <summary>
        /// Compute sign safe mod.
        /// </summary>
        /// <param name="dividend"> </param>
        /// <param name="divisor">
        /// @return </param>
        public static int SignSafeMod(long dividend, int divisor)
        {
            var mod = (int)(dividend % divisor);

            if (mod < 0)
            {
                mod += divisor;
            }

            return mod;
        }
    }

}
