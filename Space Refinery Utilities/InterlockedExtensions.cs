namespace Space_Refinery_Utilities;

public static class InterlockedExtensions
{
	public static int InterlockedReadInt(ref int location)
	{
		return Interlocked.Add(ref location, 0);
	}
}
