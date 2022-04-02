using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class ConnectorProxy : Entity
	{
		public Connector Connector;

		public PhysicsObject PhysicsObject;

		public ConnectorProxy(PipeConnector connector)
		{
			Connector = connector;
		}

		public IInformationProvider InformationProvider => ((Entity)Connector).InformationProvider;
	}
}
