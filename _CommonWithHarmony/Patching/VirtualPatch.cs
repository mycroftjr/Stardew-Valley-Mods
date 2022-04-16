using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shockah.CommonModCode.Patching
{
	public sealed class VirtualPatchFactory: IPatchFactory<VirtualPatch.Args>
	{
		private readonly Harmony Harmony;
		private readonly IPatchFactory<Patch.Args> PatchFactory;

		public VirtualPatchFactory(Harmony harmony, IPatchFactory<Patch.Args> patchFactory)
		{
			this.Harmony = harmony;
			this.PatchFactory = patchFactory;
		}

		public IPatch CreatePatch(VirtualPatch.Args args)
			=> new VirtualPatch(Harmony, args, PatchFactory);
	}

	public sealed class VirtualPatch : IPatch
	{
		public readonly struct Args
		{
			public readonly Func<MethodBase?> OriginalMethod { get; }
			public readonly HarmonyMethod? Prefix { get; }
			public readonly HarmonyMethod? Postfix { get; }
			public readonly HarmonyMethod? Finalizer { get; }

			public Args(
				MethodBase originalMethod,
				HarmonyMethod? prefix = null,
				HarmonyMethod? postfix = null,
				HarmonyMethod? finalizer = null
			) : this(() => originalMethod, prefix, postfix, finalizer) { }

			public Args(
				Func<MethodBase?> originalMethod,
				HarmonyMethod? prefix = null,
				HarmonyMethod? postfix = null,
				HarmonyMethod? finalizer = null
			)
			{
				this.OriginalMethod = originalMethod;
				this.Prefix = prefix;
				this.Postfix = postfix;
				this.Finalizer = finalizer;
			}
		}

		public Harmony Harmony { get; private set; }
		private readonly Args MyArgs;
		private readonly IPatchFactory<Patch.Args> PatchFactory;

		private IReadOnlyList<IPatch>? CurrentPatches = null;
		private IReadOnlySet<MethodBase>? CurrentPatchedMethods = null;

		public bool IsApplied => CurrentPatches == null;

		public VirtualPatch(Harmony harmony, Args args, IPatchFactory<Patch.Args> patchFactory)
		{
			this.Harmony = harmony;
			this.MyArgs = args;
			this.PatchFactory = patchFactory;
		}

		public IReadOnlySet<MethodBase> Apply()
		{
			if (CurrentPatches is not null)
				return new HashSet<MethodBase>();
			if (MyArgs.Prefix is null && MyArgs.Postfix is null && MyArgs.Finalizer is null)
			{
				CurrentPatches = Array.Empty<IPatch>();
				CurrentPatchedMethods = new HashSet<MethodBase>();
				return new HashSet<MethodBase>();
			}

			var originalMethod = MyArgs.OriginalMethod() ?? throw new NullReferenceException("Provided `originalMethod` is `null`.");
			IList<IPatch> patches = new List<IPatch>();
			ISet<MethodBase> patchedMethods = new HashSet<MethodBase>();

			Type? declaringType = originalMethod.DeclaringType;
			if (declaringType == null)
				throw new ArgumentException($"{nameof(originalMethod)}.{nameof(originalMethod.DeclaringType)} is null.");
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				IEnumerable<Type> subtypes = Enumerable.Empty<Type>();
				try
				{
					subtypes = assembly.GetTypes().Where(t => t.IsAssignableTo(declaringType));
				}
				catch (Exception)
				{
				}

				foreach (Type subtype in subtypes)
				{
					var originalParameters = originalMethod.GetParameters();
					var subtypeOriginal = AccessTools.Method(
						subtype,
						originalMethod.Name,
						originalParameters.Select(p => p.ParameterType).ToArray()
					);
					if (subtypeOriginal is null)
						continue;
					if (!subtypeOriginal.IsDeclaredMember())
						continue;
					if (!subtypeOriginal.HasMethodBody())
						continue;

					static bool ContainsNonSpecialArguments(HarmonyMethod patch)
						=> patch.method.GetParameters().Any(p => !(p.Name ?? "").StartsWith("__"));

					if (
						(MyArgs.Prefix is not null && ContainsNonSpecialArguments(MyArgs.Prefix)) ||
						(MyArgs.Postfix is not null && ContainsNonSpecialArguments(MyArgs.Postfix)) ||
						(MyArgs.Finalizer is not null && ContainsNonSpecialArguments(MyArgs.Finalizer))
					)
					{
						var subtypeOriginalParameters = subtypeOriginal.GetParameters();
						for (int i = 0; i < originalMethod.GetParameters().Length; i++)
							if (originalParameters[i].Name != subtypeOriginalParameters[i].Name)
								throw new InvalidOperationException($"Method {declaringType.Name}.{originalMethod.Name} cannot be automatically patched for subtype {subtype.Name}, because argument #{i} has a mismatched name: `{originalParameters[i].Name}` vs `{subtypeOriginalParameters[i].Name}`.");
					}

					IPatch patch = PatchFactory.CreatePatch(new Patch.Args(subtypeOriginal, MyArgs.Prefix, MyArgs.Postfix, null, MyArgs.Finalizer));
					foreach (var patchedMethod in patch.Apply())
						patchedMethods.Add(patchedMethod);
					patches.Add(patch);
				}
			}

			CurrentPatches = (IReadOnlyList<IPatch>)patches;
			CurrentPatchedMethods = (IReadOnlySet<MethodBase>)patchedMethods;
			return CurrentPatchedMethods;
		}

		public IReadOnlySet<MethodBase> Unapply()
		{
			if (CurrentPatches is null || CurrentPatchedMethods is null)
				return new HashSet<MethodBase>();
			foreach (var patch in CurrentPatches)
				patch.Unapply();
			var results = CurrentPatchedMethods.ToHashSet();
			CurrentPatches = null;
			CurrentPatchedMethods = null;
			return results;
		}
	}
}
