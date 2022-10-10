﻿using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using FixedPrecision;
using static FixedPrecision.Convenience;
using FXRenderer;

namespace Space_Refinery_Game
{
	public static class InputTracker
	{
		private static HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
		private static HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

		private static HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
		private static HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

		public static Vector2FixedDecimalInt4 MousePosition;

		public static Vector2FixedDecimalInt4 PreviousFrameMousePosition;

		public static Vector2FixedDecimalInt4 MouseDelta => MousePosition - PreviousFrameMousePosition;

		public static DecimalNumber ScrollWheelDelta;

		private static object SyncRoot = new();

		public static bool IgnoreNextFrameMousePosition;

		private static bool ignoredMousePositonLastFrame;

		public static void ListenToWindow(Window window)
		{
			lock (SyncRoot)
			{
				window.SdlWindow.KeyDown += (ke) => KeyDown(ke.Key);
				window.SdlWindow.KeyUp += (ke) => KeyUp(ke.Key);
				window.SdlWindow.MouseDown += (me) => MouseDown(me.MouseButton);
				window.SdlWindow.MouseUp += (me) => MouseUp(me.MouseButton);
				window.SdlWindow.MouseMove += (me) => MouseMove(me.MousePosition.ToFixed<Vector2FixedDecimalInt4>());
				window.SdlWindow.MouseWheel += (me) => MouseWheel(me.WheelDelta.ToFixed<DecimalNumber>());
			}
		}

		private static void MouseWheel(DecimalNumber wheelDelta)
		{
			lock (SyncRoot)
			{
				ScrollWheelDelta += wheelDelta;
			}
		}

		private static void MouseMove(Vector2FixedDecimalInt4 mousePosition)
		{
			lock (SyncRoot)
			{
				if (!IgnoreNextFrameMousePosition)
				{
					MousePosition = mousePosition;
				}
			}
		}

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

		public static void UpdateInputFrame()
		{
			lock (SyncRoot)
			{
				_newKeysThisFrame.Clear();
				_newMouseButtonsThisFrame.Clear();
				ScrollWheelDelta = 0;

				if (IgnoreNextFrameMousePosition)
				{
					PreviousFrameMousePosition = Vector2FixedDecimalInt4.Zero;
					MousePosition = Vector2FixedDecimalInt4.Zero;
					ignoredMousePositonLastFrame = true;
				}
				else
				{
					PreviousFrameMousePosition = MousePosition;

					if (ignoredMousePositonLastFrame)
					{
						PreviousFrameMousePosition = MousePosition; // ensure MouseDelta is 0
						ignoredMousePositonLastFrame = false;
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
