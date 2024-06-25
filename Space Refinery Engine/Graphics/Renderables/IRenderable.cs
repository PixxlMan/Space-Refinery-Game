using FixedPrecision;
using Veldrid;

namespace Space_Refinery_Engine.Renderer;

public interface IRenderable
{
	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime);
}
