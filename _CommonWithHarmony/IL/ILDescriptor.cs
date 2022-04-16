using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Shockah.CommonModCode.IL
{
	public interface IILDescriptor: IEquatable<IILDescriptor>
	{
		bool Matches(CodeInstruction instruction);
	}

	public readonly struct OpcodeILDescriptor: IILDescriptor
	{
		public readonly OpCode OpCode;

		public OpcodeILDescriptor(OpCode opcode)
		{
			this.OpCode = opcode;
		}

		public override bool Equals(object? obj)
			=> obj is IILDescriptor descriptor && Equals(descriptor);

		public bool Equals(IILDescriptor? other)
			=> other is OpcodeILDescriptor descriptor && descriptor.OpCode == this.OpCode;

		public override int GetHashCode()
			=> OpCode.GetHashCode();

		public override string ToString()
			=> $"`opcode` == {OpCode}";

		public bool Matches(CodeInstruction instruction)
			=> instruction.opcode == OpCode;

		public static bool operator ==(OpcodeILDescriptor left, OpcodeILDescriptor right)
			=> left.Equals(right);

		public static bool operator !=(OpcodeILDescriptor left, OpcodeILDescriptor right)
			=> !left.Equals(right);
	}

	public readonly struct CallILDescriptor: IILDescriptor
	{
		public readonly MethodInfo? Method;

		public CallILDescriptor(MethodInfo? method = null)
		{
			this.Method = method;
		}

		public override bool Equals(object? obj)
			=> obj is IILDescriptor descriptor && Equals(descriptor);

		public bool Equals(IILDescriptor? other)
			=> other is CallILDescriptor descriptor && descriptor.Method == Method;

		public override int GetHashCode()
			=> Method?.GetHashCode() ?? 0;

		public override string ToString()
			=> Method is null ? "Calls any method" : $"Calls method {Method}";

		public bool Matches(CodeInstruction instruction)
			=> Method is null
				? instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt
				: instruction.Calls(Method!);

		public static bool operator ==(CallILDescriptor left, CallILDescriptor right)
			=> left.Equals(right);

		public static bool operator !=(CallILDescriptor left, CallILDescriptor right)
			=> !left.Equals(right);
	}
}
