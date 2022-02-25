using System;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.CommonModCode.Graph
{
	public class AllNodesClusterGraphSearch<Node>: IClusterGraphSearch<IGraph<Node>, Node>.WithMultipleStartingNodes
	{
		private class Cluster
		{
			public readonly ISet<Node> Nodes = new HashSet<Node>();
		}

		private readonly Lazy<IGraphSearch<IGraph<Node>, Node>> NonGraphSearch = new(() => new AllNodesGraphSearch<Node>());

		public IReadOnlySet<IReadOnlySet<Node>> FindNodes(IGraph<Node> graph, Node startingNode)
			=> new HashSet<IReadOnlySet<Node>> { NonGraphSearch.Value.FindNodes(graph, startingNode) };

		public IReadOnlySet<IReadOnlySet<Node>> FindNodes(IGraph<Node> graph, IEnumerable<Node> startingNodes)
		{
			LinkedList<(Node node, Cluster cluster)> toCheck = new();
			foreach (var startingNode in startingNodes)
				toCheck.AddLast((node: startingNode, cluster: new()));
			return toCheck.Count switch
			{
				0 => throw new ArgumentException("At least a single starting node is required."),
				1 => new HashSet<IReadOnlySet<Node>> { NonGraphSearch.Value.FindNodes(graph, toCheck.First!.Value.node) },
				_ => FindNodes(graph, toCheck),
			};
		}

		private IReadOnlySet<IReadOnlySet<Node>> FindNodes(IGraph<Node> graph, LinkedList<(Node node, Cluster cluster)> toCheck)
		{
			IList<Cluster> clusters = new List<Cluster>();
			ISet<Node> @checked = new HashSet<Node>();

			void CombineClusters(Cluster clusterToRemove, Cluster clusterToMergeWith)
			{
				clusters.Remove(clusterToRemove);
				clusterToMergeWith.Nodes.UnionWith(clusterToRemove.Nodes);

				var current = toCheck.First;
				while (current is not null)
				{
					if (ReferenceEquals(current.Value.cluster, clusterToRemove))
						current.Value = (current.Value.node, clusterToMergeWith);
					current = current.Next;
				}
			}

			while (true)
			{
				var listNode = toCheck.First;
				if (listNode is null)
					break;
				var (node, cluster) = listNode.Value;
				toCheck.RemoveFirst();

				if (@checked.Contains(node))
				{
					var existingCluster = clusters.FirstOrDefault(c => c.Nodes.Contains(node));
					if (existingCluster is not null && !ReferenceEquals(cluster, existingCluster))
						CombineClusters(cluster, existingCluster);
				}
				else
				{
					cluster.Nodes.Add(node);
					foreach (var neighbor in graph.GetNeighbors(node))
						toCheck.AddLast((neighbor, cluster));
					@checked.Add(node);
				}
			}
			return clusters.Select(c => (IReadOnlySet<Node>)c.Nodes).ToHashSet();
		}
	}
}
