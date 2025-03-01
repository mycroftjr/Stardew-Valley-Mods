﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Shockah.XPDisplay
{
	internal static class WalkOfLifeBridge
	{
		private static IImmersiveProfessionsAPI? Api { get; set; }

		private static void SetupIfNeeded()
		{
			if (Api is not null)
				return;
			if (!XPDisplay.Instance.Helper.ModRegistry.IsLoaded("DaLion.ImmersiveProfessions"))
				return;
			Api = XPDisplay.Instance.Helper.ModRegistry.GetApi<IImmersiveProfessionsAPI>("DaLion.ImmersiveProfessions");
		}

		public static bool IsPrestigeEnabled()
		{
			SetupIfNeeded();
			if (Api is null)
				return false;
			return Api.GetConfigs().EnablePrestige;
		}

		public static int GetRequiredXPPerExtendedLevel()
		{
			SetupIfNeeded();
			if (Api is null)
				return int.MaxValue;
			return (int)Api.GetConfigs().RequiredExpPerExtendedLevel;
		}

		public static (Texture2D, Rectangle)? GetExtendedSmallBar()
		{
			SetupIfNeeded();
			if (Api is null)
				return null;
			return (Game1.content.Load<Texture2D>("DaLion.ImmersiveProfessions/SkillBars"), new(0, 0, 7, 9));
		}

		public static (Texture2D, Rectangle)? GetExtendedBigBar()
		{
			SetupIfNeeded();
			if (Api is null)
				return null;
			return (Game1.content.Load<Texture2D>("DaLion.ImmersiveProfessions/SkillBars"), new(16, 0, 13, 9));
		}
	}
}