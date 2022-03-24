using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class PipeType : IEntityType
	{
		public PositionAndDirection[] ConnectorPlacements;

		public Mesh Model;

		public string Name;
	}
}
