using System.Numerics;
using Veldrid;

namespace Space_Refinery_Game
{
	internal struct BogusInputSnapshot : InputSnapshot
	{
		public IReadOnlyList<KeyEvent> KeyEvents => new List<KeyEvent>();

		public IReadOnlyList<MouseEvent> MouseEvents => new List<MouseEvent>();

		public IReadOnlyList<char> KeyCharPresses => new List<char>();

		public Vector2 MousePosition => new Vector2();

		public float WheelDelta => 0;

		public bool IsMouseDown(MouseButton button)
		{
			return false;
		}
	}
}
