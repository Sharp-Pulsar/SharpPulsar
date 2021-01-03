using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Common.Schema
{
	/// <summary>
	/// Encoding types of supported KeyValueSchema for Pulsar messages.
	/// </summary>
	public enum KeyValueEncodingType
	{
		/// <summary>
		/// Key is stored as message key, while value is stored as message payload.
		/// </summary>
		SEPARATED,

		/// <summary>
		/// Key and value are stored as message payload.
		/// </summary>
		INLINE
	}

}
