﻿//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2020 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to some Java HashMap or Hashtable methods.
//---------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
internal static class HashMapHelper
{
	public static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
	{
		HashSet<KeyValuePair<TKey, TValue>> entries = new HashSet<KeyValuePair<TKey, TValue>>();
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
		{
			entries.Add(keyValuePair);
		}
		return entries;
	}

	public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		TValue ret;
		dictionary.TryGetValue(key, out ret);
		return ret;
	}
}