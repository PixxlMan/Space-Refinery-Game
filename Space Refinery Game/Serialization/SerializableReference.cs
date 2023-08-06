using System.Diagnostics;

namespace Space_Refinery_Game
{
	public readonly struct SerializableReference : IEquatable<SerializableReference>
	{
		public readonly Guid ReferenceGuid;

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Never use a GUID as the name of a reference. If you do, things might break and I will be sad (namely parsing, which cannot differentiate between a GUID-name and an actual GUID and will parse it as a GUID reference).
		/// </remarks>
		public readonly string ReferenceName;

		public SerializableReference(Guid referenceGuid)
		{
			ReferenceGuid = referenceGuid;
			ReferenceName = null;
		}

		public SerializableReference(string referenceName)
		{
			ReferenceGuid = Guid.Empty;
			ReferenceName = referenceName;
		}

		public static readonly SerializableReference Empty = new();

		/// <summary>
		/// Returns whether the <c>SerializableReference</c> contains no reference identifiers.
		/// </summary>
		public readonly bool IsEmpty => ReferenceGuid == Guid.Empty && ReferenceName is null;

		/// <summary>
		/// Returns whether the <c>SerializableReference</c> only has one type of reference identifier and also is not empty.
		/// Does not check whether name or guid is duplicate or if a <c>SerializableReferenceHandler</c> contains a reference to it.
		/// </summary>
		public readonly bool IsValid => !IsEmpty && ReferenceGuid == Guid.Empty || ReferenceName is null;

		/// <summary>
		/// Returns whether this <c>SerializableReference</c> contains a guid reference.
		/// </summary>
		public readonly bool HasGuid => ReferenceGuid != Guid.Empty;

		/// <summary>
		/// Returns whether this <c>SerializableReference</c> contains a name reference.
		/// </summary>
		public readonly bool HasName => ReferenceName is not null;

		/// <summary>
		/// Generates a new <c>SerializableReference</c> based on a unique GUID.
		/// </summary>
		public static SerializableReference NewReference()
		{
			return new(Guid.NewGuid());
		}

		public override readonly string ToString()
		{
			if (HasGuid)
			{
				return ReferenceGuid.ToString();
			}

			return ReferenceName;
		}

		public static SerializableReference ParseString(string input)
		{
			if (Guid.TryParse(input, out Guid guid))
			{
				return new(guid);
			}
			else
			{
				return new(input);
			}
		}

		public readonly bool Equals(SerializableReference other)
		{
			return ReferenceGuid == other.ReferenceGuid && ReferenceName == other.ReferenceName;
		}

		public readonly override int GetHashCode()
		{
			return HashCode.Combine(ReferenceGuid, ReferenceName);
		}

		public static bool operator ==(SerializableReference left, SerializableReference right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SerializableReference left, SerializableReference right)
		{
			return !(left == right);
		}

		public static implicit operator SerializableReference(Guid guid)
		{
			return new(guid);
		}

		public static implicit operator SerializableReference(string name)
		{
			return new(name);
		}

		public static explicit operator Guid(SerializableReference serializableReference)
		{
			Debug.Assert(serializableReference.IsValid, "An invalid SerializableReference cannot safely be used and should never have been created!");
			Debug.Assert(serializableReference.HasGuid, "An attempt to access the guid of a SerializableReference which does not contain a guid has been made.");

			return serializableReference.ReferenceGuid;
		}

		public static explicit operator string(SerializableReference serializableReference)
		{
			Debug.Assert(serializableReference.IsValid, "An invalid SerializableReference cannot safely be used and should never have been created!");
			Debug.Assert(serializableReference.HasName, "An attempt to access the guid of a SerializableReference which does not contain a guid has been made.");

			return serializableReference.ReferenceName;
		}

		public readonly override bool Equals(object obj)
		{
			return obj is SerializableReference && Equals((SerializableReference)obj);
		}
	}
}
