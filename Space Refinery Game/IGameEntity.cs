using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public interface IGameEntity
	{
		public Transform Transform { get; }

		public void Tick();
	}
}
