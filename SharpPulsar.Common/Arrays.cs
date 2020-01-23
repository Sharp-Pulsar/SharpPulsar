//---------------------------------------------------------------------------------------------------------
//	This class is used to replace some calls to java.util.Arrays methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------
internal static class Arrays
{
	public static T[] CopyOf<T>(T[] original, int newLength)
	{
		T[] dest = new T[newLength];
		System.Array.Copy(original, dest, newLength);
		return dest;
	}

	public static T[] CopyOfRange<T>(T[] original, int fromIndex, int toIndex)
	{
		int length = toIndex - fromIndex;
		T[] dest = new T[length];
		System.Array.Copy(original, fromIndex, dest, 0, length);
		return dest;
	}

	public static void Fill<T>(T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
	{
		for (int i = fromIndex; i < toIndex; i++)
		{
			array[i] = value;
		}
	}
}