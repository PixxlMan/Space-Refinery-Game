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

        public static Vector2FixedDecimalInt4 PreviousMousePosition;

        public static Vector2FixedDecimalInt4 MouseDelta => MousePosition - PreviousMousePosition;

        private static InputSnapshot inputSnapshot;
        public static InputSnapshot FrameSnapshot { get => inputSnapshot is null ? new BogusInputSnapshot() : inputSnapshot; }

        public static bool IgnoreNextFrameMousePosition;

        private static bool ignoredMousePositonLastFrame;

        private static object SyncRoot = new();

        public static bool CaptureKeyDown(Key key)
		{
			lock (SyncRoot)
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
        }

        public static bool GetKey(Key key)
		{
			lock (SyncRoot)
			{
                return _currentlyPressedKeys.Contains(key);
			}
        }

        public static bool GetKeyDown(Key key)
        {
			lock (SyncRoot)
			{
                return _newKeysThisFrame.Contains(key);
			}
        }

        public static bool GetMouseButton(MouseButton button)
        {
			lock (SyncRoot)
			{
                return _currentlyPressedMouseButtons.Contains(button);
			}
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            lock (SyncRoot)
            {
                return _newMouseButtonsThisFrame.Contains(button);
            }
        }

        public static void UpdateFrameInput(InputSnapshot snapshot)
        {
            lock (SyncRoot)
            {
                inputSnapshot = snapshot;
                _newKeysThisFrame.Clear();
                _newMouseButtonsThisFrame.Clear();

                if (!IgnoreNextFrameMousePosition)
                {
                    PreviousMousePosition = MousePosition;
                    MousePosition = snapshot.MousePosition.ToFixed<Vector2FixedDecimalInt4>();

                    if (ignoredMousePositonLastFrame)
                    {
                        PreviousMousePosition = MousePosition; // ensure MouseDelta is 0
                        ignoredMousePositonLastFrame = false;
                    }
                }
                else
                {
                    PreviousMousePosition = Vector2FixedDecimalInt4.Zero;
                    MousePosition = Vector2FixedDecimalInt4.Zero;
                    ignoredMousePositonLastFrame = true;
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
		}

		private static void MouseUp(MouseButton mouseButton)
        {
            lock (SyncRoot)
            {
                _currentlyPressedMouseButtons.Remove(mouseButton);
                _newMouseButtonsThisFrame.Remove(mouseButton);
            }
        }

        private static void MouseDown(MouseButton mouseButton)
        {
            lock (SyncRoot)
            {
                if (_currentlyPressedMouseButtons.Add(mouseButton))
                {
                    _newMouseButtonsThisFrame.Add(mouseButton);
                }
            }
        }

        private static void KeyUp(Key key)
        {
            lock (SyncRoot)
            {
                _currentlyPressedKeys.Remove(key);
                _newKeysThisFrame.Remove(key);
            }
        }

        private static void KeyDown(Key key)
        {
            lock (SyncRoot)
            {
                if (_currentlyPressedKeys.Add(key))
                {
                    _newKeysThisFrame.Add(key);
                }
            }
        }
    }
}
