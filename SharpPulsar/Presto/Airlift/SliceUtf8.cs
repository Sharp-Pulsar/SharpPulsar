using System;
using System.Globalization;
using SharpPulsar.Precondition;
using SharpPulsar.Presto.Facebook.Type;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Presto.Airlift
{

	/// <summary>
	/// Utility methods for UTF-8 encoded slices.
	/// </summary>
	public sealed class SliceUtf8
	{
		private SliceUtf8()
		{
		}

        private const int MinSurrogate = '\uD800';
        private const int MaxSurrogate = '\uDFFF';

		private const int MinSupplementaryCodePoint = 65536;
		private const int MinHighSurrogateCodePoint = 0xD800;
		private const int ReplacementCodePoint = 0xFFFD;
        private static readonly long MAX_CODE_POINT = 1114111;
		private const int TopMask32 = unchecked((int)0x8080_8080);
		private const long TopMask64 = unchecked((long)0x8080_8080_8080_8080L);

		private static readonly int[] LowerCodePoints;
		private static readonly int[] UpperCodePoints;
		private static readonly bool[] WhitespaceCodePoints;

		static SliceUtf8()
		{
			LowerCodePoints = new int[MAX_CODE_POINT + 1];
			UpperCodePoints = new int[MAX_CODE_POINT + 1];
			WhitespaceCodePoints = new bool[MAX_CODE_POINT + 1];
			for (int codePoint = 0; codePoint <= MAX_CODE_POINT; codePoint++)
			{
				int type = char.GetUnicodeCategory((char)codePoint);
				if (type != UnicodeCategory.Surrogate)
				{
					LowerCodePoints[codePoint] = char.ToLower(codePoint);
					UpperCodePoints[codePoint] = char.ToUpper(codePoint);
					WhitespaceCodePoints[codePoint] = char.IsWhiteSpace(codePoint);
				}
				else
				{
					LowerCodePoints[codePoint] = ReplacementCodePoint;
					UpperCodePoints[codePoint] = ReplacementCodePoint;
					WhitespaceCodePoints[codePoint] = false;
				}
			}
		}

		/// <summary>
		/// Does the slice contain only 7-bit ASCII characters.
		/// </summary>
		public static bool IsAscii(Slice utf8)
		{
			int length = utf8.Length;
			int offset = 0;

			// Length rounded to 8 bytes
			int length8 = length & 0x7FFF_FFF8;
			for (; offset < length8; offset += 8)
			{
				if ((utf8.GetLongUnchecked(offset) & TopMask64) != 0)
				{
					return false;
				}
			}
			// Enough bytes left for 32 bits?
			if (offset + 4 < length)
			{
				if ((utf8.GetIntUnchecked(offset) & TopMask32) != 0)
				{
					return false;
				}

				offset += 4;
			}
			// Do the rest one by one
			for (; offset < length; offset++)
			{
				if ((utf8.GetByteUnchecked(offset) & 0x80) != 0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Counts the code points within UTF-8 encoded slice.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int CountCodePoints(Slice utf8)
		{
			return CountCodePoints(utf8, 0, utf8.Length);
		}

		/// <summary>
		/// Counts the code points within UTF-8 encoded slice up to {@code length}.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int CountCodePoints(Slice utf8, int offset, int length)
		{
			ParameterCondition.CheckPositionIndexes(offset, offset + length, utf8.Length);

			// Quick exit if empty string
			if (length == 0)
			{
				return 0;
			}

			int continuationBytesCount = 0;
			// Length rounded to 8 bytes
			int length8 = length & 0x7FFF_FFF8;
			for (; offset < length8; offset += 8)
			{
				// Count bytes which are NOT the start of a code point
				continuationBytesCount += CountContinuationBytes(utf8.GetLongUnchecked(offset));
			}
			// Enough bytes left for 32 bits?
			if (offset + 4 < length)
			{
				// Count bytes which are NOT the start of a code point
				continuationBytesCount += CountContinuationBytes(utf8.GetIntUnchecked(offset));

				offset += 4;
			}
			// Do the rest one by one
			for (; offset < length; offset++)
			{
				// Count bytes which are NOT the start of a code point
				continuationBytesCount += CountContinuationBytes(utf8.GetByteUnchecked(offset));
			}

			Verify(continuationBytesCount <= length);
			return length - continuationBytesCount;
		}

		/// <summary>
		/// Gets the substring starting at {@code codePointStart} and extending for
		/// {@code codePointLength} code points.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static Slice Substring(Slice utf8, int codePointStart, int codePointLength)
		{
			Condition.CheckArgument(codePointStart >= 0, "codePointStart is negative");
            Condition.CheckArgument(codePointLength >= 0, "codePointLength is negative");

			int indexStart = OffsetOfCodePoint(utf8, codePointStart);
			if (indexStart < 0)
			{
				throw new System.ArgumentException("UTF-8 does not contain " + codePointStart + " code points");
			}
			if (codePointLength == 0)
			{
				return Slices.EmptySlice;
			}
			int indexEnd = OffsetOfCodePoint(utf8, indexStart, codePointLength - 1);
			if (indexEnd < 0)
			{
				throw new System.ArgumentException("UTF-8 does not contain " + (codePointStart + codePointLength) + " code points");
			}
			indexEnd += LengthOfCodePoint(utf8, indexEnd);
			if (indexEnd > utf8.Length)
			{
				throw new ArgumentException("UTF-8 is not well formed");
			}
			return utf8.ToSlice(indexStart, indexEnd - indexStart);
		}

		/// <summary>
		/// Reverses the slice code point by code point.
		/// <para>
		/// Note: Invalid UTF-8 sequences are copied directly to the output.
		/// </para>
		/// </summary>
		public static Slice Reverse(Slice utf8)
		{
			int length = utf8.Length;
			Slice reverse = Slices.Allocate(length);

			int forwardPosition = 0;
			int reversePosition = length;
			while (forwardPosition < length)
			{
				int codePointLength = LengthOfCodePointSafe(utf8, forwardPosition);

				// backup the reverse pointer
				reversePosition -= codePointLength;
				if (reversePosition < 0)
				{
					// this should not happen
					throw new ArgumentException("UTF-8 is not well formed");
				}
				// copy the character
				CopyUtf8SequenceUnsafe(utf8, forwardPosition, reverse, reversePosition, codePointLength);

				forwardPosition += codePointLength;
			}
			return reverse;
		}

		/// <summary>
		/// Compares to UTF-8 sequences using UTF-16 big endian semantics.  This is
		/// equivalent to the <seealso cref="string.compareTo(object)"/>.
		/// {@code java.lang.String}.
		/// </summary>
		/// <exception cref="ArgumentException"> if the UTF-8 are invalid </exception>
		public static int CompareUtf16Be(Slice utf8Left, Slice utf8Right)
		{
			int leftLength = utf8Left.Length;
			int rightLength = utf8Right.Length;

			int offset = 0;
			while (offset < leftLength)
			{
				// if there are no more right code points, right is less
				if (offset >= rightLength)
				{
					return 1; // left.compare(right) > 0
				}

				int leftCodePoint = TryGetCodePointAt(utf8Left, offset);
				if (leftCodePoint < 0)
				{
					throw new ArgumentException("Invalid UTF-8 sequence in utf8Left at " + offset);
				}

				int rightCodePoint = TryGetCodePointAt(utf8Right, offset);
				if (rightCodePoint < 0)
				{
					throw new ArgumentException("Invalid UTF-8 sequence in utf8Right at " + offset);
				}

				int result = CompareUtf16Be(leftCodePoint, rightCodePoint);
				if (result != 0)
				{
					return result;
				}

				// the code points are the same and non-canonical sequences are not allowed,
				// so we advance a single offset through both sequences
				offset += LengthOfCodePoint(leftCodePoint);
			}

			// there are no more left code points, so if there are more right code points,
			// left is less
			if (offset < rightLength)
			{
				return -1; // left.compare(right) < 0
			}

			return 0;
		}

		internal static int CompareUtf16Be(int leftCodePoint, int rightCodePoint)
		{
			if (leftCodePoint < MinSupplementaryCodePoint)
			{
				if (rightCodePoint < MinHighSurrogateCodePoint)
				{
					return leftCodePoint.CompareTo(rightCodePoint);
				}
				else
				{
					// left simple, right complex
					return leftCodePoint < MinSupplementaryCodePoint ? -1 : 1;
				}
			}
			else
			{
				if (rightCodePoint >= MinSupplementaryCodePoint)
				{
					return leftCodePoint.CompareTo(rightCodePoint);
				}
				else
				{
					// left complex, right simple
					return rightCodePoint < MinHighSurrogateCodePoint ? 1 : -1;
				}
			}
		}

		/// <summary>
		/// Converts slice to upper case code point by code point.  This method does
		/// not perform perform locale-sensitive, context-sensitive, or one-to-many
		/// mappings required for some languages.  Specifically, this will return
		/// incorrect results for Lithuanian, Turkish, and Azeri.
		/// <para>
		/// Note: Invalid UTF-8 sequences are copied directly to the output.
		/// </para>
		/// </summary>
		public static Slice ToUpperCase(Slice utf8)
		{
			return TranslateCodePoints(utf8, UpperCodePoints);
		}

		/// <summary>
		/// Converts slice to lower case code point by code point.  This method does
		/// not perform perform locale-sensitive, context-sensitive, or one-to-many
		/// mappings required for some languages.  Specifically, this will return
		/// incorrect results for Lithuanian, Turkish, and Azeri.
		/// <para>
		/// Note: Invalid UTF-8 sequences are copied directly to the output.
		/// </para>
		/// </summary>
		public static Slice ToLowerCase(Slice utf8)
		{
			return TranslateCodePoints(utf8, LowerCodePoints);
		}

		private static Slice TranslateCodePoints(Slice utf8, int[] codePointTranslationMap)
		{
			int length = utf8.Length;
			Slice newUtf8 = Slices.Allocate(length);

			int position = 0;
			int upperPosition = 0;
			while (position < length)
			{
				int codePoint = TryGetCodePointAt(utf8, position);
				if (codePoint >= 0)
				{
					int upperCodePoint = codePointTranslationMap[codePoint];

					// grow slice if necessary
					int nextUpperPosition = upperPosition + LengthOfCodePoint(upperCodePoint);
					if (nextUpperPosition > length)
					{
						newUtf8 = Slices.EnsureSize(newUtf8, nextUpperPosition);
					}

					// write new byte
					SetCodePointAt(upperCodePoint, newUtf8, upperPosition);

					position += LengthOfCodePoint(codePoint);
					upperPosition = nextUpperPosition;
				}
				else
				{
					int skipLength = -codePoint;

					// grow slice if necessary
					int nextUpperPosition = upperPosition + skipLength;
					if (nextUpperPosition > length)
					{
						newUtf8 = Slices.EnsureSize(newUtf8, nextUpperPosition);
					}

					CopyUtf8SequenceUnsafe(utf8, position, newUtf8, upperPosition, skipLength);
					position += skipLength;
					upperPosition = nextUpperPosition;
				}
			}
			return newUtf8.ToSlice(0, upperPosition);
		}

		private static void CopyUtf8SequenceUnsafe(Slice source, int sourcePosition, Slice destination, int destinationPosition, int length)
		{
			switch (length)
			{
				case 1:
					destination.SetByteUnchecked(destinationPosition, source.GetByteUnchecked(sourcePosition));
					break;
				case 2:
					destination.setShortUnchecked(destinationPosition, source.GetShortUnchecked(sourcePosition));
					break;
				case 3:
					destination.setShortUnchecked(destinationPosition, source.GetShortUnchecked(sourcePosition));
					destination.setByteUnchecked(destinationPosition + 2, source.GetByteUnchecked(sourcePosition + 2));
					break;
				case 4:
					destination.SetIntUnchecked(destinationPosition, source.GetIntUnchecked(sourcePosition));
					break;
				case 5:
					destination.SetIntUnchecked(destinationPosition, source.GetIntUnchecked(sourcePosition));
					destination.setByteUnchecked(destinationPosition + 4, source.GetByteUnchecked(sourcePosition + 4));
					break;
				case 6:
					destination.SetIntUnchecked(destinationPosition, source.GetIntUnchecked(sourcePosition));
					destination.SetShortUnchecked(destinationPosition + 4, source.GetShortUnchecked(sourcePosition + 4));
					break;
				default:
					throw new System.InvalidOperationException("Invalid code point length " + length);
			}
		}

		/// <summary>
		/// Removes all white space characters from the left side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice LeftTrim(Slice utf8)
		{
			int length = utf8.Length;

			int position = FirstNonWhitespacePosition(utf8);
			return utf8.ToSlice(position, length - position);
		}

		/// <summary>
		/// Removes all {@code whiteSpaceCodePoints} from the left side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice LeftTrim(Slice utf8, int[] whiteSpaceCodePoints)
		{
			int length = utf8.Length;

			int position = FirstNonMatchPosition(utf8, whiteSpaceCodePoints);
			return utf8.ToSlice(position, length - position);
		}

		private static int FirstNonWhitespacePosition(Slice utf8)
		{
			int length = utf8.Length;

			int position = 0;
			while (position < length)
			{
				int codePoint = TryGetCodePointAt(utf8, position);
				if (codePoint < 0)
				{
					break;
				}
				if (!WhitespaceCodePoints[codePoint])
				{
					break;
				}
				position += LengthOfCodePoint(codePoint);
			}
			return position;
		}

		// This function is an exact duplicate of firstNonWhitespacePosition(Slice) except for one line.
		private static int FirstNonMatchPosition(Slice utf8, int[] codePointsToMatch)
		{
			int length = utf8.Length;

			int position = 0;
			while (position < length)
			{
				int codePoint = TryGetCodePointAt(utf8, position);
				if (codePoint < 0)
				{
					break;
				}
				if (!Matches(codePoint, codePointsToMatch))
				{
					break;
				}
				position += LengthOfCodePoint(codePoint);
			}
			return position;
		}

		private static bool Matches(int codePoint, int[] codePoints)
		{
			foreach (int codePointToTrim in codePoints)
			{
				if (codePoint == codePointToTrim)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Removes all white space characters from the right side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice RightTrim(Slice utf8)
		{
			int position = LastNonWhitespacePosition(utf8, 0);
			return utf8.ToSlice(0, position);
		}

		/// <summary>
		/// Removes all white {@code whiteSpaceCodePoints} from the right side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice RightTrim(Slice utf8, int[] whiteSpaceCodePoints)
		{
			int position = LastNonMatchPosition(utf8, 0, whiteSpaceCodePoints);
			return utf8.ToSlice(0, position);
		}

		private static int LastNonWhitespacePosition(Slice utf8, int minPosition)
		{
			int position = utf8.Length;
			while (minPosition < position)
			{
				// decode the code point before position if possible
				int codePoint;
				int codePointLength;
				sbyte unsignedByte = utf8.GetByte(position - 1);
				if (!IsContinuationByte(unsignedByte))
				{
					codePoint = unsignedByte & 0xFF;
					codePointLength = 1;
				}
				else if (minPosition <= position - 2 && !IsContinuationByte(utf8.GetByte(position - 2)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 2);
					codePointLength = 2;
				}
				else if (minPosition <= position - 3 && !IsContinuationByte(utf8.GetByte(position - 3)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 3);
					codePointLength = 3;
				}
				else if (minPosition <= position - 4 && !IsContinuationByte(utf8.GetByte(position - 4)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 4);
					codePointLength = 4;
				}
				else
				{
					break;
				}
				if (codePoint < 0 || codePointLength != LengthOfCodePoint(codePoint))
				{
					break;
				}
				if (!WhitespaceCodePoints[codePoint])
				{
					break;
				}
				position -= codePointLength;
			}
			return position;
		}

		// This function is an exact duplicate of lastNonWhitespacePosition(Slice, int) except for one line.
		private static int LastNonMatchPosition(Slice utf8, int minPosition, int[] codePointsToMatch)
		{
			int position = utf8.Length;
			while (position > minPosition)
			{
				// decode the code point before position if possible
				int codePoint;
				int codePointLength;
				sbyte unsignedByte = utf8.GetByte(position - 1);
				if (!IsContinuationByte(unsignedByte))
				{
					codePoint = unsignedByte & 0xFF;
					codePointLength = 1;
				}
				else if (minPosition <= position - 2 && !IsContinuationByte(utf8.GetByte(position - 2)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 2);
					codePointLength = 2;
				}
				else if (minPosition <= position - 3 && !IsContinuationByte(utf8.GetByte(position - 3)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 3);
					codePointLength = 3;
				}
				else if (minPosition <= position - 4 && !IsContinuationByte(utf8.GetByte(position - 4)))
				{
					codePoint = TryGetCodePointAt(utf8, position - 4);
					codePointLength = 4;
				}
				else
				{
					break;
				}
				if (codePoint < 0 || codePointLength != LengthOfCodePoint(codePoint))
				{
					break;
				}
				if (!Matches(codePoint, codePointsToMatch))
				{
					break;
				}
				position -= codePointLength;
			}
			return position;
		}

		/// <summary>
		/// Removes all white space characters from the left and right side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice Trim(Slice utf8)
		{
			int start = FirstNonWhitespacePosition(utf8);
			int end = LastNonWhitespacePosition(utf8, start);
			return utf8.ToSlice(start, end - start);
		}

		/// <summary>
		/// Removes all white {@code whiteSpaceCodePoints} from the left and right side of the string.
		/// <para>
		/// Note: Invalid UTF-8 sequences are not trimmed.
		/// </para>
		/// </summary>
		public static Slice Trim(Slice utf8, int[] whiteSpaceCodePoints)
		{
			int start = FirstNonMatchPosition(utf8, whiteSpaceCodePoints);
			int end = LastNonMatchPosition(utf8, start, whiteSpaceCodePoints);
			return utf8.ToSlice(start, end - start);
		}

		public static Slice FixInvalidUtf8(Slice slice)
		{
			return FixInvalidUtf8(slice, ReplacementCodePoint);
		}

		public static Slice FixInvalidUtf8(Slice slice, int? replacementCodePoint)
		{
			if (IsAscii(slice))
			{
				return slice;
			}

			int replacementCodePointValue = -1;
			int replacementCodePointLength = 0;
			if (replacementCodePoint.HasValue)
			{
				replacementCodePointValue = replacementCodePoint.Value;
				replacementCodePointLength = LengthOfCodePoint(replacementCodePointValue);
			}

			int length = slice.Length;
			Slice utf8 = Slices.Allocate(length);

			int dataPosition = 0;
			int utf8Position = 0;
			while (dataPosition < length)
			{
				int codePoint = TryGetCodePointAt(slice, dataPosition);
				int codePointLength;
				if (codePoint >= 0)
				{
					codePointLength = LengthOfCodePoint(codePoint);
					dataPosition += codePointLength;
				}
				else
				{
					// negative number carries the number of invalid bytes
					dataPosition += (-codePoint);
					if (replacementCodePointValue < 0)
					{
						continue;
					}
					codePoint = replacementCodePointValue;
					codePointLength = replacementCodePointLength;
				}
				utf8 = Slices.EnsureSize(utf8, utf8Position + codePointLength);
				utf8Position += SetCodePointAt(codePoint, utf8, utf8Position);
			}
			return utf8.ToSlice(0, utf8Position);
		}

		/// <summary>
		/// Tries to get the UTF-8 encoded code point at the {@code position}.  A positive
		/// return value means the UTF-8 sequence at the position is valid, and the result
		/// is the code point.  A negative return value means the UTF-8 sequence at the
		/// position is invalid, and the length of the invalid sequence is the absolute
		/// value of the result.
		/// </summary>
		/// <returns> the code point or negative the number of bytes in the invalid UTF-8 sequence. </returns>
		public static int TryGetCodePointAt(Slice utf8, int position)
		{
			//
			// Process first byte
			sbyte firstByte = utf8.GetByte(position);

			int length = LengthOfCodePointFromStartByteSafe(firstByte);
			if (length < 0)
			{
				return length;
			}

			if (length == 1)
			{
				// normal ASCII
				// 0xxx_xxxx
				return firstByte;
			}

			//
			// Process second byte
			if (position + 1 >= utf8.Length)
			{
				return -1;
			}

			sbyte secondByte = utf8.GetByteUnchecked(position + 1);
			if (!IsContinuationByte(secondByte))
			{
				return -1;
			}

			if (length == 2)
			{
				// 110x_xxxx 10xx_xxxx
				int codePoint = ((firstByte & 0b0001_1111) << 6) | (secondByte & 0b0011_1111);
				// fail if overlong encoding
				return codePoint < 0x80 ? -2 : codePoint;
			}

			//
			// Process third byte
			if (position + 2 >= utf8.Length)
			{
				return -2;
			}

			sbyte thirdByte = utf8.GetByteUnchecked(position + 2);
			if (!IsContinuationByte(thirdByte))
			{
				return -2;
			}

			if (length == 3)
			{
				// 1110_xxxx 10xx_xxxx 10xx_xxxx
				int codePoint = ((firstByte & 0b0000_1111) << 12) | ((secondByte & 0b0011_1111) << 6) | (thirdByte & 0b0011_1111);

				// surrogates are invalid
				if (MinSurrogate <= codePoint && codePoint <= MaxSurrogate)
				{
					return -3;
				}
				// fail if overlong encoding
				return codePoint < 0x800 ? -3 : codePoint;
			}

			//
			// Process forth byte
			if (position + 3 >= utf8.Length)
			{
				return -3;
			}

			sbyte forthByte = utf8.GetByteUnchecked(position + 3);
			if (!IsContinuationByte(forthByte))
			{
				return -3;
			}

			if (length == 4)
			{
				// 1111_0xxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				int codePoint = ((firstByte & 0b0000_0111) << 18) | ((secondByte & 0b0011_1111) << 12) | ((thirdByte & 0b0011_1111) << 6) | (forthByte & 0b0011_1111);
				// fail if overlong encoding or above upper bound of Unicode
				if (codePoint < 0x11_0000 && codePoint >= 0x1_0000)
				{
					return codePoint;
				}
				return -4;
			}

			//
			// Process fifth byte
			if (position + 4 >= utf8.Length)
			{
				return -4;
			}

			sbyte fifthByte = utf8.GetByteUnchecked(position + 4);
			if (!IsContinuationByte(fifthByte))
			{
				return -4;
			}

			if (length == 5)
			{
				// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
				return -5;
			}

			//
			// Process sixth byte
			if (position + 5 >= utf8.Length)
			{
				return -5;
			}

			sbyte sixthByte = utf8.GetByteUnchecked(position + 5);
			if (!IsContinuationByte(sixthByte))
			{
				return -5;
			}

			if (length == 6)
			{
				// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
				return -6;
			}

			// for longer sequence, which can't happen
			return -1;
		}

		internal static int LengthOfCodePointFromStartByteSafe(sbyte startByte)
		{
			int unsignedStartByte = startByte & 0xFF;
			if (unsignedStartByte < 0b1000_0000)
			{
				// normal ASCII
				// 0xxx_xxxx
				return 1;
			}
			if (unsignedStartByte < 0b1100_0000)
			{
				// illegal bytes
				// 10xx_xxxx
				return -1;
			}
			if (unsignedStartByte < 0b1110_0000)
			{
				// 110x_xxxx 10xx_xxxx
				return 2;
			}
			if (unsignedStartByte < 0b1111_0000)
			{
				// 1110_xxxx 10xx_xxxx 10xx_xxxx
				return 3;
			}
			if (unsignedStartByte < 0b1111_1000)
			{
				// 1111_0xxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				return 4;
			}
			if (unsignedStartByte < 0b1111_1100)
			{
				// 1111_10xx 10xx_xxxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				return 5;
			}
			if (unsignedStartByte < 0b1111_1110)
			{
				// 1111_110x 10xx_xxxx 10xx_xxxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				return 6;
			}
			return -1;
		}

		/// <summary>
		/// Finds the index of the first byte of the code point at a position, or
		/// {@code -1} if the position is not within the slice.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int OffsetOfCodePoint(Slice utf8, int codePointCount)
		{
			return OffsetOfCodePoint(utf8, 0, codePointCount);
		}

		/// <summary>
		/// Starting from {@code position} bytes in {@code utf8}, finds the
		/// index of the first byte of the code point {@code codePointCount}
		/// in the slice.  If the slice does not contain
		/// {@code codePointCount} code points after {@code position}, {@code -1}
		/// is returned.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int OffsetOfCodePoint(Slice utf8, int position, int codePointCount)
		{
			ParameterCondition.CheckPositionIndex(position, utf8.Length);
            ParameterCondition.CheckArgument(codePointCount >= 0, "codePointPosition is negative");

			// Quick exit if we are sure that the position is after the end
			if (utf8.Length - position <= codePointCount)
			{
				return -1;
			}
			if (codePointCount == 0)
			{
				return position;
			}

			int correctIndex = codePointCount + position;
			// Length rounded to 8 bytes
			int length8 = utf8.Length & 0x7FFF_FFF8;
			// While we have enough bytes left and we need at least 8 characters process 8 bytes at once
			while (position < length8 && correctIndex >= position + 8)
			{
				// Count bytes which are NOT the start of a code point
				correctIndex += CountContinuationBytes(utf8.GetLongUnchecked(position));

				position += 8;
			}
			// Length rounded to 4 bytes
			int length4 = utf8.Length & 0x7FFF_FFFC;
			// While we have enough bytes left and we need at least 4 characters process 4 bytes at once
			while (position < length4 && correctIndex >= position + 4)
			{
				// Count bytes which are NOT the start of a code point
				correctIndex += CountContinuationBytes(utf8.GetIntUnchecked(position));

				position += 4;
			}
			// Do the rest one by one, always check the last byte to find the end of the code point
			while (position < utf8.Length)
			{
				// Count bytes which are NOT the start of a code point
				correctIndex += CountContinuationBytes(utf8.GetByteUnchecked(position));
				if (position == correctIndex)
				{
					break;
				}

				position++;
			}

			if (position == correctIndex && correctIndex < utf8.Length)
			{
				return correctIndex;
			}
			return -1;
		}

		/// <summary>
		/// Gets the UTF-8 sequence length of the code point at {@code position}.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int LengthOfCodePoint(Slice utf8, int position)
		{
			return LengthOfCodePointFromStartByte(utf8.GetByte(position));
		}

		/// <summary>
		/// Gets the UTF-8 sequence length of the code point at {@code position}.
		/// <para>
		/// Truncated UTF-8 sequences, 5 and 6 byte sequences, and invalid code points
		/// are handled by this method without throwing an exception.
		/// </para>
		/// </summary>
		public static int LengthOfCodePointSafe(Slice utf8, int position)
		{
			int length = LengthOfCodePointFromStartByteSafe(utf8.GetByte(position));
			if (length < 0)
			{
				return -length;
			}

			if (length == 1 || position + 1 >= utf8.Length || !IsContinuationByte(utf8.GetByteUnchecked(position + 1)))
			{
				return 1;
			}

			if (length == 2 || position + 2 >= utf8.Length || !IsContinuationByte(utf8.GetByteUnchecked(position + 2)))
			{
				return 2;
			}

			if (length == 3 || position + 3 >= utf8.Length || !IsContinuationByte(utf8.GetByteUnchecked(position + 3)))
			{
				return 3;
			}

			if (length == 4 || position + 4 >= utf8.Length || !IsContinuationByte(utf8.GetByteUnchecked(position + 4)))
			{
				return 4;
			}

			if (length == 5 || position + 5 >= utf8.Length || !IsContinuationByte(utf8.GetByteUnchecked(position + 5)))
			{
				return 5;
			}

			if (length == 6)
			{
				return 6;
			}

			return 1;
		}

		/// <summary>
		/// Gets the UTF-8 sequence length of the code point.
		/// </summary>
		/// <exception cref="InvalidCodePointException"> if code point is not within a valid range </exception>
		public static int LengthOfCodePoint(int codePoint)
		{
			if (codePoint < 0)
			{
				throw new ArgumentException(codePoint.ToString());
			}
			if (codePoint < 0x80)
			{
				// normal ASCII
				// 0xxx_xxxx
				return 1;
			}
			if (codePoint < 0x800)
			{
				return 2;
			}
			if (codePoint < 0x1_0000)
			{
				return 3;
			}
			if (codePoint < 0x11_0000)
			{
				return 4;
			}
			// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
            throw new ArgumentException(codePoint.ToString());
		}

		/// <summary>
		/// Gets the UTF-8 sequence length using the sequence start byte.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int LengthOfCodePointFromStartByte(sbyte startByte)
		{
			int unsignedStartByte = startByte & 0xFF;
			if (unsignedStartByte < 0x80)
			{
				// normal ASCII
				// 0xxx_xxxx
				return 1;
			}
			if (unsignedStartByte < 0xc0)
			{
				// illegal bytes
				// 10xx_xxxx
				throw new ArgumentException("Illegal start 0x" + unsignedStartByte.ToString("X4").ToUpper() + " of code point");
			}
			if (unsignedStartByte < 0xe0)
			{
				// 110x_xxxx 10xx_xxxx
				return 2;
			}
			if (unsignedStartByte < 0xf0)
			{
				// 1110_xxxx 10xx_xxxx 10xx_xxxx
				return 3;
			}
			if (unsignedStartByte < 0xf8)
			{
				// 1111_0xxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				return 4;
			}
			// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
			throw new ArgumentException("Illegal start 0x" + unsignedStartByte.ToString("X4").ToUpper() + " of code point");
		}

		/// <summary>
		/// Gets the UTF-8 encoded code point at the {@code position}.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int GetCodePointAt(Slice utf8, int position)
		{
			int unsignedStartByte = utf8.GetByte(position) & 0xFF;
			if (unsignedStartByte < 0x80)
			{
				// normal ASCII
				// 0xxx_xxxx
				return unsignedStartByte;
			}
			if (unsignedStartByte < 0xc0)
			{
				// illegal bytes
				// 10xx_xxxx
				throw new ArgumentException("Illegal start 0x" + unsignedStartByte.ToString("X4").ToUpper() + " of code point");
			}
			if (unsignedStartByte < 0xe0)
			{
				// 110x_xxxx 10xx_xxxx
				if (position + 1 >= utf8.Length)
				{
					throw new ArgumentException("UTF-8 sequence truncated");
				}
				return ((unsignedStartByte & 0b0001_1111) << 6) | (utf8.GetByte(position + 1) & 0b0011_1111);
			}
			if (unsignedStartByte < 0xf0)
			{
				// 1110_xxxx 10xx_xxxx 10xx_xxxx
				if (position + 2 >= utf8.Length)
				{
					throw new ArgumentException("UTF-8 sequence truncated");
				}
				return ((unsignedStartByte & 0b0000_1111) << 12) | ((utf8.GetByteUnchecked(position + 1) & 0b0011_1111) << 6) | (utf8.GetByteUnchecked(position + 2) & 0b0011_1111);
			}
			if (unsignedStartByte < 0xf8)
			{
				// 1111_0xxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				if (position + 3 >= utf8.Length)
				{
					throw new ArgumentException("UTF-8 sequence truncated");
				}
				return ((unsignedStartByte & 0b0000_0111) << 18) | ((utf8.GetByteUnchecked(position + 1) & 0b0011_1111) << 12) | ((utf8.GetByteUnchecked(position + 2) & 0b0011_1111) << 6) | (utf8.GetByteUnchecked(position + 3) & 0b0011_1111);
			}
			// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
			throw new ArgumentException("Illegal start 0x" + unsignedStartByte.ToString("X4").ToUpper() + " of code point");
		}

		/// <summary>
		/// Gets the UTF-8 encoded code point before the {@code position}.
		/// <para>
		/// Note: This method does not explicitly check for valid UTF-8, and may
		/// return incorrect results or throw an exception for invalid UTF-8.
		/// </para>
		/// </summary>
		public static int GetCodePointBefore(Slice utf8, int position)
		{
			sbyte unsignedByte = utf8.GetByte(position - 1);
			if (!IsContinuationByte(unsignedByte))
			{
				return unsignedByte & 0xFF;
			}
			if (!IsContinuationByte(utf8.GetByte(position - 2)))
			{
				return GetCodePointAt(utf8, position - 2);
			}
			if (!IsContinuationByte(utf8.GetByte(position - 3)))
			{
				return GetCodePointAt(utf8, position - 3);
			}
			if (!IsContinuationByte(utf8.GetByte(position - 4)))
			{
				return GetCodePointAt(utf8, position - 4);
			}

			// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
			throw new ArgumentException("UTF-8 is not well formed");
		}

		private static bool IsContinuationByte(sbyte b)
		{
			return (b & 0b1100_0000) == 0b1000_0000;
		}

		/// <summary>
		/// Convert the code point to UTF-8.
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="InvalidCodePointException"> if code point is not within a valid range </exception>
		public static Slice CodePointToUtf8(int codePoint)
		{
			Slice utf8 = Slices.Allocate(LengthOfCodePoint(codePoint));
			SetCodePointAt(codePoint, utf8, 0);
			return utf8;
		}

		/// <summary>
		/// Sets the UTF-8 sequence for code point at the {@code position}.
		/// </summary>
		/// <exception cref="InvalidCodePointException"> if code point is not within a valid range </exception>
		public static int SetCodePointAt(int codePoint, Slice utf8, int position)
		{
			if (codePoint < 0)
			{
				throw new ArgumentException(codePoint.ToString());
			}
			if (codePoint < 0x80)
			{
				// normal ASCII
				// 0xxx_xxxx
				utf8.SetByte(position, codePoint);
				return 1;
			}
			if (codePoint < 0x800)
			{
				// 110x_xxxx 10xx_xxxx
				utf8.SetByte(position, 0b1100_0000 | ((int)((uint)codePoint >> 6)));
				utf8.SetByte(position + 1, 0b1000_0000 | (codePoint & 0b0011_1111));
				return 2;
			}
			if (MinSurrogate <= codePoint && codePoint <= MaxSurrogate)
			{
				throw new ArgumentException(codePoint.ToString());
			}
			if (codePoint < 0x1_0000)
			{
				// 1110_xxxx 10xx_xxxx 10xx_xxxx
				utf8.SetByte(position, 0b1110_0000 | (((int)((uint)codePoint >> 12)) & 0b0000_1111));
				utf8.SetByte(position + 1, 0b1000_0000 | (((int)((uint)codePoint >> 6)) & 0b0011_1111));
				utf8.SetByte(position + 2, 0b1000_0000 | (codePoint & 0b0011_1111));
				return 3;
			}
			if (codePoint < 0x11_0000)
			{
				// 1111_0xxx 10xx_xxxx 10xx_xxxx 10xx_xxxx
				utf8.SetByte(position, 0b1111_0000 | (((int)((uint)codePoint >> 18)) & 0b0000_0111));
				utf8.SetByte(position + 1, 0b1000_0000 | (((int)((uint)codePoint >> 12)) & 0b0011_1111));
				utf8.SetByte(position + 2, 0b1000_0000 | (((int)((uint)codePoint >> 6)) & 0b0011_1111));
				utf8.SetByte(position + 3, 0b1000_0000 | (codePoint & 0b0011_1111));
				return 4;
			}
			// Per RFC3629, UTF-8 is limited to 4 bytes, so more bytes are illegal
			throw new ArgumentException(codePoint.ToString());
		}

		private static int CountContinuationBytes(sbyte i8)
		{
			// see below
			int value = i8 & 0xff;
			return ((int)((uint)value >> 7)) & ((int)((uint)~value >> 6));
		}

		private static int CountContinuationBytes(int i32)
		{
			// see below
			i32 = ((int)((uint)(i32 & TopMask32) >> 1)) & (~i32);
			return BitCount(i32);
		}

		private static int CountContinuationBytes(long i64)
		{
			// Count the number of bytes that match 0b10xx_xxxx as follows:
			// 1. Mask off the 8th bit of every byte and shift it into the 7th position.
			// 2. Then invert the bytes, which turns the 0 in the 7th bit to a one.
			// 3. And together the results of step 1 and 2, giving us a one in the 7th
			//    position if the byte matched.
			// 4. Count the number of bits in the result, which is the number of bytes
			//    that matched.
			i64 = ((long)((ulong)(i64 & TopMask64) >> 1)) & (~i64);
			return BitCount(i64);
		}
        private static int BitCount(Int32 num)
        {
            int bitCount = 0;

            while (num > 0)
            {
                bitCount += num & 1;
                num = num / 2;
            }

            return bitCount;
        }
        private static int BitCount(long num)
        {
            long bitCount = 0;

            while (num > 0)
            {
                bitCount += num & 1;
                num = num / 2;
            }

            return (int)bitCount;
        }
	}

}