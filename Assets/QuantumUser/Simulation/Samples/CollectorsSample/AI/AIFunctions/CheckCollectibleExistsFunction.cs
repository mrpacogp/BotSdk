namespace Quantum
{
	using System;

	[Serializable]
	public class CheckCollectibleExistsFunction : AIFunction<bool>
	{
		public override unsafe bool Execute(Frame frame, EntityRef e, ref AIContext aiContext)
		{
			var collector = frame.Unsafe.GetPointer<Collector>(e);

			bool targetExsits = frame.Exists(collector->DesiredCollectible);
			return targetExsits;
		}
	}
}