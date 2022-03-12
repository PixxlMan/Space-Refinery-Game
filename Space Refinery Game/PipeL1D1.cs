using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class PipeL1D1 : IPipeEntity
	{
		private PipeL1D1()
		{ }

		public static FixedDecimalInt4 AEndPipeDiameter => 1;

		public static FixedDecimalInt4 BEndPipeDiameter => 1;

		public IConstruction AEnd { get; protected set; }

		public IConstruction BEnd { get; protected set; }

		private Volume volume;

		public Volume PipeVolume => volume;

		public Vector3FixedDecimalInt4 ContentVelocity;

		public Vector3FixedDecimalInt4 AEndDirection { get => Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitY, Transform.Rotation); }

		public Transform Transform { get; protected set; }

		public void PipeTransferEnter(IPipeEntity.PipeEnd pipeEnd, Volume contents)
		{
			volume.AddToVolume(contents);
		}

		public void Tick()
		{
			FixedDecimalInt4 AEndExitFlow = Vector3FixedDecimalInt4.Dot(ContentVelocity, AEndDirection) * volume.GetDensity(); //https://answers.unity.com/questions/1351855/how-do-i-get-an-objects-velocity-in-one-direction.html

			if (AEndExitFlow == 0)
			{
				return;
			}
			else if (AEndExitFlow > 0)
			{
				if (AEnd is null)
				{
					// Leak
				}

				((IPipeEntity)AEnd).PipeTransferEnter(IPipeEntity.PipeEnd.B /*placeholder*/, PipeVolume.TakePart(AEndExitFlow));
			}
			else
			{
				if (BEnd is null)
				{
					// Leak
				}

				((IPipeEntity)BEnd).PipeTransferEnter(IPipeEntity.PipeEnd.B, PipeVolume.TakePart(AEndExitFlow * -1));
			}

		}
	}
}
