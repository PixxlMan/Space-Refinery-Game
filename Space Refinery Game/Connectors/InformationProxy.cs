using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public sealed class InformationProxy : Entity
	{
		public Entity ProxiedEntity;

		public PhysicsObject PhysicsObject;

		public InformationProxy(Connector connector)
		{
			ProxiedEntity = connector;
		}

		public void Enable()
		{
			PhysicsObject.Enabled = true;
		}

		public void Disable()
		{
			PhysicsObject.Enabled = false;
		}

		public void Tick() { }

		public void Interacted() { }

		public void Destroyed() { }

		public IInformationProvider InformationProvider => ((Entity)ProxiedEntity).InformationProvider;
	}
}
