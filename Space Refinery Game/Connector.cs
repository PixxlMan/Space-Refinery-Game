﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public abstract class Connector : Entity
	{
		public abstract IInformationProvider InformationProvider { get; }
	}
}