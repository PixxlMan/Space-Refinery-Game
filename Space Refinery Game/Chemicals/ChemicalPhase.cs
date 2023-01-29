using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public enum ChemicalPhase // Ordered by entropy of the phase. The larger the number the greater the entropy.
	{
		Solid = 0,
		Liquid = 1,
		Gas = 2,
		Plasma = 3
	}
}
