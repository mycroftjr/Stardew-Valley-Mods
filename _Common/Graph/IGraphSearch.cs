using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	#region no attributes
	public interface IGraphSearch<Graph, Node> where Graph: IGraph<Node>
	{
		IReadOnlySet<Node> FindNodes(Graph graph, Node startingNode);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithMultipleStartingNodes: IGraphSearch<Graph, Node>
		{
			IReadOnlySet<Node> FindNodes(Graph graph, IEnumerable<Node> startingNodes);

			IReadOnlySet<Node> IGraphSearch<Graph, Node>.FindNodes(Graph graph, Node startingNode)
				=> FindNodes(graph, new[] { startingNode });
		}
	}

	public static class IGraphSearchExtensions
	{
		public static IReadOnlySet<Node> FindNodes<Graph, Node>(this IGraphSearch<Graph, Node> self, Graph graph) where Graph: IGraph<Node>.WithAnyNode
		{
			var node = graph.GetAnyNode();
			return node is null ? new HashSet<Node>() : self.FindNodes(graph, node);
		}
	}
	#endregion

	#region 1 attribute
	public interface IGraphSearch<Graph, Node, Attribute> where Graph: IGraph<Node>
	{
		IReadOnlyDictionary<Node, Attribute> FindNodes(Graph graph, Node startingNode);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithMultipleStartingNodes: IGraphSearch<Graph, Node, Attribute>
		{
			IReadOnlyDictionary<Node, Attribute> FindNodes(Graph graph, IEnumerable<Node> startingNodes);

			IReadOnlyDictionary<Node, Attribute> IGraphSearch<Graph, Node, Attribute>.FindNodes(Graph graph, Node startingNode)
				=> FindNodes(graph, new[] { startingNode });
		}
	}
	#endregion
}