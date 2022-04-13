﻿using FixedPrecision;
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

		public static Transform GenerateTransformForConnector(PositionAndDirection chosenConnectorTransform, PipeConnector connector, FixedDecimalInt4 rotation)
		{
			QuaternionFixedDecimalInt4 connectorRotation = /*connector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(connector.Transform.Rotation) :*/ connector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			ITransformable pipeConnectorTransformable = new Transform(connector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Direction : chosenConnectorTransform.Direction;

			Vector3FixedDecimalInt4 position = connector.VacantSide == ConnectorSide.A ? -chosenConnectorTransform.Position : chosenConnectorTransform.Position;

			QuaternionFixedDecimalInt4 orientation = QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.Concatenate(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransformable.LocalUnitZ, -pipeConnectorTransformable.LocalUnitY), QuaternionFixedDecimalInt4.CreateFromAxisAngle(direction, rotation)));

			orientation = QuaternionFixedDecimalInt4.Normalize(orientation);

			Transform transform =
				new(
					connector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(Vector3FixedDecimalInt4.Transform(direction, orientation), connector.VacantSide == ConnectorSide.A ? -Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitZ, orientation) : Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitZ, orientation), connector.VacantSide == ConnectorSide.A ? -Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitY, orientation) : Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitY, orientation)))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(Vector3FixedDecimalInt4.Transform(direction, orientation), -Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitZ, orientation), -Vector3FixedDecimalInt4.Transform(Vector3FixedDecimalInt4.UnitY, orientation)))
				);

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			return transform;
		}
	}
}
