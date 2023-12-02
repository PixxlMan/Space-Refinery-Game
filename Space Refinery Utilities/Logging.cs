using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Space_Refinery_Utilities;

/// <remarks>
/// This class is entirely thread safe.
/// </remarks>
public static class Logging
{
	private static object syncRoot = new();

	private static int scopeDepth = 0;

	private const int scopeIndentation = 4;

	private const int minimumIndentation = 20;

	private static int extraSpace = 0;

	private const int spaceMargin = 1;

	private readonly static Stopwatch stopwatch = new();

	public static void StartTime()
	{
		stopwatch.Start();
		Log($"Logging began at {DateTime.UtcNow} UTC");
	}

	public enum LogType
	{
		Error,
		Warning,
		Simulation,
		Log,
		Debug,
	}

	[DebuggerHidden]
	private static void PreFormat(LogType logType)
	{
		string timeStamp = $"@{stopwatch.Elapsed} s:";

		switch (logType)
		{
			case LogType.Error:
				Console.Write($"[ERROR]{timeStamp}");
				break;
			case LogType.Warning:
				Console.Write($"[WARN]{timeStamp}");
				break;
			case LogType.Simulation:
				timeStamp = $"{stopwatch.Elapsed} s & {Time.CurrentTickTime} tt & {Time.TicksElapsed} ticks";
				Console.Write($"[SIMUL]{timeStamp}");
				break;
			case LogType.Log:
				Console.Write($"[LOG]{timeStamp}");
				break;
			case LogType.Debug:
				// Debug doesn't call PreFormat.
				break;
			default:
				Console.Write($"[MISC]{timeStamp}");
				break;
		}

		const int longestLogTag = 7;
		if (timeStamp.Length + longestLogTag + spaceMargin >= minimumIndentation + extraSpace)
		{
			extraSpace = timeStamp.Length + longestLogTag + spaceMargin - minimumIndentation;
		}

		Console.SetCursorPosition(minimumIndentation + extraSpace + scopeIndentation * scopeDepth, Console.GetCursorPosition().Top);
	}

	// also log current thread's name?
	// perhaps not to console, but seems sensible for text file log
	[DebuggerHidden]
	public static void Log(string logText)
	{
		lock (syncRoot)
		{
			PreFormat(LogType.Log);
			Console.WriteLine($"{logText}");
		}
	}
	
	[DebuggerHidden]
	public static void LogIf(bool condition, string logText)
	{
		if (condition)
		{
			lock (syncRoot)
			{
				PreFormat(LogType.Log);
				Console.WriteLine($"{logText}");
			}
		}
	}

	[DebuggerHidden]
	public static void LogSimulation(string logText)
	{
		lock (syncRoot)
		{
			PreFormat(LogType.Simulation);
			Console.WriteLine($"{logText}");
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
			LogColor($"{logText}", ConsoleColor.Red, LogType.Error);
		}
	}

	[DebuggerHidden]
	public static void LogWarning(string logText)
	{
		lock (syncRoot)
		{
			LogColor($"{logText}", ConsoleColor.Yellow, LogType.Warning);
		}
	}

	[DebuggerHidden]
	public static void LogColor(string logText, ConsoleColor color, LogType logType)
	{
		lock (syncRoot)
		{
			PreFormat(logType);

			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(logText);
			Console.ForegroundColor = originalColor;
		}
	}

	[DebuggerHidden]
	public static void LogScopeStart(string scopeName)
	{
		lock (syncRoot)
		{
			Log($"{scopeName}:");
			Log("{");

			scopeDepth++;
		}
	}

	[DebuggerHidden]
	public static void LogScopeEnd()
	{
		lock (syncRoot)
		{
			scopeDepth--;

			Log("}");
		}
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
	public static void LogAll<T>(IEnumerable<T> enumerable, string logText, Func<T, string> stringLogFunc)
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
