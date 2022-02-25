using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	public interface IClusterGraphSearch<Graph, Node> where Graph: IGraph<Node>
	{
		IReadOnlySet<IReadOnlySet<Node>> FindNodes(Graph graph, Node startingNode);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithMultipleStartingNodes: IClusterGraphSearch<Graph, Node>
		{
			IReadOnlySet<IReadOnlySet<Node>> FindNodes(Graph graph, IEnumerable<Node> startingNodes);

			IReadOnlySet<IReadOnlySet<Node>> IClusterGraphSearch<Graph, Node>.FindNodes(Graph graph, Node startingNode)
				=> FindNodes(graph, new[] { startingNode });
		}
	}
}
