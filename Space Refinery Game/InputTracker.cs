using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using FixedPrecision;
using static FixedPrecision.Convenience;

namespace Space_Refinery_Game
{
    public static class InputTracker
    {
        private static HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private static HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

        private static HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private static HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

        public static Vector2FixedDecimalInt4 MousePosition;

        private static InputSnapshot inputSnapshot;
        public static InputSnapshot FrameSnapshot { get => inputSnapshot is null ? new BogusInputSnapshot() : inputSnapshot; private set => inputSnapshot = value; }

        public static bool IgnoreNextFrameMousePosition;

        public static bool CaptureKeyDown(Key key)
		{
			if (_newKeysThisFrame.Contains(key))
			{
                _newKeysThisFrame.Remove(key);

                return true;
			}
			else
			{
                return false;
			}
        }

        public static bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public static bool GetKeyDown(Key key)
        {
            return _newKeysThisFrame.Contains(key);
        }

        public static bool GetMouseButton(MouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return _newMouseButtonsThisFrame.Contains(button);
        }

        public static void UpdateFrameInput(InputSnapshot snapshot)
        {
            FrameSnapshot = snapshot;
            _newKeysThisFrame.Clear();
            _newMouseButtonsThisFrame.Clear();

			if (!IgnoreNextFrameMousePosition)
			{
                MousePosition = snapshot.MousePosition.ToFixed<Vector2FixedDecimalInt4>();
			}
			else
			{
                MousePosition = Vector2FixedDecimalInt4.Zero;
			}

            for (int i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                KeyEvent ke = snapshot.KeyEvents[i];
                if (ke.Down)
                {
                    KeyDown(ke.Key);
                }
                else
                {
                    KeyUp(ke.Key);
                }
            }

            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    MouseDown(me.MouseButton);
                }
                else
                {
                    MouseUp(me.MouseButton);
                }
            }

            IgnoreNextFrameMousePosition = false;
        }

        private static void MouseUp(MouseButton mouseButton)
        {
            _currentlyPressedMouseButtons.Remove(mouseButton);
            _newMouseButtonsThisFrame.Remove(mouseButton);
        }

        private static void MouseDown(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton))
            {
                _newMouseButtonsThisFrame.Add(mouseButton);
            }
        }

        private static void KeyUp(Key key)
        {
            _currentlyPressedKeys.Remove(key);
            _newKeysThisFrame.Remove(key);
        }

        private static void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _newKeysThisFrame.Add(key);
            }
        }
    }
}
