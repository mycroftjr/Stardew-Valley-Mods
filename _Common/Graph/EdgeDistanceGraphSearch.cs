using System;
using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	public class EdgeDistanceGraphSearch<Node>: IGraphSearch<IGraph<Node>, Node, int>.WithMultipleStartingNodes where Node: notnull
	{
		public readonly int MaxEdgeDistance;

		public EdgeDistanceGraphSearch(int maxEdgeDistance = int.MaxValue)
		{
			this.MaxEdgeDistance = maxEdgeDistance;
		}
		
		public IReadOnlyDictionary<Node, int> FindNodes(IGraph<Node> graph, Node startingNode)
		{
			LinkedList<(Node node, int edgeDistance)> toCheck = new();
			toCheck.AddLast((node: startingNode, edgeDistance: 0));
			return FindNodes(graph, toCheck);
		}

		public IReadOnlyDictionary<Node, int> FindNodes(IGraph<Node> graph, IEnumerable<Node> startingNodes)
		{
			LinkedList<(Node node, int edgeDistance)> toCheck = new();
			foreach (var startingNode in startingNodes)
				toCheck.AddLast((node: startingNode, edgeDistance: 0));
			if (toCheck.Count == 0)
				throw new ArgumentException("At least a single starting node is required.");
			return FindNodes(graph, toCheck);
		}

		private IReadOnlyDictionary<Node, int> FindNodes(IGraph<Node> graph, LinkedList<(Node node, int edgeDistance)> toCheck)
		{
			IDictionary<Node, int> results = new Dictionary<Node, int>();
			while (true)
			{
				var listNode = toCheck.First;
				if (listNode is null)
					break;
				var (node, edgeDistance) = listNode.Value;
				toCheck.RemoveFirst();
				if (edgeDistance > MaxEdgeDistance)
					continue;

				// we don't need to update existing keys, because we're doing BFS, so the lowest distance will always be first
				if (!results.ContainsKey(node))
				{
					results[node] = edgeDistance;
					foreach (var neighbor in graph.GetNeighbors(node))
						toCheck.AddLast((node: neighbor, edgeDistance: edgeDistance + 1));
				}
			}
			return (IReadOnlyDictionary<Node, int>)results;
		}
	}
}
