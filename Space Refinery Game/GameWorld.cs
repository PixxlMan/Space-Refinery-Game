using FixedPrecision;
using FXRenderer;
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

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, PipeConnector connector)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			ITransformable pipeConnectorTransformable = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransformable.LocalUnitZ : pipeConnectorTransformable.LocalUnitZ, connector.VacantSide == ConnectorSide.A ? -pipeConnectorTransformable.LocalUnitY : pipeConnectorTransformable.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransformable.LocalUnitZ, -pipeConnectorTransformable.LocalUnitY))
				);			

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			return transform;
		}
	}
}
