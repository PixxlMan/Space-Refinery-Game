using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	internal class InputTrackerCloneSnapshot : InputSnapshot
	{
		public InputTrackerCloneSnapshot(IReadOnlyList<KeyEvent> keyEvents, IReadOnlyList<MouseEvent> mouseEvents, IReadOnlyList<char> keyCharPresses, Vector2 mousePosition, float wheelDelta)
		{
			KeyEvents = keyEvents;
			MouseEvents = mouseEvents;
			KeyCharPresses = keyCharPresses;
			MousePosition = mousePosition;
			WheelDelta = wheelDelta;
		}

		public IReadOnlyList<KeyEvent> KeyEvents { get; }

		public IReadOnlyList<MouseEvent> MouseEvents { get; }

		public IReadOnlyList<char> KeyCharPresses { get; }

		public Vector2 MousePosition { get; }

		public float WheelDelta { get; }

		public bool IsMouseDown(MouseButton button)
		{
			return InputTracker.GetMouseButton(button);
		}
	}
}
