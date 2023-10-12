using System.Diagnostics;

namespace Space_Refinery_Utilities;

/// <remarks>
/// This class is entirely thread safe.
/// </remarks>
public static class Logging
{
	private static object syncRoot = new();

	private static int scopeDepth = 0;

	private const int scopeIndentation = 4;

	[DebuggerHidden]
	public static void Log(string logText)
	{
		lock (syncRoot)
		{
			Indent();
			Console.WriteLine($"[LOG] {logText}");
		}
	}

	[DebuggerHidden]
	public static void LogSimulation(string logText)
	{
		lock (syncRoot)
		{
			Indent();
			Console.WriteLine($"[SIMUL@{Time.TicksElapsed}] {logText}");
		}
	}

	/// <remarks>
	/// This method is ignored in builds without the "IncludeAdditionalDebugLogging" conditional compilation symbol is defined.
	/// <para/>
	/// This method will always print unindented and never wait for other logs to finish.
	/// <para/>
	/// This method is somewhat faster than other log methods, making it ideal for use in performance-sensitive code, such as rendering.
	/// No synchronization is performed manually and no indentations are produced.
	/// <para/>
	/// This method is too cool to lock or indent.
	/// </remarks>
	[Conditional("IncludeAdditionalDebugLogging")]
	[DebuggerHidden]
	// TODO: make this, and maybe all logging methods, faster by avoiding the slow console. Perhaps use a separate thread for that.
	public static void LogDebug(string logText)
	{
		Console.WriteLine($"[DEBUG] {logText}");
	}

	[DebuggerHidden]
	public static void LogError(string logText)
	{
		lock (syncRoot)
		{
			Indent();
			LogColor($"[ERROR@{Time.TicksElapsed}={Time.CurrentTickTime} s] {logText}", ConsoleColor.Red);
		}
	}

	[DebuggerHidden]
	public static void LogWarning(string logText)
	{
		lock (syncRoot)
		{
			Indent();
			LogColor($"[WARN] {logText}", ConsoleColor.Yellow);
		}
	}

	[DebuggerHidden]
	public static void LogColor(string logText, ConsoleColor color)
	{
		lock (syncRoot)
		{
			Indent();
			lock (syncRoot)
			{
				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(logText);
				Console.ForegroundColor = originalColor;
			}
		}
	}

	[DebuggerHidden]
	private static void LogScopeStart(string scopeName)
	{
		lock (syncRoot)
		{
			Log($"{scopeName}: {{");

			scopeDepth++;
		}
	}

	[DebuggerHidden]
	private static void LogScopeEnd()
	{
		lock (syncRoot)
		{
			scopeDepth--;

			Log("}");
		}
	}

	private static void Indent()
	{
		Console.SetCursorPosition(scopeIndentation * scopeDepth, Console.GetCursorPosition().Top);
	}

	[DebuggerHidden]
	public static void LogAll<T>(IEnumerable<T> enumerable, string logText)
	{
		lock (syncRoot)
		{
			LogScopeStart(logText);

			foreach (var item in enumerable)
			{
				Log(item?.ToString() ?? "null");
			}

			LogScopeEnd();
		}
	}

	[DebuggerHidden]
	public static void LogAll<T>(IEnumerable<T> enumerable, Func<T, string> stringLogFunc, string logText)
	{
		lock (syncRoot)
		{
			LogScopeStart(logText);

			foreach (var item in enumerable)
			{
				Log(stringLogFunc(item));
			}

			LogScopeEnd();
		}
	}
}
