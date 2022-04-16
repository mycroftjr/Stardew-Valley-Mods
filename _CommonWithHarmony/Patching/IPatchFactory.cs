namespace Shockah.CommonModCode.Patching
{
	public interface IPatchFactory<Args>
	{
		IPatch CreatePatch(Args args);
	}
}