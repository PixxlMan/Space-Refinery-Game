using System.Numerics;
using System.Runtime.InteropServices;

namespace Space_Refinery_Game_Renderer;

[StructLayout(LayoutKind.Sequential)]
public struct LightInfo
{
	public BlittableVector3 LightDirection;
	private float padding0;
	public BlittableVector3 CameraPosition;
	private float padding1;

	public LightInfo(Vector3 lightDirection, Vector3 cameraPosition)
	{
		LightDirection = new BlittableVector3(lightDirection);
		CameraPosition = new BlittableVector3(cameraPosition);
		padding0 = 0;
		padding1 = 0;
	}
}
