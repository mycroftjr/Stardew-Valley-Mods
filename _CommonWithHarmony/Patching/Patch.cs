using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shockah.CommonModCode.Patching
{
	public sealed class PatchFactory: IPatchFactory<Patch.Args>
	{
		private readonly Harmony Harmony;

		public PatchFactory(Harmony harmony)
		{
			this.Harmony = harmony;
		}

		public IPatch CreatePatch(Patch.Args args)
			=> new Patch(Harmony, args);
	}

	public sealed class Patch: IPatch
	{
		public readonly struct Args
		{
			public readonly Func<MethodBase?> OriginalMethod { get; }
			public readonly HarmonyMethod? Prefix { get; }
			public readonly HarmonyMethod? Postfix { get; }
			public readonly HarmonyMethod? Transpiler { get; }
			public readonly HarmonyMethod? Finalizer { get; }

			public Args(
				MethodBase originalMethod,
				HarmonyMethod? prefix = null,
				HarmonyMethod? postfix = null,
				HarmonyMethod? transpiler = null,
				HarmonyMethod? finalizer = null
			) : this(() => originalMethod, prefix, postfix, transpiler, finalizer) { }

			public Args(
				Func<MethodBase?> originalMethod,
				HarmonyMethod? prefix = null,
				HarmonyMethod? postfix = null,
				HarmonyMethod? transpiler = null,
				HarmonyMethod? finalizer = null
			)
			{
				this.OriginalMethod = originalMethod;
				this.Prefix = prefix;
				this.Postfix = postfix;
				this.Transpiler = transpiler;
				this.Finalizer = finalizer;
			}
		}

		public Harmony Harmony { get; private set; }
		private readonly Args MyArgs;

		public bool IsApplied { get; private set; } = false;
		
		public Patch(Harmony harmony, Args args)
		{
			this.Harmony = harmony;
			this.MyArgs = args;
		}

		public IReadOnlySet<MethodBase> Apply()
		{
			if (IsApplied)
				return new HashSet<MethodBase>();
			if (MyArgs.Prefix is null && MyArgs.Postfix is null && MyArgs.Transpiler is null && MyArgs.Finalizer is null)
			{
				IsApplied = true;
				return new HashSet<MethodBase>();
			}

			var originalMethod = MyArgs.OriginalMethod() ?? throw new NullReferenceException("Provided `originalMethod` is `null`.");
			Harmony.Patch(originalMethod, MyArgs.Prefix, MyArgs.Postfix, MyArgs.Transpiler, MyArgs.Finalizer);
			IsApplied = true;
			return new HashSet<MethodBase> { originalMethod };
		}

		public IReadOnlySet<MethodBase> Unapply()
		{
			if (!IsApplied)
				return new HashSet<MethodBase>();
			if (MyArgs.Prefix is null && MyArgs.Postfix is null && MyArgs.Transpiler is null && MyArgs.Finalizer is null)
			{
				IsApplied = false;
				return new HashSet<MethodBase>();
			}

			var originalMethod = MyArgs.OriginalMethod() ?? throw new NullReferenceException("Provided `originalMethod` is `null`.");
			if (MyArgs.Prefix is not null)
				Harmony.Unpatch(originalMethod, MyArgs.Prefix.method);
			if (MyArgs.Postfix is not null)
				Harmony.Unpatch(originalMethod, MyArgs.Postfix.method);
			if (MyArgs.Transpiler is not null)
				Harmony.Unpatch(originalMethod, MyArgs.Transpiler.method);
			if (MyArgs.Finalizer is not null)
				Harmony.Unpatch(originalMethod, MyArgs.Finalizer.method);
			IsApplied = false;
			return new HashSet<MethodBase> { originalMethod };
		}
	}
}
