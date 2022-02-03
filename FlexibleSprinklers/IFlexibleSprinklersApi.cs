﻿using Microsoft.Xna.Framework;
using StardewValley;

namespace Shockah.FlexibleSprinklers
{
	/// <summary>The API which provides access to Flexible Sprinklers for other mods.</summary>
	public interface IFlexibleSprinklersApi
	{
		/// <summary>
		/// Register a new sprinkler tier provider, to add support for Flexible Sprinklers for your custom tiered sprinklers in your mod or override existing ones.<br />
		/// This is only used for tiered sprinkler power config overrides (how many tiles they water).<br />
		/// Return `null` if you don't want to modify this specific tier.
		/// </summary>
		void RegisterSprinklerTierProvider(System.Func<Object, int?> provider);

		/// <summary>
		/// Register a new sprinkler coverage provider, to add support for Flexible Sprinklers for your custom sprinklers in your mod or override existing ones.<br />
		/// Returned tile coverage should be relative.<br />
		/// Return `null` if you don't want to modify this specific coverage.
		/// </summary>
		void RegisterSprinklerCoverageProvider(System.Func<Object, Vector2[]> provider);

		/// <summary>Activates a sprinkler, taking into account the Flexible Sprinklers mod behavior.</summary>
		void ActivateSprinkler(Object sprinkler, GameLocation location);

		/// <summary>Get the relative tile coverage by supported sprinkler ID. This API is location/position-agnostic. Note that sprinkler IDs may change after a save is loaded due to Json Assets reallocating IDs.</summary>
		Vector2[] GetUnmodifiedSprinklerCoverage(Object sprinkler);

		/// <summary>Get the relative tile coverage by supported sprinkler ID, modified by the Flexible Sprinklers mod. This API takes into consideration the location and position. Note that sprinkler IDs may change after a save is loaded due to Json Assets reallocating IDs.</summary>
		Vector2[] GetModifiedSprinklerCoverage(Object sprinkler, GameLocation location);
	}
}