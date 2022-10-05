﻿using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game_Renderer
{
	public class MeshLoader
	{
		private Dictionary<string, Mesh> meshCache = new();

		private GraphicsWorld graphicsWorld;

		public MeshLoader(GraphicsWorld graphicsWorld)
		{
			this.graphicsWorld = graphicsWorld;
		}

		public Mesh LoadCached(string path)
		{
			path = Path.GetFullPath(path);

			if (meshCache.ContainsKey(path))
			{
				return meshCache[path];
			}

			var mesh = Mesh.LoadMesh(graphicsWorld.GraphicsDevice, graphicsWorld.Factory, path);

			meshCache.Add(path, mesh);

			return mesh;
		}
	}
}
