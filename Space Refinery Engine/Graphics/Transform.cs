using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game.Renderer;

public struct Transform : IEquatable<Transform>
{
	private static Transform identity = new(Vector3FixedDecimalInt4.Zero);
	public static Transform Identity => identity;

	public Vector3FixedDecimalInt4 Position;
	public QuaternionFixedDecimalInt4 Rotation;

	public Transform(Vector3FixedDecimalInt4 position = default, QuaternionFixedDecimalInt4? rotation = null)
	{
		if (!rotation.HasValue)
		{
			this.Rotation = QuaternionFixedDecimalInt4.Identity;
		}
		else
		{
			this.Rotation = rotation.Value;
		}

		this.Position = position;
	}

	public Transform(Vector3FixedDecimalInt4 position, QuaternionFixedDecimalInt4 rotation)
	{
		this.Position = position;
		this.Rotation = rotation;
	}

	public Transform(Transform transform)
	{
		Position = transform.Position;
		Rotation = transform.Rotation;
	}

	public static Transform RotateAround(Transform transform, Vector3FixedDecimalInt4 pivotPoint, Vector3FixedDecimalInt4 axis, FixedDecimalInt4 angle) // https://answers.unity.com/questions/1751620/rotating-around-a-pivot-point-using-a-quaternion.html
	{
		QuaternionFixedDecimalInt4 rot = QuaternionFixedDecimalInt4.CreateFromAxisAngle(axis, angle);
		transform.Position = Vector3FixedDecimalInt4.Transform((transform.Position - pivotPoint), rot) + pivotPoint;
		transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(rot, transform.Rotation);

		return transform;
	}

	public static Transform RotateAround(Transform transform, Vector3FixedDecimalInt4 pivotPoint, QuaternionFixedDecimalInt4 rot) // https://answers.unity.com/questions/1751620/rotating-around-a-pivot-point-using-a-quaternion.html
	{
		transform.Position = Vector3FixedDecimalInt4.Transform((transform.Position - pivotPoint), rot) + pivotPoint;
		transform.Rotation = QuaternionFixedDecimalInt4.Concatenate(rot, transform.Rotation);

		return transform;
	}

	public Transform PerformTransform(Transform other)
	{
		QuaternionFixedDecimalInt4 rotation = this.Rotation * other.Rotation;

		Vector3FixedDecimalInt4 position = other.Position + this.Position;

		return new(position, rotation);
	}

	public Transform Invert()
	{
		Transform inverse = default;
		inverse.Rotation = QuaternionFixedDecimalInt4.Conjugate(Rotation);
		inverse.Position = Vector3FixedDecimalInt4.Transform(-Position, inverse.Rotation);

		return inverse;
	}

	public override bool Equals(object? obj)
	{
		return obj is Transform transform && Equals(transform);
	}

	public bool Equals(Transform other)
	{
		return Position.Equals(other.Position) &&
			   Rotation.Equals(other.Rotation);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Position, Rotation);
	}

	public Vector3FixedDecimalInt4 LocalUnitX
	{
		get
		{
			return Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitX, Rotation);
		}
	}

	public Vector3FixedDecimalInt4 LocalUnitY
	{
		get
		{
			return Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitY, Rotation);
		}
	}

	public Vector3FixedDecimalInt4 LocalUnitZ
	{
		get
		{
			return Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitZ, Rotation);
		}
	}

	public static Vector3FixedDecimalInt4 TransformDirectionToOrientation(Transform transform, Vector3FixedDecimalInt4 untransformedDirection)
	{
		return Vector3FixedDecimalInt4.Transform(untransformedDirection, transform.Rotation);
	}

	public void CopyTransform(Transform transform)
	{
		Position = transform.Position;
		Rotation = transform.Rotation;
	}

	public BlittableTransform GetBlittableTransform(Vector3FixedDecimalInt4 origin)
	{
		return new BlittableTransform
		{
			Position = new(Position.ToVector3RelativeToOrigin(origin)),
			Rotation = new(Matrix4x4.CreateFromQuaternion(Rotation.ToQuaternion())), // TODO do via matrixfixed?
		};
	}

	public override string? ToString()
	{
		return $"{{ Position = {Position}, Rotation = {Rotation} }}";
	}

	public static bool operator ==(Transform left, Transform right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Transform left, Transform right)
	{
		return !(left == right);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct BlittableOrientationMatrix
{
public BlittableOrientationMatrix(Matrix4x4 matrix)
{
	M11 = matrix.M11;
	M12 = matrix.M12;
	M13 = matrix.M13;
	//M14 = matrix.M14;

	M21 = matrix.M21;
	M22 = matrix.M22;
	M23 = matrix.M23;
	//M24 = matrix.M24;

	M31 = matrix.M31;
	M32 = matrix.M32;
	M33 = matrix.M33;
	//M34 = matrix.M34;

	/*M41 = matrix.M41;
	M42 = matrix.M42;
	M43 = matrix.M43;
	M44 = matrix.M44;*/
}

public const uint /*Actual*/SizeInBytes = sizeof(float) * 9;

//public const uint SizeInBytes = sizeof(float) * 16; // The size actually practically used when blitted to gpu.

public float M11, M12, M13, /*M14,*/ M21, M22, M23, /*M24,*/ M31, M32, M33/*, /*M34,*/ /*M41, M42, M43, M44*/;
}

[StructLayout(LayoutKind.Sequential)]
public struct BlittableVector3
{
public BlittableVector3(Vector3 vector3)
{
	X = vector3.X;
	Y = vector3.Y;
	Z = vector3.Z;
}

public const uint /*Actual*/SizeInBytes = sizeof(float) * 3;

//public const uint SizeInBytes = sizeof(float) * 4; // The size actually practically used when blitted to gpu.

public float X, Y, Z;
}

[StructLayout(LayoutKind.Sequential)]
public struct BlittableTransform
{
public const uint /*Actual*/SizeInBytes = BlittableVector3./*Actual*/SizeInBytes + BlittableOrientationMatrix./*Actual*/SizeInBytes;

//public const uint SizeInBytes = 32;

public BlittableVector3 Position;

public BlittableOrientationMatrix Rotation;
}