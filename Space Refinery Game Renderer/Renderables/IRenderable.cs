using FixedPrecision;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public interface IRenderable
{
	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime);
}
