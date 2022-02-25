using Optional;
using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
	public interface IGraph<Node>
	{
		IEnumerable<Node> GetNeighbors(Node node);

		public interface WithAnyNode: IGraph<Node>
		{
			Node? GetAnyNode();
		}

		public interface WithNodeIndexes<Index>: IGraph<Node>
		{
			Option<Node> this[Index index] { get; }

			Index GetIndex(Node node);

			public interface WithBounds: WithNodeIndexes<Index>
			{
				bool IsInBounds(Index index);

				public interface TwoDimensionalGrid: WithBounds
				{
					Index FirstCorner { get; }
					Index SecondCorner { get; }

					IEnumerable<Index> GetAllIndexes();

					public interface Rectangular: TwoDimensionalGrid
					{
						Index TopLeft { get; }
						Index TopRight { get; }
						Index BottomLeft { get; }
						Index BottomRight { get; }

						Index TwoDimensionalGrid.FirstCorner
							=> TopLeft;

						Index TwoDimensionalGrid.SecondCorner
							=> BottomRight;
					}
				}
			}
		}

		public interface WithNodeValues<Value>: IGraph<Node>
		{
			Value GetValue(Node node);
		}

		public interface WithEdgeWeights<WeightUnit>: IGraph<Node> where WeightUnit: INumber<WeightUnit>
		{
			WeightUnit? GetWeight(Node from, Node to);
		}
	}
}
