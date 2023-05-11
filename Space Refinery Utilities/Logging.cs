using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

public static class Logging
{
	public static void Log(string logText)
	{
		Console.WriteLine(logText);
	}

	public static void LogError(string logText)
	{
		var originalColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(logText);
		Console.ForegroundColor = originalColor;
	}
}
