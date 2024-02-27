using System.Diagnostics;

namespace Space_Refinery_Utilities;

/// <remarks>
/// This class is entirely thread safe.
/// </remarks>
public static class Logging
{
	private static readonly object syncRoot = new();

	private const int scopeIndentation = 4;

	private readonly static Stopwatch stopwatch = new();

	private static LogLevel loggingFilterLevel;

	/// <summary>
	/// The log level at which to 
	/// </summary>
	private const LogLevel logLevelToProduceThreadLegend = LogLevel.Deep;

	/// <summary>
	/// Used to keep track of which threads (by their ids) have been encountered in the logging system to facilitate printing a legend with the name of the thread the first time it is encountered.
	/// </summary>
	private static HashSet<int> encounteredThreads = new();

	public static void SetUp(LogLevel filterLevel)
	{
		stopwatch.Start();
		loggingFilterLevel = filterLevel;

		LogLegend($"Logging at level '{filterLevel}' begins at {DateTime.UtcNow} UTC", LogLevel.Everything);
	}

	public enum LogType
	{
		/// <summary>
		/// Used for logging errors and problems which occur during execution.
		/// </summary>
		Error,
		/// <summary>
		/// Used for logging warnings or non-fatal problems which occur during execution.
		/// </summary>
		Warning,
		/// <summary>
		/// Used to log things about the state of the simulation, will include current tick in output.
		/// </summary>
		Simulation,
		/// <summary>
		/// A regular ol' log.
		/// </summary>
		Log,
		/// <summary>
		/// The fastest form of log, should be used in hot paths for minimal impact to performance, although with reduced output detail.
		/// </summary>
		Debug,
		/// <summary>
		/// Used for explanatory purposes, for instance to declare starting time and thread id to thread name connections.
		/// </summary>
		Legend,
	}

	public enum LogLevel : int
	{
		None = int.MinValue,
		Everything = int.MaxValue,
		Release = Critical,
		Debug = Everything,

		Critical = 1,
		Basic = 2,
		Deep = 3,
	}

	/// <remarks>
	/// All calls to this method must be synchronized with a lock on syncRoot.
	/// </remarks>
	/// <param name="logType"></param>
	[DebuggerHidden]
	private static void PreFormat(LogType logType)
	{
		if (loggingFilterLevel >= logLevelToProduceThreadLegend)
		{
			if (!encounteredThreads.Contains(Environment.CurrentManagedThreadId))
			{
				encounteredThreads.Add(Environment.CurrentManagedThreadId); // Must add this _before_ calling any other logging method to avoid infinite recursion.

				LogLegend($"Thread '{Thread.CurrentThread.Name}' has id '{Environment.CurrentManagedThreadId}'");
			}
		}

		string timeStamp = $"@{stopwatch.Elapsed}:";

		string formatText = string.Empty;

		switch (logType)
		{
			case LogType.Error:
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[ERROR]{timeStamp}");
				break;
			case LogType.Warning:
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[WARN]{timeStamp}");
				break;
			case LogType.Simulation:
				timeStamp = $"{stopwatch.Elapsed} s & {Time.CurrentTickTime} tt & {Time.TicksElapsed} ticks";
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[SIMUL]{timeStamp}");
				break;
			case LogType.Log:
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[LOG]{timeStamp}");
				break;
			case LogType.Debug:
				// Debug doesn't call PreFormat.
				break;
			case LogType.Legend:
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[LGND]{timeStamp}");
				break;
			default:
				formatText = ($"{{{Environment.CurrentManagedThreadId}}}[MISC]{timeStamp}");
				break;
			}

		Console.Write(formatText);

		int scopeDepth = 0;

		if (scopeTimings.TryGetValue(Environment.CurrentManagedThreadId, out Stack<long>? value))
		{
			scopeDepth = value.Count;
		}

		Console.SetCursorPosition((scopeIndentation * scopeDepth) + (formatText.Length + 1), Console.GetCursorPosition().Top);
	}

	[DebuggerHidden]
	public static void Log(string logText, LogLevel logLevel = LogLevel.Basic)
	{
		if (logLevel > loggingFilterLevel)
		{
			return;
		}

		lock (syncRoot)
		{
			PreFormat(LogType.Log);
			Console.WriteLine($"{logText}");
		}
	}

	[DebuggerHidden]
	public static void LogIf(bool condition, string logText, LogLevel logLevel = LogLevel.Basic)
	{
		if (condition)
		{
			Log(logText, logLevel);
		}
	}

	[DebuggerHidden]
	public static void LogSimulation(string logText, LogLevel logLevel = LogLevel.Basic)
	{
		if (logLevel > loggingFilterLevel)
		{
			return;
		}

		lock (syncRoot)
		{
			PreFormat(LogType.Simulation);
			Console.WriteLine($"{logText}");
		}
	}

	[DebuggerHidden]
	public static void LogLegend(string logText, LogLevel logLevel = LogLevel.Deep)
	{
		if (logLevel > loggingFilterLevel)
		{
			return;
		}

		lock (syncRoot)
		{
			PreFormat(LogType.Legend);
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
	public static void LogError(string logText, LogLevel logLevel = LogLevel.Critical)
	{
		LogColor($"{logText}", ConsoleColor.Red, LogType.Error, logLevel);
	}

	[DebuggerHidden]
	public static void LogWarning(string logText, LogLevel logLevel = LogLevel.Basic)
	{
		LogColor($"{logText}", ConsoleColor.Yellow, LogType.Warning, logLevel);
	}

	[DebuggerHidden]
	public static void LogColor(string logText, ConsoleColor color, LogType logType, LogLevel logLevel = LogLevel.Basic)
	{
		if (logLevel > loggingFilterLevel)
		{
			return;
		}

		lock (syncRoot)
		{
			PreFormat(logType);

			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(logText);
			Console.ForegroundColor = originalColor;
		}
	}

	private static readonly Dictionary<int, Stack<long>> scopeTimings = new();

	[DebuggerHidden]
	public static void LogScopeStart(string scopeName)
	{
		lock (syncRoot)
		{
			Log($"{scopeName}:");
			Log("{");

			if (scopeTimings.ContainsKey(Environment.CurrentManagedThreadId))
			{
				scopeTimings[Environment.CurrentManagedThreadId].Push(stopwatch.ElapsedTicks);
			}
			else
			{
				scopeTimings.Add(Environment.CurrentManagedThreadId, new([stopwatch.ElapsedTicks]));
			}
		}
	}

	[DebuggerHidden]
	public static void LogScopeEnd()
	{
		lock (syncRoot)
		{
			long elapsedTicks = (stopwatch.ElapsedTicks - scopeTimings[Environment.CurrentManagedThreadId].Pop());
			var time = FormatUnit.FormatTime((double)elapsedTicks / Stopwatch.Frequency);
			
			Log($"}} ({time})");
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
	public static void LogAll<T>(IEnumerable<T> enumerable, string logText, Func<T, string> stringLogFunc, LogLevel logLevel = LogLevel.Basic)
	{
		if (logLevel > loggingFilterLevel)
		{
			return;
		}

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
