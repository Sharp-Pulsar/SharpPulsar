
namespace SharpPulsar.Akka.Admin.Api.Models
{
	using System.Collections.Generic;

	/// <summary>
	/// Pulsar Schema compatibility strategy.
	/// </summary>
	public sealed class SchemaCompatibilityStrategy
	{

		/// <summary>
		/// Undefined.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy Undefined = new SchemaCompatibilityStrategy("UNDEFINED", InnerEnum.Undefined);

		/// <summary>
		/// Always incompatible.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy AlwaysIncompatible = new SchemaCompatibilityStrategy("AlwaysIncompatible", InnerEnum.AlwaysIncompatible);

		/// <summary>
		/// Always compatible.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy AlwaysCompatible = new SchemaCompatibilityStrategy("AlwaysCompatible", InnerEnum.AlwaysCompatible);

		/// <summary>
		/// Messages written by an old schema can be read by a new schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy Backward = new SchemaCompatibilityStrategy("BACKWARD", InnerEnum.Backward);

		/// <summary>
		/// Messages written by a new schema can be read by an old schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy Forward = new SchemaCompatibilityStrategy("FORWARD", InnerEnum.Forward);

		/// <summary>
		/// Equivalent to both FORWARD and BACKWARD.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy Full = new SchemaCompatibilityStrategy("FULL", InnerEnum.Full);

		/// <summary>
		/// Be similar to BACKWARD, BACKWARD_TRANSITIVE ensure all previous version schema can
		/// be read by the new schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy BackwardTransitive = new SchemaCompatibilityStrategy("BackwardTransitive", InnerEnum.BackwardTransitive);

		/// <summary>
		/// Be similar to FORWARD, FORWARD_TRANSITIVE ensure new schema can be ready by all previous
		/// version schema.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy ForwardTransitive = new SchemaCompatibilityStrategy("ForwardTransitive", InnerEnum.ForwardTransitive);

		/// <summary>
		/// Equivalent to both FORWARD_TRANSITIVE and BACKWARD_TRANSITIVE.
		/// </summary>
		public static readonly SchemaCompatibilityStrategy FullTransitive = new SchemaCompatibilityStrategy("FullTransitive", InnerEnum.FullTransitive);

		private static readonly IList<SchemaCompatibilityStrategy> ValueList = new List<SchemaCompatibilityStrategy>();

		static SchemaCompatibilityStrategy()
		{
			ValueList.Add(Undefined);
			ValueList.Add(AlwaysIncompatible);
			ValueList.Add(AlwaysCompatible);
			ValueList.Add(Backward);
			ValueList.Add(Forward);
			ValueList.Add(Full);
			ValueList.Add(BackwardTransitive);
			ValueList.Add(ForwardTransitive);
			ValueList.Add(FullTransitive);
		}

		public enum InnerEnum
		{
			Undefined,
			AlwaysIncompatible,
			AlwaysCompatible,
			Backward,
			Forward,
			Full,
			BackwardTransitive,
			ForwardTransitive,
			FullTransitive
		}

		public readonly InnerEnum InnerEnumValue;
		private readonly string _nameValue;
		private readonly int _ordinalValue;
		private static int _nextOrdinal = 0;

		private SchemaCompatibilityStrategy(string name, InnerEnum innerEnum)
		{
			_nameValue = name;
			_ordinalValue = _nextOrdinal++;
			InnerEnumValue = innerEnum;
		}



		public static SchemaCompatibilityStrategy FromAutoUpdatePolicy(SchemaAutoUpdateCompatibilityStrategy strategy)
		{
			if (strategy == null)
			{
				return SchemaCompatibilityStrategy.AlwaysIncompatible;
			}
			switch (strategy)
			{
				case SchemaAutoUpdateCompatibilityStrategy.Backward:
					return Backward;
				case SchemaAutoUpdateCompatibilityStrategy.Forward:
					return Forward;
				case SchemaAutoUpdateCompatibilityStrategy.Full:
					return Full;
				case SchemaAutoUpdateCompatibilityStrategy.AlwaysCompatible:
					return AlwaysCompatible;
				case SchemaAutoUpdateCompatibilityStrategy.ForwardTransitive:
					return ForwardTransitive;
				case SchemaAutoUpdateCompatibilityStrategy.BackwardTransitive:
					return BackwardTransitive;
				case SchemaAutoUpdateCompatibilityStrategy.FullTransitive:
					return FullTransitive;
				case SchemaAutoUpdateCompatibilityStrategy.AutoUpdateDisabled:
				default:
					return AlwaysCompatible;
			}
		}

		public static IList<SchemaCompatibilityStrategy> values()
		{
			return ValueList;
		}

		public int ordinal()
		{
			return _ordinalValue;
		}

		public override string ToString()
		{
			return _nameValue;
		}

		public static SchemaCompatibilityStrategy valueOf(string name)
		{
			foreach (SchemaCompatibilityStrategy enumInstance in SchemaCompatibilityStrategy.ValueList)
			{
				if (enumInstance._nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}
