using FixedPrecision;
using FXRenderer;
using Space_Refinery_Game_Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Veldrid;

namespace Space_Refinery_Game
{
	[Serializable]
	public class PipeType : IEntityType
	{
		public PositionAndDirection[] ConnectorPlacements;

		public PipeConnectorProperties[] ConnectorProperties;

		public string[] ConnectorNames;

		public string ModelPath;

		[NonSerialized]
		public Mesh Mesh;

		public string Name;

		public PipeProperties PipeProperties;

		public Type TypeOfPipe;

		public void Serialize(string path)
		{
			using FileStream stream = File.OpenWrite(path);

			JsonSerializer.Serialize(stream, this, new JsonSerializerOptions() { IncludeFields = true });
		}

		public static PipeType Deserialize(string path, GraphicsDevice gd, ResourceFactory factory)
		{
			using FileStream stream = File.OpenRead(path);

			var pipeType = JsonSerializer.Deserialize<PipeType>(stream, new JsonSerializerOptions() { IncludeFields = true });

			pipeType.AssignModel(gd, factory);

			return pipeType;
		}

		public void AssignModel(GraphicsDevice gd, ResourceFactory factory)
		{
			Mesh = Mesh.LoadMesh(gd, factory, ModelPath);
		}

		public static PipeType[] GetAllPipeTypes(GraphicsWorld graphicsWorld)
		{
			PipeType[] entityTypes =
			{
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).5f, 0, 0),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).5f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.16,
						Friction = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(OrdinaryPipe),
					Name = "Straight Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeStraight.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).9f, 0, -(FixedDecimalInt4).35),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).9f, 0, (FixedDecimalInt4).35),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).9f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					ConnectorNames = new string[]
					{
						"HydrogenOutput",
						"OxygenOutput",
						"WaterInput",
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.16,
						Friction = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(ElectrolysisMachinery),
					Name = "Electrolysis Machine",
					ModelPath = Path.Combine("Assets", "Models", "Machinery", "MachineryElectrolysis.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).375, 0, 0),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).375, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					ConnectorNames = new string[]
					{
						"A",
						"B",
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.1,
						Friction = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(PumpPipe),
					Name = "Pump",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "Special", "PipeSpecialPump.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).5f, 0, 0),
							Direction = new(1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.08,
						Friction = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(SpaceDockPipe),
					Name = "Space Dock",
					ModelPath = Path.Combine("Assets", "Models", "Machinery", "SpaceDock.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4)0.0125, 0, 0),
							Direction = new(1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = 0,
						Friction = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(OrdinaryPipe),
					Name = "Pipe End",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "Special", "PipeSpecialEnd.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).25f, 0, 0),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).25f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.04,
					},
					TypeOfPipe = typeof(ValvePipe),
					Name = "Valve Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "Special", "PipeSpecialValve.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new(0, -(FixedDecimalInt4).5f, 0),
							Direction = new(0, -1, 0),
						},
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).5f, 0, 0),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).5f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.24,
					},
					TypeOfPipe = typeof(OrdinaryPipe),
					Name = "T Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeStraightDivergeT.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new(0, -(FixedDecimalInt4).5f, 0),
							Direction = new(0, -1, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).5f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.16,
					},
					TypeOfPipe = typeof(OrdinaryPipe),
					Name = "90 Bend Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeBend90.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new(0, -(FixedDecimalInt4).5f, 0),
							Direction = new(0, -1, 0),
						},
						new PositionAndDirection()
						{
							Position = new(0, (FixedDecimalInt4).5f, 0),
							Direction = new(0, 1, 0),
						},
						new PositionAndDirection()
						{
							Position = new((FixedDecimalInt4).5f, 0, 0),
							Direction = new(1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(-(FixedDecimalInt4).5f, 0, 0),
							Direction = new(-1, 0, 0),
						},
					},
					ConnectorProperties = new PipeConnectorProperties[]
					{
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
						new PipeConnectorProperties()
						{
							Shape = PipeShape.Cylindrical,
							ConnectorDiameter = (FixedDecimalInt4).475,
							ConnectorFlowAreaDiameter = (FixedDecimalInt4).425,
						},
					},
					PipeProperties = new()
					{
						FlowableVolume = (FixedDecimalLong8)0.32,
					},
					TypeOfPipe = typeof(OrdinaryPipe),
					Name = "X Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeStraightDivergeX.obj"),
				},
			};

			foreach (PipeType pipeType in entityTypes)
			{
				pipeType.AssignModel(graphicsWorld.GraphicsDevice, graphicsWorld.Factory);
			}

			return entityTypes;
		}
	}
}
