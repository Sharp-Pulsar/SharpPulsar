
namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From java.util.function.BiConsumer.java
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public interface BiConsumer<T, U>
    {
        /// <summary>
        /// Performs this operation on the given arguments.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="u"></param>
        void Accept(T t, U u);

        /// <summary>
        /// Returns a composed BiConsumer that performs, in sequence, this operation followed by the after operation.
        /// </summary>
        /// <param name="after"></param>
        /// <returns></returns>
        BiConsumer<T, U> AndThen(BiConsumer<T, U> after);
    }
}
