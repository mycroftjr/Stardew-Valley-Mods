﻿using StardewModdingAPI;
using System.Collections.Generic;

namespace Shockah.PleaseGiftMeInPerson
{
	internal class OverrideAssetLoader : IAssetLoader
	{
		public bool CanLoad<T>(IAssetInfo asset)
			=> asset.AssetNameEquals(PleaseGiftMeInPerson.OverrideAssetPath);

		public T Load<T>(IAssetInfo asset)
		{
			var assetData = new Dictionary<string, string>();
			if (PleaseGiftMeInPerson.Instance.Config.EnableNPCOverrides)
			{
				assetData["Dwarf"] = $"{GiftPreference.Neutral}/{GiftPreference.Hates}";
				assetData["Elliott"] = $"{GiftPreference.Neutral}/{GiftPreference.Neutral}";
				assetData["Krobus"] = $"{GiftPreference.Neutral}/{GiftPreference.Hates}";
				assetData["Leo"] = $"{GiftPreference.Neutral}/{GiftPreference.LovesInfrequent}";
				assetData["Linus"] = $"{GiftPreference.Neutral}/{GiftPreference.DislikesAndHatesFrequent}";
				assetData["Penny"] = $"{GiftPreference.Neutral}/{GiftPreference.Neutral}";
				assetData["Sandy"] = $"{GiftPreference.LikesInfrequent}/{GiftPreference.LikesInfrequentButDislikesFrequent}";
				assetData["Sebastian"] = $"{GiftPreference.Dislikes}/{GiftPreference.Neutral}";
				assetData["Wizard"] = $"{GiftPreference.DislikesFrequent}/{GiftPreference.Neutral}";
			}
			return (T)(object)assetData;
		}
	}
}
