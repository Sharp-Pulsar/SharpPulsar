using System.Collections.Generic;

namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.type.Type.java
    /// </summary>
    public interface IType
    {
        /// <summary>
        /// Gets the name of this type which must be case insensitive globally unique.
        /// The name of a user defined type must be a legal identifier in Presto.
        /// </summary>
        /// <returns></returns>
        TypeSignature GetTypeSignature();

        /// <summary>
        /// Returns the name of this type that should be displayed to end-users.
        /// </summary>
        /// <returns></returns>
        string GetDisplayName();

        /// <summary>
        /// True if the type supports equalTo and hash.
        /// </summary>
        /// <returns></returns>
        bool IsComparable();

        /// <summary>
        /// True if the type supports compareTo.
        /// </summary>
        /// <returns></returns>
        bool IsOrderable();

        /// <summary>
        /// Gets the Java class type used to represent this value on the stack during
        /// expression execution.This value is used to determine which method should
        /// be called on Cursor, RecordSet or RandomAccessBlock to fetch a value of
        /// this type.
        /// Currently, this must be boolean, long, double, or Slice.
        /// </summary>
        /// <returns></returns>
        System.Type GetJavaType();

        /// <summary>
        /// For parameterized types returns the list of parameters.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IType> GetTypeParameter();

        /// <summary>
        /// Gets an object representation of the type value in the {@code block}
        /// {@code position}. This is the value returned to the user via the
        /// REST endpoint and therefore must be JSON serializable.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        object GetObjectValue(IConnectorSession session, IBlock block, int position);


        /// <summary>
        /// Creates the preferred block builder for this type. This is the builder used to
        /// store values after an expression projection within the query.
        /// </summary>
        /// <param name="blockBuilderStatus"></param>
        /// <param name="expectedEntries"></param>
        /// <returns></returns>
        IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries);

        /// <summary>
        /// Creates the preferred block builder for this type. This is the builder used to
        /// tore values after an expression projection within the query.
        /// </summary>
        /// <param name="blockBuilderStatus"></param>
        /// <param name="expectedEntries"></param>
        /// <param name="expectedBytesPerEntry"></param>
        /// <returns></returns>
        IBlockBuilder CreateBlockBuilder(BlockBuilderStatus blockBuilderStatus, int expectedEntries, int expectedBytesPerEntry);

        /// <summary>
        /// Gets the value at the {@code block} {@code position} as a long.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        long GetLong(IBlock block, int position);

        /// <summary>
        /// Gets the value at the {@code block} {@code position} as a double.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        double GetDouble(IBlock block, int position);

        /// <summary>
        /// Gets the value at the {@code block} {@code position} as a boolean.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool GetBoolean(IBlock block, int position);

        /// <summary>
        /// Gets the value at the {@code block} {@code position} as a Slice.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Slice GetSlice(IBlock block, int position);

        /// <summary>
        /// Gets the value at the {@code block} {@code position} as an Object.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        object GetObject(IBlock block, int position);

        /// <summary>
        /// Writes the boolean value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        void WriteBoolean(IBlockBuilder blockBuilder, bool value);

        /// <summary>
        /// Writes the long value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        void WriteLong(IBlockBuilder blockBuilder, long value);

        /// <summary>
        /// Writes the double value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        void WriteDouble(IBlockBuilder blockBuilder, double value);

        /// <summary>
        /// Writes the Slice value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        void WriteSlice(IBlockBuilder blockBuilder, Slice value);

        /// <summary>
        /// Writes the Slice value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void WriteSlice(IBlockBuilder blockBuilder, Slice value, int offset, int length);

        /// <summary>
        /// Writes the Object value into the {@code BlockBuilder}.
        /// </summary>
        /// <param name="blockBuilder"></param>
        /// <param name="value"></param>
        void WriteObject(IBlockBuilder blockBuilder, object value);

        /// <summary>
        /// Append the value at {@code position} in {@code block} to {@code blockBuilder}.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        /// <param name="blockBuilder"></param>
        void AppendTo(IBlock block, int position, IBlockBuilder blockBuilder);

        /// <summary>
        /// Are the values in the specified blocks at the specified positions equal?
        /// </summary>
        /// <param name="leftBlock"></param>
        /// <param name="leftPosition"></param>
        /// <param name="rightBlock"></param>
        /// <param name="rightPosition"></param>
        bool EqualTo(IBlock leftBlock, int leftPosition, IBlock rightBlock, int rightPosition);

        /// <summary>
        /// Calculates the hash code of the value at the specified position in the
        /// specified block.
        /// </summary>
        /// <param name=""></param>
        /// <param name="position"></param>
        /// <returns></returns>
        long Hash(IBlock block, int position);

        /// <summary>
        /// Compare the values in the specified block at the specified positions equal.
        /// </summary>
        /// <param name="leftBlock"></param>
        /// <param name="leftPosition"></param>
        /// <param name="rightBlock"></param>
        /// <param name="rightPosition"></param>
        /// <returns></returns>
        int CompareTo(IBlock leftBlock, int leftPosition, IBlock rightBlock, int rightPosition);
    }
}
