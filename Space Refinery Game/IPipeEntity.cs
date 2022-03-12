using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public interface IPipeEntity : IGameEntity, IConstruction
	{
		public enum PipeEnd
		{
			A,
			B
		}

		public Volume PipeVolume { get; }

		public static abstract FixedDecimalInt4 AEndPipeDiameter { get; }
		public static abstract FixedDecimalInt4 BEndPipeDiameter { get; }

		public IConstruction AEnd { get; }
		public IConstruction BEnd { get; }

		public void PipeTransferEnter(PipeEnd pipeEnd, Volume contents);
	}
}
