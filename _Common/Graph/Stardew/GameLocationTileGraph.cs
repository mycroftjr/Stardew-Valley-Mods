using Optional;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.CommonModCode.Graph.Stardew
{
	public interface IGameLocationTileGraph
	{
		GameLocation Location { get; }
	}
	
	public class GameLocationTileGraph: IGraph<IntPoint>.WithNodeIndexes<IntPoint>.WithBounds.TwoDimensionalGrid.Rectangular, IGameLocationTileGraph
	{
		public GameLocation Location { get; private set; }
		private readonly int Width;
		private readonly int Height;

		public IntPoint TopLeft
			=> new(0, 0);

		public IntPoint TopRight
			=> new(Width - 1, 0);

		public IntPoint BottomLeft
			=> new(0, Height - 1);

		public IntPoint BottomRight
			=> new(Width - 1, Height - 1);

		public GameLocationTileGraph(GameLocation location)
		{
			this.Location = location;
			Width = location.Map.DisplayWidth / Game1.tileSize;
			Height = location.Map.DisplayHeight / Game1.tileSize;
		}

		public Option<IntPoint> this[IntPoint index]
			=> IsInBounds(index) ? Option.Some(index) : Option.None<IntPoint>();

		public bool IsInBounds(IntPoint index)
			=> index.X >= 0 && index.Y >= 0 && index.X < Width && index.Y < Height;

		public IntPoint GetIndex(IntPoint node)
			=> node;

		public IEnumerable<IntPoint> GetNeighbors(IntPoint node)
		{
			foreach (var neighbor in node.Neighbors)
				if (IsInBounds(neighbor))
					yield return neighbor;
		}

		public IEnumerable<IntPoint> GetAllIndexes()
		{
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
					yield return new(x, y);
		}

		public class WithValues<T>: IGraph<IntPoint>.WithNodeIndexes<IntPoint>.WithBounds.TwoDimensionalGrid.Rectangular, IGraph<IntPoint>.WithNodeValues<T>, IGameLocationTileGraph
		{
			public delegate T ValueDelegate(GameLocation location, IntPoint point);
			public delegate bool DoesConnectDelegate(GameLocation location, (IntPoint point, T value) from, (IntPoint point, T value) to);
			
			private readonly GameLocationTileGraph Parent;
			private readonly ValueDelegate Value;
			public readonly DoesConnectDelegate DoesConnect;

			public GameLocation Location
				=> Parent.Location;

			public IntPoint TopLeft
				=> Parent.TopLeft;

			public IntPoint TopRight
				=> Parent.TopRight;

			public IntPoint BottomLeft
				=> Parent.BottomLeft;

			public IntPoint BottomRight
				=> Parent.BottomRight;

			public WithValues(GameLocation location, ValueDelegate value, DoesConnectDelegate? doesConnect = null)
				: this(new GameLocationTileGraph(location), value, doesConnect) { }

			public WithValues(GameLocationTileGraph parent, ValueDelegate value, DoesConnectDelegate? doesConnect = null)
			{
				this.Parent = parent;
				this.Value = value;
				this.DoesConnect = doesConnect ?? ((location, from, to) => true);
			}

			public Option<IntPoint> this[IntPoint index]
				=> Parent[index];

			public bool IsInBounds(IntPoint index)
				=> Parent.IsInBounds(index);

			public IntPoint GetIndex(IntPoint node)
				=> Parent.GetIndex(node);

			public IEnumerable<IntPoint> GetNeighbors(IntPoint node)
				=> Parent.GetNeighbors(node).Where(neighbor => DoesConnect(Location, from: (node, GetValue(node)), to: (neighbor, GetValue(neighbor))));

			public IEnumerable<IntPoint> GetAllIndexes()
				=> Parent.GetAllIndexes();

			public T GetValue(IntPoint node)
				=> Value(Parent.Location, node);

			public class Caching: IGraph<IntPoint>.WithNodeIndexes<IntPoint>.WithBounds.TwoDimensionalGrid.Rectangular, IGraph<IntPoint>.WithNodeValues<T>, IGameLocationTileGraph
			{
				private readonly WithValues<T> Parent;
				private readonly T?[,] Cache;

				public GameLocation Location
					=> Parent.Location;

				public IntPoint TopLeft
					=> Parent.TopLeft;

				public IntPoint TopRight
					=> Parent.TopRight;

				public IntPoint BottomLeft
					=> Parent.BottomLeft;

				public IntPoint BottomRight
					=> Parent.BottomRight;

				public Caching(GameLocation location, ValueDelegate value, DoesConnectDelegate? doesConnect = null)
					: this(new WithValues<T>(location, value, doesConnect)) { }

				public Caching(WithValues<T> parent)
				{
					this.Parent = parent;
					Cache = new T?[parent.TopRight.X - parent.TopLeft.X + 1, parent.BottomLeft.Y - parent.TopLeft.Y + 1];
				}

				public Option<IntPoint> this[IntPoint index]
					=> Parent[index];

				public bool IsInBounds(IntPoint index)
					=> Parent.IsInBounds(index);

				public IntPoint GetIndex(IntPoint node)
					=> Parent.GetIndex(node);

				public IEnumerable<IntPoint> GetNeighbors(IntPoint node)
					=> Parent.GetNeighbors(node).Where(neighbor => Parent.DoesConnect(Location, from: (node, GetValue(node)), to: (neighbor, GetValue(neighbor))));

				public IEnumerable<IntPoint> GetAllIndexes()
					=> Parent.GetAllIndexes();

				public T GetValue(IntPoint node)
				{
					var value = Cache[node.X, node.Y];
					if (value is null)
					{
						value = Parent.GetValue(node);
						Cache[node.X, node.Y] = value;
					}
					return value;
				}
			}
		}
	}
}
