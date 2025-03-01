﻿using StardewValley;
using StardewValley.Objects;
using System.Diagnostics.CodeAnalysis;
using SObject = StardewValley.Object;

namespace Shockah.CommonModCode.Stardew
{
	public static class SObjectExt
	{
		public static bool TryGetAnyHeldObject(this SObject self, [NotNullWhen(true)] out SObject? heldObject)
		{
			var anyHeldObject = self.GetAnyHeldObject();
			if (anyHeldObject is null)
			{
				heldObject = null;
				return false;
			}
			else
			{
				heldObject = anyHeldObject;
				return true;
			}
		}

		public static SObject? GetAnyHeldObject(this SObject self)
		{
			if (self.heldObject.Value is Chest || self.heldObject.Value?.Name == "Chest")
				return null;
			if (self.heldObject.Value is not null)
				return self.heldObject.Value;
			if (self is CrabPot crabPot && crabPot.bait.Value is not null)
				return crabPot.bait.Value;
			if (self is WoodChipper woodChipper && woodChipper.depositedItem.Value is not null)
				return woodChipper.depositedItem.Value;
			return null;
		}

		public static GameLocation? FindGameLocation(this SObject self, GameLocation? potentialLocation = null)
		{
			static bool IsObjectInLocation(SObject @object, GameLocation location)
				=> location.getObjectAtTile((int)@object.TileLocation.X, (int)@object.TileLocation.Y) == @object;

			if (potentialLocation is not null && IsObjectInLocation(self, potentialLocation))
				return potentialLocation;
			foreach (GameLocation location in GameExt.GetAllLocations())
				if (IsObjectInLocation(self, location))
					return location;
			return null;
		}
	}
}
