using System;
using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	public class AllNodesGraphSearch<Node>: IGraphSearch<IGraph<Node>, Node>.WithMultipleStartingNodes
	{
		public IReadOnlySet<Node> FindNodes(IGraph<Node> graph, Node startingNode)
		{
			LinkedList<Node> toCheck = new();
			toCheck.AddLast(startingNode);
			return FindNodes(graph, toCheck);
		}

		public IReadOnlySet<Node> FindNodes(IGraph<Node> graph, IEnumerable<Node> startingNodes)
		{
			LinkedList<Node> toCheck = new();
			foreach (var startingNode in startingNodes)
				toCheck.AddLast(startingNode);
			if (toCheck.Count == 0)
				throw new ArgumentException("At least a single starting node is required.");
			return FindNodes(graph, toCheck);
		}

		private IReadOnlySet<Node> FindNodes(IGraph<Node> graph, LinkedList<Node> toCheck)
		{
			ISet<Node> results = new HashSet<Node>();
			while (true)
			{
				var listNode = toCheck.First;
				if (listNode is null)
					break;
				var node = listNode.Value;
				toCheck.RemoveFirst();

				if (!results.Contains(node))
				{
					results.Add(node);
					foreach (var neighbor in graph.GetNeighbors(node))
						toCheck.AddLast(neighbor);
				}
			}
			return (IReadOnlySet<Node>)results;
		}
	}
}
