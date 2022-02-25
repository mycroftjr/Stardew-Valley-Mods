﻿using Shockah.CommonModCode;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.FlexibleSprinklers
{
	internal static class SprinklerLayouts
	{
		public static readonly ISet<IntPoint> Basic = IntPoint.NeighborOffsets.ToHashSet();
		public static ISet<IntPoint> Quality => Box(1).ToHashSet();
		public static ISet<IntPoint> Iridium => Box(2).ToHashSet();

		public static ISet<IntPoint> Vanilla(int tier)
		{
			if (tier <= 1)
				return Basic;
			else
				return Box(tier - 1).ToHashSet();
		}

		private static IEnumerable<IntPoint> Box(int radius)
		{
			for (var y = -radius; y <= radius; y++)
			{
				for (var x = -radius; x <= radius; x++)
				{
					if (x != 0 || y != 0)
						yield return new IntPoint(x, y);
				}
			}
		}
	}
}