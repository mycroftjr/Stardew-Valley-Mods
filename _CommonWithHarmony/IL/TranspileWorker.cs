using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Shockah.CommonModCode.IL
{
	public class TranspileWorkerException : Exception
	{
		public TranspileWorkerException(string message) : base(message) { }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested interfaces")]
	public interface ITranspileWorker
	{
		public enum Direction { Forward, Backward }
		public enum RangePosition { AtStart, AtEnd, BeforeStart, AfterEnd }
		public enum AdvanceBehavior { MoveCursor, Encompass }
		//public enum BoundaryLabelBehavior { Throw, MoveToNewSpot, KeepAtOldSpot, Drop }
		public enum BoundaryLabelReplacementBehavior { Throw, MoveToNewSpot, Drop }
		public enum NonBoundaryLabelBehavior { Throw, MoveToStart, MoveToEnd, Drop }

		IReadOnlyList<CodeInstruction> Instructions { get; }
		Range Range { get; }

		#region Cursor
		ITranspileWorker Find(IReadOnlyList<IILDescriptor> instructionsToFind, Direction direction = Direction.Forward);
		ITranspileWorker Advance(int instructions = 1, AdvanceBehavior behavior = AdvanceBehavior.MoveCursor, Direction direction = Direction.Forward);
		#endregion

		#region Modification
		ITranspileWorker Prefix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false);
		ITranspileWorker Postfix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false);
		ITranspileWorker Replace(
			IEnumerable<CodeInstruction> newInstructions,
			BoundaryLabelReplacementBehavior boundaryLabelBehavior = BoundaryLabelReplacementBehavior.MoveToNewSpot,
			NonBoundaryLabelBehavior nonBoundaryLabelBehavior = NonBoundaryLabelBehavior.Throw
		);
		#endregion

		public interface LabelCreation: ITranspileWorker
		{
			#region Cursor
			new LabelCreation Find(IReadOnlyList<IILDescriptor> instructionsToFind, Direction direction = Direction.Forward)
				=> (LabelCreation)((ITranspileWorker)this).Find(instructionsToFind, direction);
			new LabelCreation Advance(int instructions = 1, AdvanceBehavior behavior = AdvanceBehavior.MoveCursor, Direction direction = Direction.Forward)
				=> (LabelCreation)((ITranspileWorker)this).Advance(instructions, behavior, direction);
			#endregion

			#region Modification
			new LabelCreation Prefix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LabelCreation)((ITranspileWorker)this).Prefix(newInstructions, moveBoundaryLabels);
			new LabelCreation Postfix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LabelCreation)((ITranspileWorker)this).Postfix(newInstructions, moveBoundaryLabels);
			new LabelCreation Replace(
				IEnumerable<CodeInstruction> newInstructions,
				BoundaryLabelReplacementBehavior boundaryLabelBehavior = BoundaryLabelReplacementBehavior.MoveToNewSpot,
				NonBoundaryLabelBehavior nonBoundaryLabelBehavior = NonBoundaryLabelBehavior.Throw
			)
				=> (LabelCreation)((ITranspileWorker)this).Replace(newInstructions, boundaryLabelBehavior, nonBoundaryLabelBehavior);
			#endregion

			#region Labels
			Label PutLabel(RangePosition position = RangePosition.AtStart);

			LabelCreation PutLabel(out Label label, RangePosition position = RangePosition.AtStart)
			{
				label = PutLabel(position);
				return this;
			}
			#endregion
		}

		public interface LocalLookup: ITranspileWorker
		{
			#region Cursor
			new LocalLookup Find(IReadOnlyList<IILDescriptor> instructionsToFind, Direction direction = Direction.Forward)
				=> (LocalLookup)((ITranspileWorker)this).Find(instructionsToFind, direction);
			new LocalLookup Advance(int instructions = 1, AdvanceBehavior behavior = AdvanceBehavior.MoveCursor, Direction direction = Direction.Forward)
				=> (LocalLookup)((ITranspileWorker)this).Advance(instructions, behavior, direction);
			#endregion

			#region Modification
			new LocalLookup Prefix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LocalLookup)((ITranspileWorker)this).Prefix(newInstructions, moveBoundaryLabels);
			new LocalLookup Postfix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LocalLookup)((ITranspileWorker)this).Postfix(newInstructions, moveBoundaryLabels);
			new LocalLookup Replace(
				IEnumerable<CodeInstruction> newInstructions,
				BoundaryLabelReplacementBehavior boundaryLabelBehavior = BoundaryLabelReplacementBehavior.MoveToNewSpot,
				NonBoundaryLabelBehavior nonBoundaryLabelBehavior = NonBoundaryLabelBehavior.Throw
			)
				=> (LocalLookup)((ITranspileWorker)this).Replace(newInstructions, boundaryLabelBehavior, nonBoundaryLabelBehavior);
			#endregion

			#region Locals
			int GetLocalIndex(Type type, int skip = 0);

			LocalLookup GetLocalIndex(out int localIndex, Type type, int skip = 0)
			{
				localIndex = GetLocalIndex(type, skip);
				return this;
			}
			#endregion
		}

		public interface LabelCreationAndLocalLookup: LabelCreation, LocalLookup
		{
			#region Cursor
			new LabelCreationAndLocalLookup Find(IReadOnlyList<IILDescriptor> instructionsToFind, Direction direction = Direction.Forward)
				=> (LabelCreationAndLocalLookup)((ITranspileWorker)this).Find(instructionsToFind, direction);
			new LabelCreationAndLocalLookup Advance(int instructions = 1, AdvanceBehavior behavior = AdvanceBehavior.MoveCursor, Direction direction = Direction.Forward)
				=> (LabelCreationAndLocalLookup)((ITranspileWorker)this).Advance(instructions, behavior, direction);
			#endregion

			#region Modification
			new LabelCreationAndLocalLookup Prefix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LabelCreationAndLocalLookup)((ITranspileWorker)this).Prefix(newInstructions, moveBoundaryLabels);
			new LabelCreationAndLocalLookup Postfix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
				=> (LabelCreationAndLocalLookup)((ITranspileWorker)this).Postfix(newInstructions, moveBoundaryLabels);
			new LabelCreationAndLocalLookup Replace(
				IEnumerable<CodeInstruction> newInstructions,
				BoundaryLabelReplacementBehavior boundaryLabelBehavior = BoundaryLabelReplacementBehavior.MoveToNewSpot,
				NonBoundaryLabelBehavior nonBoundaryLabelBehavior = NonBoundaryLabelBehavior.Throw
			)
				=> (LabelCreationAndLocalLookup)((ITranspileWorker)this).Replace(newInstructions, boundaryLabelBehavior, nonBoundaryLabelBehavior);
			#endregion

			#region Labels
			new LabelCreationAndLocalLookup PutLabel(out Label label, RangePosition position = RangePosition.AtStart)
				=> (LabelCreationAndLocalLookup)((LabelCreation)this).PutLabel(out label, position);
			#endregion

			#region Locals
			new LabelCreationAndLocalLookup GetLocalIndex(out int localIndex, Type type, int skip = 0)
				=> (LabelCreationAndLocalLookup)((LocalLookup)this).GetLocalIndex(out localIndex, type, skip);
			#endregion
		}
	}

	public static class TranspileWorkers
	{
		public static ITranspileWorker Create(IEnumerable<CodeInstruction> instructions)
			=> new BasicTranspileWorker(instructions);

		public static ITranspileWorker.LabelCreation Create(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
			=> new LabelCreatingTranspileWorker(instructions, ilGenerator);

		public static ITranspileWorker.LocalLookup Create(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
			=> new LocalLookupTranspileWorker(instructions, originalMethod);

		public static ITranspileWorker.LabelCreationAndLocalLookup Create(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator, MethodBase originalMethod)
			=> new LabelCreatingAndLocalLookupTranspileWorker(instructions, ilGenerator, originalMethod);

		internal static Label PutLabel(IList<CodeInstruction> instructions, Range range, ILGenerator ilGenerator, ITranspileWorker.RangePosition position)
		{
			Label label = ilGenerator.DefineLabel();
			int index = position switch
			{
				ITranspileWorker.RangePosition.AtStart => range.Start.Value,
				ITranspileWorker.RangePosition.AtEnd => range.End.Value - 1,
				ITranspileWorker.RangePosition.BeforeStart => range.Start.Value - 1,
				ITranspileWorker.RangePosition.AfterEnd => range.End.Value,
				_ => throw new ArgumentException($"{nameof(ITranspileWorker.RangePosition)} has an invalid value.")
			};
			instructions[index].labels.Add(label);
			return label;
		}
	}

	internal class LabelCreatingTranspileWorker: BasicTranspileWorker, ITranspileWorker.LabelCreation
	{
		protected readonly ILGenerator ILGenerator;

		public LabelCreatingTranspileWorker(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) : base(instructions)
		{
			this.ILGenerator = ilGenerator;
		}

		public Label PutLabel(ITranspileWorker.RangePosition position = ITranspileWorker.RangePosition.AtStart)
			=> TranspileWorkers.PutLabel(MutableInstructions, Range, ILGenerator, position);
	}

	internal class LabelCreatingAndLocalLookupTranspileWorker: LocalLookupTranspileWorker, ITranspileWorker.LabelCreationAndLocalLookup
	{
		protected readonly ILGenerator ILGenerator;

		public LabelCreatingAndLocalLookupTranspileWorker(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator, MethodBase originalMethod) : base(instructions, originalMethod)
		{
			this.ILGenerator = ilGenerator;
		}

		public Label PutLabel(ITranspileWorker.RangePosition position = ITranspileWorker.RangePosition.AtStart)
			=> TranspileWorkers.PutLabel(MutableInstructions, Range, ILGenerator, position);
	}

	internal class BasicTranspileWorker: ITranspileWorker
	{
		public IReadOnlyList<CodeInstruction> Instructions => (IReadOnlyList<CodeInstruction>)MutableInstructions;
		public Range Range { get; protected set; }

		protected readonly IList<CodeInstruction> MutableInstructions;

		public BasicTranspileWorker(IEnumerable<CodeInstruction> instructions)
		{
			this.MutableInstructions = instructions.ToList();
			this.Range = 0..^MutableInstructions.Count;
		}

		public ITranspileWorker Find(IReadOnlyList<IILDescriptor> instructionsToFind, ITranspileWorker.Direction direction = ITranspileWorker.Direction.Forward)
		{
			switch (direction)
			{
				case ITranspileWorker.Direction.Forward:
					var maxIndex = (MutableInstructions.Count) - instructionsToFind.Count;
					for (int index = Range.Start.Value; index < maxIndex; index++)
					{
						for (int toFindIndex = 0; toFindIndex < instructionsToFind.Count; toFindIndex++)
						{
							if (!instructionsToFind[toFindIndex].Matches(MutableInstructions[index + toFindIndex]))
								goto continueOuter;
						}
						return this;
						continueOuter:;
					}
					break;
				case ITranspileWorker.Direction.Backward:
					var minIndex = instructionsToFind.Count - 1;
					var intEndIndex = Range.End.Value;
					for (int index = intEndIndex; index >= minIndex; index--)
					{
						for (int toFindIndex = instructionsToFind.Count - 1; toFindIndex >= 0; toFindIndex--)
						{
							if (!instructionsToFind[toFindIndex].Matches(MutableInstructions[index + toFindIndex]))
								goto continueOuter;
						}
						return this;
						continueOuter:;
					}
					break;
			}
			throw new TranspileWorkerException($"Could not find the given set of instructions in the {direction} direction:\n{string.Join("\n", instructionsToFind)}");
		}

		public ITranspileWorker Advance(
			int instructions = 1,
			ITranspileWorker.AdvanceBehavior behavior = ITranspileWorker.AdvanceBehavior.MoveCursor,
			ITranspileWorker.Direction direction = ITranspileWorker.Direction.Forward
		)
		{
			if (instructions == 0)
				return this;
			if (instructions < 0)
				throw new ArgumentException($"{nameof(instructions)} cannot be less than 0.");

			Range newRange;
			switch (direction)
			{
				case ITranspileWorker.Direction.Forward:
					{
						int newIndex = Range.End.Value - 1 + instructions;
						newRange = behavior switch
						{
							ITranspileWorker.AdvanceBehavior.MoveCursor => newIndex..newIndex,
							ITranspileWorker.AdvanceBehavior.Encompass => Range.Start..newIndex,
							_ => throw new ArgumentException($"{nameof(ITranspileWorker.AdvanceBehavior)} has an invalid value."),
						};
					}
					break;
				case ITranspileWorker.Direction.Backward:
					{
						int newIndex = Range.Start.Value - instructions;
						newRange = behavior switch
						{
							ITranspileWorker.AdvanceBehavior.MoveCursor => newIndex..newIndex,
							ITranspileWorker.AdvanceBehavior.Encompass => newIndex..Range.End,
							_ => throw new ArgumentException($"{nameof(ITranspileWorker.AdvanceBehavior)} has an invalid value."),
						};
					}
					break;
				default:
					throw new ArgumentException($"{nameof(ITranspileWorker.Direction)} has an invalid value.");
			}

			if (newRange.Start.Value < 0 || newRange.End.Value > MutableInstructions.Count)
				throw new TranspileWorkerException("New instruction range is out of bounds.");
			Range = newRange;
			return this;
		}

		public ITranspileWorker Prefix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
		{
			IList<Label> boundaryInstructionLabels = new List<Label>(MutableInstructions[Range.Start].labels);
			if (moveBoundaryLabels)
				MutableInstructions[Range.Start].labels.Clear();
			int extraLength = 0;
			foreach (var instruction in newInstructions)
				MutableInstructions.Insert(Range.Start.Value + extraLength++, instruction);
			Range = Range.Start..new Index(Range.End.Value + extraLength);
			if (moveBoundaryLabels)
				foreach (var label in boundaryInstructionLabels)
					MutableInstructions[Range.Start].labels.Add(label);
			return this;
		}

		public ITranspileWorker Postfix(IEnumerable<CodeInstruction> newInstructions, bool moveBoundaryLabels = false)
		{
			IList<Label> boundaryInstructionLabels = new List<Label>(MutableInstructions[Range.End.Value - 1].labels);
			if (moveBoundaryLabels)
				MutableInstructions[Range.End.Value - 1].labels.Clear();
			int extraLength = 0;
			foreach (var instruction in newInstructions)
				MutableInstructions.Insert(Range.End.Value + extraLength++, instruction);
			Range = Range.Start..new Index(Range.End.Value + extraLength);
			if (moveBoundaryLabels)
				foreach (var label in boundaryInstructionLabels)
					MutableInstructions[Range.End.Value - 1].labels.Add(label);
			return this;
		}

		public ITranspileWorker Replace(
			IEnumerable<CodeInstruction> newInstructions,
			ITranspileWorker.BoundaryLabelReplacementBehavior boundaryLabelBehavior = ITranspileWorker.BoundaryLabelReplacementBehavior.MoveToNewSpot,
			ITranspileWorker.NonBoundaryLabelBehavior nonBoundaryLabelBehavior = ITranspileWorker.NonBoundaryLabelBehavior.Throw
		)
		{
			IList<Label> nonBoundaryLabels = new List<Label>();
			for (int i = Range.Start.Value + 1; i < Range.End.Value - 1; i++)
			{
				if (MutableInstructions[i].labels.Count == 0)
					continue;
				switch (nonBoundaryLabelBehavior)
				{
					case ITranspileWorker.NonBoundaryLabelBehavior.Throw:
						throw new TranspileWorkerException("Found non-boundary labels when replacing instructions.");
					case ITranspileWorker.NonBoundaryLabelBehavior.MoveToStart:
					case ITranspileWorker.NonBoundaryLabelBehavior.MoveToEnd:
						foreach (Label label in MutableInstructions[i].labels)
							nonBoundaryLabels.Add(label);
						break;
					case ITranspileWorker.NonBoundaryLabelBehavior.Drop:
						break;
				}
			}
			IList<Label> startInstructionLabels = new List<Label>(MutableInstructions[Range.Start].labels);
			IList<Label> endInstructionLabels = new List<Label>(MutableInstructions[Range.End.Value - 1].labels);

			if (boundaryLabelBehavior == ITranspileWorker.BoundaryLabelReplacementBehavior.Throw && (startInstructionLabels.Count != 0 || endInstructionLabels.Count != 0))
				throw new TranspileWorkerException("Found boundary labels when replacing instructions.");
			MutableInstructions[Range.Start].labels.Clear();
			MutableInstructions[Range.End.Value - 1].labels.Clear();

			for (int i = Range.Start.Value; i < Range.End.Value; i++)
				MutableInstructions.RemoveAt(Range.Start.Value);
			Range = Range.Start.Value..^Range.Start.Value;

			int extraLength = 0;
			foreach (var instruction in newInstructions)
				MutableInstructions.Insert(Range.End.Value + extraLength++, instruction);
			Range = Range.Start..new Index(Range.End.Value + extraLength);

			if (boundaryLabelBehavior == ITranspileWorker.BoundaryLabelReplacementBehavior.MoveToNewSpot)
			{
				foreach (var label in startInstructionLabels)
					MutableInstructions[Range.Start].labels.Add(label);
				foreach (var label in endInstructionLabels)
					MutableInstructions[Range.End.Value - 1].labels.Add(label);
			}
			switch (nonBoundaryLabelBehavior)
			{
				case ITranspileWorker.NonBoundaryLabelBehavior.Throw:
					throw new InvalidOperationException("Invalid state. This should not happen.");
				case ITranspileWorker.NonBoundaryLabelBehavior.MoveToStart:
					foreach (var label in nonBoundaryLabels)
						MutableInstructions[Range.Start].labels.Add(label);
					break;
				case ITranspileWorker.NonBoundaryLabelBehavior.MoveToEnd:
					foreach (var label in nonBoundaryLabels)
						MutableInstructions[Range.End.Value - 1].labels.Add(label);
					break;
				case ITranspileWorker.NonBoundaryLabelBehavior.Drop:
					break;
			}
			return this;
		}
	}

	internal class LocalLookupTranspileWorker: BasicTranspileWorker, ITranspileWorker.LocalLookup
	{
		protected readonly MethodBase OriginalMethod;

		public LocalLookupTranspileWorker(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod) : base(instructions)
		{
			this.OriginalMethod = originalMethod;
		}

		public int GetLocalIndex(Type type, int skip = 0)
		{
			var body = OriginalMethod.GetMethodBody();
			if (body is not null)
			{
				for (int i = 0; i < body.LocalVariables.Count; i++)
				{
					if (body.LocalVariables[i].LocalType.IsAssignableTo(type))
					{
						if (skip-- <= 0)
							return body.LocalVariables[i].LocalIndex;
					}
				}
			}
			throw new TranspileWorkerException($"Cannot find specific local for type {type.GetBestName()}");
		}
	}
}
