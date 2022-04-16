using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;

namespace Shockah.CommonModCode.Patching
{
	public sealed class LoggingCatchingPatchFactory<Args>: IPatchFactory<Args>
	{
		private readonly IPatchFactory<Args> PatchFactory;
		private readonly IMonitor Monitor;
		private readonly LogLevel SuccessLogLevel;
		private readonly LogLevel FailureLogLevel;

		public LoggingCatchingPatchFactory(IPatchFactory<Args> patchFactory, IMonitor monitor, LogLevel successLogLevel = LogLevel.Trace, LogLevel failureLogLevel = LogLevel.Error)
		{
			this.PatchFactory = patchFactory;
			this.Monitor = monitor;
			this.SuccessLogLevel = successLogLevel;
			this.FailureLogLevel = failureLogLevel;
		}

		public IPatch CreatePatch(Args args)
			=> new LoggingPatch(PatchFactory.CreatePatch(args), Monitor, SuccessLogLevel, FailureLogLevel);

		private sealed class LoggingPatch: IPatch
		{
			public Harmony Harmony => Patch.Harmony;
			public bool IsApplied => Patch.IsApplied;

			private readonly IMonitor Monitor;
			private readonly IPatch Patch;
			private readonly LogLevel SuccessLogLevel;
			private readonly LogLevel FailureLogLevel;

			public LoggingPatch(IPatch patch, IMonitor monitor, LogLevel successLogLevel, LogLevel failureLogLevel)
			{
				this.Monitor = monitor;
				this.Patch = patch;
				this.SuccessLogLevel = successLogLevel;
				this.FailureLogLevel = failureLogLevel;
			}

			public IReadOnlySet<MethodBase> Apply()
			{
				try
				{
					var results = Patch.Apply();
					if (results.Count != 0)
					{
						if (results.Count == 1)
							Monitor.Log($"Patched method {results.First().FullDescription()}.", SuccessLogLevel);
						else
							Monitor.Log($"Patched methods:\n{string.Join("\n", results.Select(m => $"\t{m.FullDescription()}"))}.", SuccessLogLevel);
					}
					return results;
				}
				catch (Exception ex)
				{
					Monitor.Log($"Could not patch method - the mod may not work correctly.\nReason: {ex}", FailureLogLevel);
					return new HashSet<MethodBase>();
				}
			}

			public IReadOnlySet<MethodBase> Unapply()
			{
				try
				{
					var results = Patch.Unapply();
					if (results.Count != 0)
					{
						if (results.Count == 1)
							Monitor.Log($"Unpatched method {results.First().FullDescription()}.", SuccessLogLevel);
						else
							Monitor.Log($"Unpatched methods:\n{string.Join("\n", results.Select(m => $"\t{m.FullDescription()}"))}.", SuccessLogLevel);
					}
					return results;
				}
				catch (Exception ex)
				{
					Monitor.Log($"Could not unpatch method - the mod may not work correctly.\nReason: {ex}", FailureLogLevel);
					return new HashSet<MethodBase>();
				}
			}
		}
	}
}