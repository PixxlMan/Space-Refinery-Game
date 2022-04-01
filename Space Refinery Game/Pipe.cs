﻿using BepuPhysics.Collidables;
using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game
{
	public class Pipe : Entity, IConstruction
	{
		public PhysicsWorld PhysicsWorld;

		public PhysicsObject PhysicsObject;

		public Transform Transform { get; set; }

		public GraphicsWorld GraphicsWorld;

		public EntityRenderable Renderable;

		public PipeConnector[] Connectors;

		private IInformationProvider informationProvider;

		public IInformationProvider InformationProvider => informationProvider;

		public PipeType PipeType;

		private Pipe(Transform transform)
		{
			informationProvider = new PipeInformationProvider(this);

			Transform = transform;
		}

		public void AddDebugObjects()
		{
			MainGame.DebugRender.DrawOrientationMarks(Transform);
		}

		public static Pipe Create(PipeType pipeType, Transform transform, PhysicsWorld physWorld, GraphicsWorld graphWorld)
		{
			Pipe pipe = new(transform);

			MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

			EntityRenderable renderable = CreateRenderable(pipeType, graphWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physWorld, transform, pipe);

			PipeConnector[] connectors = CreateConnectors(pipeType, pipe, physWorld);

			pipe.SetUp(physWorld, physObj, connectors, graphWorld, renderable);

			return pipe;
		}

		private static EntityRenderable CreateRenderable(PipeType pipeType, GraphicsWorld graphWorld, Transform transform)
		{
			EntityRenderable renderable = EntityRenderable.Create(graphWorld.GraphicsDevice, graphWorld.Factory, transform, pipeType.Model, Utils.GetSolidColoredTexture(RgbaByte.Green, graphWorld.GraphicsDevice, graphWorld.Factory), graphWorld.CameraProjViewBuffer, graphWorld.LightInfoBuffer);

			graphWorld.AddRenderable(renderable);
			return renderable;
		}

		private static PhysicsObject CreatePhysicsObject(PhysicsWorld physWorld, Transform transform, Pipe pipeStraight)
		{
			PhysicsObjectDescription<Box> physicsObjectDescription = new(new Box(1, .5f, .5f), transform, 0, true);

			PhysicsObject physObj = physWorld.AddPhysicsObject(physicsObjectDescription, pipeStraight);
			return physObj;
		}

		private static PipeConnector[] CreateConnectors(PipeType pipeType, Pipe pipe, PhysicsWorld physWorld)
		{
			PipeConnector[] connectors = new PipeConnector[pipeType.ConnectorPlacements.Length];

			for (int i = 0; i < pipeType.ConnectorPlacements.Length; i++)
			{
				PhysicsObject physicsObject = physWorld.Raycast<PipeConnector>(
					pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation) * 2,
					-Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation),
					.5f);

				MainGame.DebugRender.PersistentRay(
					pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation) * 2,
					-Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation),
					RgbaFloat.Yellow);
				
				if (physicsObject is null || physicsObject.Entity is not PipeConnector)
				{
					PipeConnector connector = new PipeConnector(pipe, ConnectorSide.A);

					Transform transform = new(
						pipe.Transform.Position + Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Position, pipe.Transform.Rotation),
						QuaternionFixedDecimalInt4.CreateLookingAt(Vector3FixedDecimalInt4.Transform(pipeType.ConnectorPlacements[i].Direction, pipe.Transform.Rotation), ((ITransformable)pipe.Transform).LocalUnitZ, ((ITransformable)pipe.Transform).LocalUnitY)
					);

					connector.Transform = transform;

					var physicsObjectDescription = new PhysicsObjectDescription<Box>(new Box(.4f, .4f, .25f), transform, 0, true);

					connector.PhysicsObject = physWorld.AddPhysicsObject(physicsObjectDescription, connector);

					connectors[i] = connector;

					continue;
				}
				else if (physicsObject.Entity is PipeConnector pipeConnector)
				{
					pipeConnector.Connect(pipe);

					connectors[i] = pipeConnector;

					continue;
				}
			}

			return connectors;
		}

		public static IConstruction Build(Connector connector, IEntityType entityType, int indexOfSelectedConnector, FixedDecimalInt4 rotation, PhysicsWorld physicsWorld, GraphicsWorld graphicsWorld)
		{
			PipeConnector pipeConnector = (PipeConnector)connector;

			PipeType pipeType = (PipeType)entityType;

			QuaternionFixedDecimalInt4 connectorRotation = pipeConnector.VacantSide == ConnectorSide.A ? QuaternionFixedDecimalInt4.Inverse(pipeConnector.Transform.Rotation) : pipeConnector.Transform.Rotation;

			connectorRotation = QuaternionFixedDecimalInt4.Normalize(connectorRotation);

			ITransformable pipeConnectorTransformable = new Transform(pipeConnector.Transform) { Rotation = connectorRotation };

			Vector3FixedDecimalInt4 direction = pipeConnector.VacantSide == ConnectorSide.A ? -pipeType.ConnectorPlacements[indexOfSelectedConnector].Direction : pipeType.ConnectorPlacements[indexOfSelectedConnector].Direction;

			Vector3FixedDecimalInt4 position = pipeConnector.VacantSide == ConnectorSide.A ? -pipeType.ConnectorPlacements[indexOfSelectedConnector].Position : pipeType.ConnectorPlacements[indexOfSelectedConnector].Position;

			Transform transform =
				new(
					pipeConnector.Transform.Position + Vector3FixedDecimalInt4.Transform(position, QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, pipeConnectorTransformable.LocalUnitZ, pipeConnectorTransformable.LocalUnitY))),
					QuaternionFixedDecimalInt4.Inverse(QuaternionFixedDecimalInt4.CreateLookingAt(direction, -pipeConnectorTransformable.LocalUnitZ, -pipeConnectorTransformable.LocalUnitY))
				);

			transform.Rotation = QuaternionFixedDecimalInt4.Normalize(transform.Rotation);

			Pipe pipe = new(transform);

			MainGame.DebugRender.AddDebugObjects += pipe.AddDebugObjects;

			EntityRenderable renderable = CreateRenderable(pipeType, graphicsWorld, transform);

			PhysicsObject physObj = CreatePhysicsObject(physicsWorld, transform, pipe);

			var connectors = CreateConnectors(pipeType, pipe, physicsWorld);

			pipe.SetUp(physicsWorld, physObj, connectors, graphicsWorld, renderable);

			return pipe;
		}
		
		private void SetUp(PhysicsWorld physicsWorld, PhysicsObject physicsObject, PipeConnector[] connectors, GraphicsWorld graphicsWorld, EntityRenderable renderable)
		{
			PhysicsWorld = physicsWorld;
			PhysicsObject = physicsObject;
			Connectors = connectors;
			GraphicsWorld = graphicsWorld;
			Renderable = renderable;
		}

		public void Deconstruct()
		{
			PhysicsObject.Destroy();
			Renderable.Destroy();

			MainGame.DebugRender.AddDebugObjects -= AddDebugObjects;

			GraphicsWorld.UnorderedRenderables.Remove(Renderable);

			foreach (var connector in Connectors)
			{
				connector.Disconnect(this);
			}
		}
	}
}
