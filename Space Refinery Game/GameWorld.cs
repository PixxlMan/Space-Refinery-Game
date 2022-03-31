using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class GameWorld
	{
		public HashSet<IConstruction> Constructions = new();

		public void AddConstruction(IConstruction construction)
		{
			Constructions.Add(construction);
		}

		public void Deconstruct(IConstruction construction)
		{
			Constructions.Remove(construction);

			construction.Deconstruct();
		}
	}
}
