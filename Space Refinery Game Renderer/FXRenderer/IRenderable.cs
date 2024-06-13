using FixedPrecision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game_Renderer;

public interface IRenderable
{
	public void AddDrawCommands(CommandList commandList, FixedDecimalLong8 deltaTime);
}
