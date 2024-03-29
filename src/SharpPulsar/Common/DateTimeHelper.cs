﻿//---------------------------------------------------------------------------------------------------------
//	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
//	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
//---------------------------------------------------------------------------------------------------------
using System;

public static class DateTimeHelper
{
	private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	public static long CurrentUnixTimeMillis()
	{
		return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
	}
    public static long CurrentUnixTimeMillis(DateTime dateTime)
    {
        return (long)(dateTime - Jan1st1970).TotalMilliseconds;
    }
}