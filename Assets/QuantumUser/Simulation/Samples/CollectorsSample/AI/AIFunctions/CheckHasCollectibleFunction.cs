namespace Quantum
{
	using System;

	[Serializable]
	public class CheckHasCollectibleFunction : AIFunction<bool>
	{
		public override unsafe bool Execute(Frame frame, EntityRef e, ref AIContext aiContext)
		{
			var collector = frame.Unsafe.GetPointer<Collector>(e);
			return collector->HasCollectible == true;
		}
	}
}