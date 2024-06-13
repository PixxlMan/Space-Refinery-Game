using System;
using System.Diagnostics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using FixedPrecision;

namespace Space_Refinery_Game_Renderer;

public class Window : IDisposable
{
	/// <summary>
	/// Must synchronize access with lock (SdlWindow).
	/// </summary>
	public readonly Sdl2Window SdlWindow;

	private GraphicsDevice graphicsDevice;
	private ResourceFactory factory;

	public GraphicsDevice GraphicsDevice { get => graphicsDevice; private set => graphicsDevice = value; }
	public ResourceFactory Factory { get => factory; private set => factory = value; }

	private bool _windowResized = true;

	public event Action WindowDestroyed;
	public event Action Resized;

	private bool isSetUp = false;
	public bool IsSetUp { get { lock (syncRoot) return isSetUp; } set { lock (syncRoot) isSetUp = value; } }

	private object syncRoot = new();

	public uint Width
	{
		get
		{
			lock (SdlWindow)
				return (uint)SdlWindow.Width;
		}
		set
		{
			lock (SdlWindow)
				SdlWindow.Width = (int)value;
		}
	}

	public uint Height
	{
		get
		{
			lock (SdlWindow)
				return (uint)SdlWindow.Height;
		}
		set
		{
			lock (SdlWindow)
			{
				SdlWindow.Height = (int)value;
			}
		}
	}

	public bool Exists => SdlWindow.Exists;

	public bool CaptureMouse
	{
		get
		{
			lock (syncRoot)
				return captureMouse;
		}

		set
		{
			lock (syncRoot)
			{
				if (IsSetUp)
				{
					if (value)
					{
						lock (SdlWindow)
						{
							Sdl2Native.SDL_SetRelativeMouseMode(true);
							Sdl2Native.SDL_CaptureMouse(true);
						}
					}
					else
					{
						lock (SdlWindow)
						{
							Sdl2Native.SDL_SetRelativeMouseMode(false);
							Sdl2Native.SDL_CaptureMouse(false);
						}
					}
				}

				captureMouse = value;
			}
		}
	}

	private bool captureMouse;

	public Window(string title, int width = 1280, int height = 720, int x = 100, int y = 100)
	{
		SdlWindow = new(title, x, y, width, height, SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown | SDL_WindowFlags.MouseCapture, true);

		SdlWindow.Resized += () =>
		{
			_windowResized = true;
		};
	}

	public void SetUp(GraphicsDevice graphicsDevice, ResourceFactory factory)
	{
		GraphicsDevice = graphicsDevice;
		Factory = factory;

		IsSetUp = true;
	}

	public Swapchain CreateSwapchain(PixelFormat depthFormat = PixelFormat.R16_UNorm)
	{
		if (!IsSetUp)
		{
			throw new InvalidOperationException("SetUp must be called before CreateSwapchain.");
		}

		SwapchainDescription swapchainDescription = new(
				VeldridStartup.GetSwapchainSource(SdlWindow),
				Width,
				Height,
				depthFormat,
				false
				);

		return Factory.CreateSwapchain(swapchainDescription);
	}

	public void Open()
	{
		lock (syncRoot)
		{
			if (!IsSetUp)
			{
				throw new InvalidOperationException("SetUp must be called before Open.");
			}

			if (CaptureMouse)
			{
				lock (SdlWindow)
				{
					Sdl2Native.SDL_SetRelativeMouseMode(true);
					Sdl2Native.SDL_CaptureMouse(true);
				}
			}
		}
	}

	public void Close()
	{
		Dispose();
	}

	public void PumpEvents()
	{
		lock (syncRoot)
		{
			if (_windowResized)
			{
				_windowResized = false;

				Resized?.Invoke();
			}

			try
			{
				lock (SdlWindow)
				{
					SdlWindow.PumpEvents();
				}
			}
			catch (InvalidOperationException)
			{
				Console.WriteLine("Uh oh. Invalid yabayaba.");
			}
		}
	}

	public void Dispose()
	{
		lock (syncRoot)
		{
			lock (SdlWindow)
			{
				SdlWindow.Close();
			}
			WindowDestroyed?.Invoke();
		}
	}
}
