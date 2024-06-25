using System.Numerics;
using FixedPrecision;

namespace Space_Refinery_Game.Renderer;

// TODO: thread safety?
public sealed class Camera
{
	private FixedDecimalInt4 _fov = 1;
	private FixedDecimalInt4 _near = (FixedDecimalInt4)0.1;
	private FixedDecimalInt4 _far = 1000;

	private Matrix4x4FixedDecimalInt4 _viewMatrix;
	private Matrix4x4FixedDecimalInt4 _projectionMatrix;

	private Transform transform = Transform.Identity;
	public ref Transform Transform { get { lock (syncRoot) return ref transform; } }

	private Perspective perspective;
	public Perspective Perspective { get { lock (syncRoot) return perspective; } set { lock (syncRoot) perspective = value; } }

	private FixedDecimalInt4 _windowWidth;
	private FixedDecimalInt4 _windowHeight;

	public event Action<Matrix4x4FixedDecimalInt4>? ProjectionChanged;
	public event Action<Matrix4x4FixedDecimalInt4>? ViewChanged;

	public Camera(FixedDecimalInt4 width, FixedDecimalInt4 height, Perspective perspective)
	{
		this.perspective = perspective;
		_windowWidth = width;
		_windowHeight = height;
	}

	public Matrix4x4FixedDecimalInt4 ViewMatrix { get { lock (syncRoot) return _viewMatrix; } private set { lock (syncRoot) _viewMatrix = value; } }
	public Matrix4x4FixedDecimalInt4 ProjectionMatrix { get { lock (syncRoot) return _projectionMatrix; } private set { lock (syncRoot) _projectionMatrix = value; }  }

	public FixedDecimalInt4 FieldOfView { get { lock (syncRoot) return _fov; } set { lock (syncRoot) _fov = value; } }

	private FixedDecimalInt4 orthoWidth;
	public FixedDecimalInt4 OrthoWidth { get { lock (syncRoot) return orthoWidth; } set { lock (syncRoot) orthoWidth = value; } }
	private FixedDecimalInt4 orthoHeight;
	public FixedDecimalInt4 OrthoHeight { get { lock (syncRoot) return orthoHeight; } set { lock (syncRoot) orthoHeight = value; } }


	public FixedDecimalInt4 FarDistance { get { lock (syncRoot) return _far; } set { lock (syncRoot) _far = value; } }
	public FixedDecimalInt4 NearDistance { get { lock (syncRoot) return _near; } set { lock (syncRoot) _near = value; } }

	public FixedDecimalInt4 AspectRatio => _windowWidth / _windowHeight;

	public Vector3FixedDecimalInt4 Forward { get { lock (syncRoot) return -Transform.LocalUnitZ; } }

	private object syncRoot = new();

	public void WindowResized(FixedDecimalInt4 width, FixedDecimalInt4 height)
	{
		lock (syncRoot)
		{
			_windowWidth = width;
			_windowHeight = height;
		}

		UpdatePerspectiveMatrix();
	}

	public void UpdatePerspectiveMatrix()
	{
		if (Perspective == Perspective.Perspective)
		{
			ProjectionMatrix = Matrix4x4FixedDecimalInt4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
		}
		else
		{
			ProjectionMatrix = Matrix4x4FixedDecimalInt4.CreateOrthographic(OrthoWidth, OrthoHeight, _near, _far);
		}

		ProjectionChanged?.Invoke(_projectionMatrix);
	}

	public void UpdateViewMatrix()
	{
		ViewMatrix = Matrix4x4FixedDecimalInt4.CreateLookAt(Transform.Position, Transform.Position + Forward, Vector3FixedDecimalInt4.UnitY);
		ViewChanged?.Invoke(_viewMatrix);
	}

	/// <summary>
	/// Projects a point from world space to screen space in pixel coordinates. Values are clamped to [0,<c>screenSize</c>]. If the point is located at the camera's exact position then the output will the center of the screen.
	/// </summary>
	/// <remarks>
	/// Only supports cameras with Perspecive perspective so far.
	/// </remarks>
	/// <param name="worldPoint">The position in world space to project.</param>
	/// <param name="screenSize">The size of the screen, in pixels.</param>
	/// <param name="visible">Returns whether the point is visible to the camera or not, and consequently whether the output has been clipped or not.</param>
	/// <returns>The projected position in screen space.</returns>
	public Vector2FixedDecimalInt4 WorldPointToScreenPoint(Vector3FixedDecimalInt4 worldPoint, Vector2FixedDecimalInt4 screenSize, out bool visible)
	{
		if (perspective != Perspective.Perspective)
		{
			// How much actual change would really be necessary to support orthographic projection here?
			// The matrices would change - but that shouldn't be a problem. Perhaps there would be no problem?
			throw new NotImplementedException("Haven't bothered to implement this for orthographic projection yet!");
		}

		// The screen point is undefined - return the center of the screen!
		if (worldPoint == transform.Position)
		{
			visible = false; // Visibility is false because it must be within the near clip!
							 // TODO: (But maybe this should be reconsidered? Maybe far and near clip should be ignored in this method and in IsVisible etc because this will likely be used by UI systems where near and far clip don't matter or could obstruct?)
			return screenSize / 2;
		}

		Matrix4x4 MVP = Matrix4x4.Multiply(_viewMatrix.ToMatrix4x4(), _projectionMatrix.ToMatrix4x4());

		Vector4 homogenousWorldPosition = new Vector4(worldPoint.X.ToFloat(), worldPoint.Y.ToFloat(), worldPoint.Z.ToFloat(), 1);

		var clipPosition = Vector4.Transform(homogenousWorldPosition, MVP);

		if (Math.Abs(clipPosition.W) < double.Epsilon) // Compare to epsilon instead of zero, because we are using floating point (yuck) Vector4 here.
		{ // Normally we shouldn't ever get here - division by a zero W should be ruled out earlier in the worldPoint == transform.Position check ((I think)!?) - but since we're dealing with rounding stuff we might still end up at zero.
			visible = false;
			return screenSize / 2;
		}

		// Convert the clip space position to normalized device coordinates (NDC) [-1,1].
		Vector3FixedDecimalInt4 ndcPosition = new Vector3FixedDecimalInt4(
			clipPosition.X / clipPosition.W,
			clipPosition.Y / clipPosition.W,
			clipPosition.Z / clipPosition.W
		);

		visible = IsVisibleWithinViewFrustum(ndcPosition/*, NearDistance, FarDistance*/);

		if (!visible)
		{
			// The NDC to screen position conversion below gets wonky if we don't have NDC range numbers to feed it, with overflows etc
			// Therefore we clamp the values to allowed ranges here. That way you can still tell where off-screen something might be, but without borking stuff. I hope.

			ndcPosition.X = FixedDecimalInt4.Clamp(ndcPosition.X, -1, 1);
			ndcPosition.Y = FixedDecimalInt4.Clamp(ndcPosition.Y, -1, 1);
			ndcPosition.Z = FixedDecimalInt4.Clamp(ndcPosition.Z, -1, 1);
		}


		Vector2FixedDecimalInt4 screenPosition = new Vector2FixedDecimalInt4(
			(ndcPosition.X + 1.0) * 0.5 * screenSize.X,
			(1.0 - ndcPosition.Y) * 0.5 * screenSize.Y
		);

		return screenPosition;

		static bool IsVisibleWithinViewFrustum(Vector3FixedDecimalInt4 ndcClipPosition/*, FixedDecimalInt4 nearClip, FixedDecimalInt4 farClip*/)
		{
			return
				FixedDecimalInt4.Abs(ndcClipPosition.X) <= 1 && FixedDecimalInt4.Abs(ndcClipPosition.Y) <= 1 && // Are X and Y positions within NDC ranges [-1,1]? If outside, we know it's outside the view frustum and thus not visible
				ndcClipPosition.Z > 0;/* && // Is the point behind the camera (Z < 0)? If so we know it isn't visible
				ndcClipPosition.Z < farClip && ndcClipPosition.Z > nearClip;*/ // Is the point within the near and far clips? If not, we know it isn't visible.
		}
	}
}

public enum Perspective
{
	Orthographic,
	Perspective
}
