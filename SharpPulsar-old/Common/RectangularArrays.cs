
//
//	This class includes methods to convert Java rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
    public static long[][] RectangularLongArray(int size1, int size2)
    {
        long[][] newArray = new long[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new long[size2];
        }

        return newArray;
    }
}