using HarmonyLib;
using StardewModdingAPI;

namespace Shockah.ExtraQualities
{
	public class ExtraQualities: Mod
	{
		public const int PoorQuality = 101;
		public const int WonderfulQuality = 102;

		internal static ExtraQualities Instance { get; set; }

		public override void Entry(IModHelper helper)
		{
			Instance = this;

			ApplyPatches();
		}

		private void ApplyPatches()
		{
			var harmony = new Harmony(ModManifest.UniqueID);
			ObjectPatches.ApplyPatches(harmony);
		}
	}
}