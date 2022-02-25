using System;
using System.Collections.Generic;

namespace Shockah.CommonModCode.Graph
{
	public class EdgeWeightDistanceGraphSearch<Node, WeightUnit>: IGraphSearch<IGraph<Node>.WithEdgeWeights<WeightUnit>, Node, WeightUnit>.WithMultipleStartingNodes
		where Node: notnull
		where WeightUnit: INumber<WeightUnit>.WithZeroDomain, INumber<WeightUnit>.WithAddition
	{
		public readonly INumberDomain<WeightUnit>.WithZero WeightUnitDomain;
		public readonly WeightUnit? MaxEdgeWeightDistance;

		public EdgeWeightDistanceGraphSearch(INumberDomain<WeightUnit>.WithZero weightUnitDomain, WeightUnit? maxEdgeWeightDistance = default)
		{
			this.WeightUnitDomain = weightUnitDomain;
			this.MaxEdgeWeightDistance = maxEdgeWeightDistance;
		}

		public IReadOnlyDictionary<Node, WeightUnit> FindNodes(IGraph<Node>.WithEdgeWeights<WeightUnit> graph, Node startingNode)
		{
			LinkedList<(Node node, WeightUnit edgeWeightDistance)> toCheck = new();
			toCheck.AddLast((node: startingNode, edgeWeightDistance: WeightUnitDomain.Zero));
			return FindNodes(graph, toCheck);
		}

		public IReadOnlyDictionary<Node, WeightUnit> FindNodes(IGraph<Node>.WithEdgeWeights<WeightUnit> graph, IEnumerable<Node> startingNodes)
		{
			LinkedList<(Node node, WeightUnit edgeWeightDistance)> toCheck = new();
			foreach (var startingNode in startingNodes)
				toCheck.AddLast((node: startingNode, edgeWeightDistance: WeightUnitDomain.Zero));
			if (toCheck.Count == 0)
				throw new ArgumentException("At least a single starting node is required.");
			return FindNodes(graph, toCheck);
		}

		private IReadOnlyDictionary<Node, WeightUnit> FindNodes(IGraph<Node>.WithEdgeWeights<WeightUnit> graph, LinkedList<(Node node, WeightUnit edgeWeightDistance)> toCheck)
		{
			IDictionary<Node, WeightUnit> results = new Dictionary<Node, WeightUnit>();
			ISet<Node> @checked = new HashSet<Node>();
			while (true)
			{
				var listNode = toCheck.First;
				if (listNode is null)
					break;
				var (node, edgeWeightDistance) = listNode.Value;
				toCheck.RemoveFirst();
				@checked.Add(node);
				if (MaxEdgeWeightDistance is not null && edgeWeightDistance > MaxEdgeWeightDistance)
					continue;

				if (!results.TryGetValue(node, out var existingEdgeWeightDistance) || edgeWeightDistance < existingEdgeWeightDistance)
					results[node] = edgeWeightDistance;

				foreach (var neighbor in graph.GetNeighbors(node))
					if (!@checked.Contains(neighbor))
						toCheck.AddLast((node: neighbor, edgeWeightDistance: edgeWeightDistance + graph.GetWeight(node, neighbor)!));
			}
			return (IReadOnlyDictionary<Node, WeightUnit>)results;
		}
	}
}
