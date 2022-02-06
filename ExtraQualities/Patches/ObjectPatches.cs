using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace Shockah.ExtraQualities
{
	internal static class ObjectPatches
	{
		internal static void ApplyPatches(Harmony harmony)
		{
			try
			{
				harmony.Patch(
					original: AccessTools.Method(typeof(SObject), "_PopulateContextTags"),
					postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(PopulateContextTags_Postfix))
				);

				// TODO: (?) `rot` and change item quality to Poor
				// TODO: `performObjectDropInAction` -> `if name.Equals("Loom")` -> `doubleChance`
				// TODO: performRemoveAction -> if bigCraftable -> `if quality != 0` + `quality - 1`
			}
			catch (Exception e)
			{
				ExtraQualities.Instance.Monitor.Log($"Could not patch methods - Extra Qualities probably won't work.\nReason: {e}", LogLevel.Error);
			}
		}

		private static void PopulateContextTags_Postfix(SObject __instance, HashSet<string> tags)
		{
			if (__instance.Quality == ExtraQualities.PoorQuality)
				tags.Add("quality_poor");
			else if (__instance.Quality == ExtraQualities.WonderfulQuality)
				tags.Add("quality_wonderful");
		}
	}
}
