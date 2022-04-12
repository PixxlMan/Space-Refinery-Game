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

		public string ModelPath;

		[NonSerialized]
		public Mesh Mesh;

		public string Name;

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
					Name = "Straight Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeStraight.obj"),
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
					Name = "90 Bend Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeBend90.obj"),
				},
				new PipeType()
				{
					ConnectorPlacements = new PositionAndDirection[]
					{
						new PositionAndDirection()
						{
							Position = new(0, (FixedDecimalInt4).5f, 0),
							Direction = new(-1, 0, 0),
						},
						new PositionAndDirection()
						{
							Position = new(0, (FixedDecimalInt4).5f, 0),
							Direction = new(-1, 0, 0),
						},
					},
					Name = "180 Bend Pipe",
					ModelPath = Path.Combine("Assets", "Models", "Pipe", "PipeBend180.obj"),
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
