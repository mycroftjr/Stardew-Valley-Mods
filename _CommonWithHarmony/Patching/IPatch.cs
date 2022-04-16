using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Shockah.CommonModCode.Patching
{
	public interface IPatch
	{
		Harmony Harmony { get; }
		bool IsApplied { get; }

		IReadOnlySet<MethodBase> Apply();
		IReadOnlySet<MethodBase> Unapply();
	}
}