﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.FlexibleSprinklers
{
	internal class FlexibleSprinklerBehavior: ISprinklerBehavior
	{
		public enum TileWaterBalanceMode
		{
			Relaxed, Exact, Restrictive
		}

		private readonly TileWaterBalanceMode tileWaterBalanceMode;
		private readonly ISprinklerBehavior vanillaBehavior;

		public FlexibleSprinklerBehavior(TileWaterBalanceMode tileWaterBalanceMode, ISprinklerBehavior vanillaBehavior)
		{
			this.tileWaterBalanceMode = tileWaterBalanceMode;
			this.vanillaBehavior = vanillaBehavior;
		}

		public ISet<IntPoint> GetSprinklerTiles(IMap map, IntPoint sprinklerPosition, SprinklerInfo info)
		{
			var wateredTiles = new HashSet<IntPoint>();
			var unwateredTileCount = info.Power;

			void WaterTile(IntPoint tilePosition)
			{
				unwateredTileCount--;
				wateredTiles.Add(tilePosition);
			}

			void WaterTiles(IEnumerable<IntPoint> tilePositions)
			{
				foreach (var tilePosition in tilePositions)
				{
					WaterTile(tilePosition);
				}
			}

			if (vanillaBehavior != null)
			{
				foreach (var tileToWater in vanillaBehavior.GetSprinklerTiles(map, sprinklerPosition, info))
				{
					switch (map[tileToWater])
					{
						case SoilType.Dry:
						case SoilType.Wet:
							WaterTile(tileToWater);
							break;
						case SoilType.NonWaterable:
						case SoilType.Sprinkler:
						case SoilType.NonSoil:
							break;
					}
				}
			}
			if (unwateredTileCount <= 0)
				return wateredTiles;

			var sprinklerRange = FlexibleSprinklers.Instance.GetFloodFillSprinklerRange(info.Power);
			var waterableTiles = new HashSet<IntPoint>();
			var otherSprinklers = new HashSet<IntPoint>();
			var @checked = new HashSet<IntPoint>();
			var toCheck = new Queue<IntPoint>();
			var maxCost = 0;

			var maxDX = Math.Max(wateredTiles.Count > 0 ? wateredTiles.Max(t => Math.Abs(t.X - sprinklerPosition.X)) : 0, sprinklerRange);
			var maxDY = Math.Max(wateredTiles.Count > 0 ? wateredTiles.Max(t => Math.Abs(t.Y - sprinklerPosition.Y)) : 0, sprinklerRange);

			var costArray = new int[maxDX * 2 + 1, maxDY * 2 + 1];
			var costArrayBaseXIndex = maxDX;
			var costArrayBaseYIndex = maxDY;

			for (int y = 0; y < costArray.GetLength(1); y++)
			{
				for (int x = 0; x < costArray.GetLength(0); x++)
				{
					costArray[x, y] = int.MaxValue;
				}
			}

			int GetCost(IntPoint point)
			{
				return costArray[costArrayBaseXIndex + point.X - sprinklerPosition.X, costArrayBaseYIndex + point.Y - sprinklerPosition.Y];
			}

			void SetCost(IntPoint point, int cost)
			{
				costArray[costArrayBaseXIndex + point.X - sprinklerPosition.X, costArrayBaseYIndex + point.Y - sprinklerPosition.Y] = cost;
			}

			@checked.Add(sprinklerPosition);
			SetCost(sprinklerPosition, 0);
			foreach (var wateredTile in wateredTiles)
			{
				toCheck.Enqueue(wateredTile);
				SetCost(wateredTile, 0);
			}
			foreach (var neighbor in sprinklerPosition.Neighbors)
			{
				toCheck.Enqueue(neighbor);
				SetCost(neighbor, 1);
			}

			while (toCheck.Count > 0)
			{
				var tilePosition = toCheck.Dequeue();
				@checked.Add(tilePosition);

				var tilePathLength = GetCost(tilePosition);
				var newTilePathLength = tilePathLength + 1;

				if (waterableTiles.Count >= unwateredTileCount && newTilePathLength > maxCost)
					continue;

				switch (map[tilePosition])
				{
					case SoilType.Dry:
					case SoilType.Wet:
						if (!wateredTiles.Contains(tilePosition))
							waterableTiles.Add(tilePosition);
						break;
					case SoilType.Sprinkler:
						otherSprinklers.Add(tilePosition);
						continue;
					case SoilType.NonSoil:
					case SoilType.NonWaterable:
						continue;
				}

				if (tilePathLength == sprinklerRange)
					continue;

				foreach (var neighbor in tilePosition.Neighbors)
				{
					if (@checked.Contains(neighbor) || Math.Abs(neighbor.X - sprinklerPosition.X) > maxDX || Math.Abs(neighbor.Y - sprinklerPosition.Y) > maxDY)
						continue;
					toCheck.Enqueue(neighbor);
					var newCost = Math.Min(GetCost(neighbor), newTilePathLength);
					SetCost(neighbor, newCost);
					maxCost = Math.Max(newCost, maxCost);
				}
			}

			var otherSprinklerDetectionRange = (int)Math.Sqrt(sprinklerRange - 1);

			IEnumerable<IntPoint> DetectSprinklers(IntPoint singleDirection)
			{
				int[] directions = { -1, 1 };
				
				for (int i = 1; i <= otherSprinklerDetectionRange; i++)
				{
					foreach (var direction in directions)
					{
						var position = sprinklerPosition + singleDirection * direction * i;
						if (!otherSprinklers.Contains(position))
							continue;
						if (map[position] == SoilType.Sprinkler)
							yield return position;
					}
				}
			}

			int? ClosestSprinkler(IntPoint singleDirection)
			{
				return DetectSprinklers(singleDirection)
					.Select(p => (Math.Abs(p.X - sprinklerPosition.X) + Math.Abs(p.Y - sprinklerPosition.Y)) as int?)
					.FirstOrDefault();
			}

			var sprinklerNeighbors = sprinklerPosition.Neighbors;
			var horizontalSprinklerDistance = ClosestSprinkler(IntPoint.Right);
			var verticalSprinklerDistance = ClosestSprinkler(IntPoint.Bottom);

			var sortedWaterableTiles = waterableTiles
				.Select(e => {
					var dx = Math.Abs(e.X - sprinklerPosition.X) * ((horizontalSprinklerDistance ?? 0) * sprinklerRange + 1);
					var dy = Math.Abs(e.Y - sprinklerPosition.Y) * ((verticalSprinklerDistance ?? 0) * sprinklerRange + 1);
					return (
						tilePosition: e,
						pathLength: GetCost(e),
						distance: Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2))
					);
				})
				.OrderBy(e => e.pathLength)
				.ThenBy(e => e.distance)
				.ToList();

			while (unwateredTileCount > 0 && sortedWaterableTiles.Count > 0)
			{
				var reachable = sortedWaterableTiles
					.Where(e => sprinklerNeighbors.Contains(e.tilePosition) || wateredTiles.SelectMany(t => t.Neighbors).Contains(e.tilePosition))
					.ToList();
				var currentDistance = reachable.First().distance;
				var tileEntries = reachable.TakeWhile(e => e.distance == currentDistance).ToList();
				if (tileEntries.Count == 0)
				{
					FlexibleSprinklers.Instance.Monitor.Log($"Could not find all tiles to water for sprinkler at {sprinklerPosition}.", StardewModdingAPI.LogLevel.Warn);
					break;
				}

				foreach (var tileEntry in tileEntries)
				{
					sortedWaterableTiles.Remove(tileEntry);
				}

				if (unwateredTileCount >= tileEntries.Count)
				{
					WaterTiles(tileEntries.Select(e => e.tilePosition));
				}
				else
				{
					switch (tileWaterBalanceMode)
					{
						case TileWaterBalanceMode.Relaxed:
							WaterTiles(tileEntries.Select(e => e.tilePosition));
							break;
						case TileWaterBalanceMode.Restrictive:
							unwateredTileCount = 0;
							break;
						case TileWaterBalanceMode.Exact:
							IEnumerable<IntPoint> GetSpiralingTiles()
							{
								var minD = tileEntries.Min(e => Math.Max(Math.Abs(e.tilePosition.X - sprinklerPosition.X), Math.Abs(e.tilePosition.Y - sprinklerPosition.Y)));
								var maxD = tileEntries.Max(e => Math.Max(Math.Abs(e.tilePosition.X - sprinklerPosition.X), Math.Abs(e.tilePosition.Y - sprinklerPosition.Y)));

								for (int i = minD; i <= maxD; i++)
								{
									var borderTiles = new List<IntPoint>();
									for (int j = 0; j <= i; j++)
									{
										yield return new IntPoint(sprinklerPosition.X - j, sprinklerPosition.Y - i);
										if (j != 0)
											yield return new IntPoint(sprinklerPosition.X + j, sprinklerPosition.Y - i);

										yield return new IntPoint(sprinklerPosition.X + i, sprinklerPosition.Y - j);
										if (j != 0)
											yield return new IntPoint(sprinklerPosition.X + i, sprinklerPosition.Y + j);

										yield return new IntPoint(sprinklerPosition.X + j, sprinklerPosition.Y + i);
										if (j != 0)
											yield return new IntPoint(sprinklerPosition.X - j, sprinklerPosition.Y + i);

										yield return new IntPoint(sprinklerPosition.X - i, sprinklerPosition.Y + j);
										if (j != 0)
											yield return new IntPoint(sprinklerPosition.X - i, sprinklerPosition.Y - j);
									}
								}
							}

							foreach (var spiralingTile in GetSpiralingTiles())
							{
								foreach (var tileEntry in tileEntries)
								{
									if (tileEntry.tilePosition == spiralingTile)
									{
										WaterTile(tileEntry.tilePosition);
										tileEntries.Remove(tileEntry);
										if (unwateredTileCount <= 0)
											goto done;
										break;
									}
								}
							}
							done:;
							break;
					}
				}
			}

			return wateredTiles;
		}
	}
}