namespace Quantum
{
	using System;

	[Serializable]
	public partial class CheckCollectibleExists : HFSMDecision
	{
		public override unsafe bool Decide(Frame frame, EntityRef e, ref AIContext aiContext)
		{
			var collector = frame.Unsafe.GetPointer<Collector>(e);
			return frame.Exists(collector->DesiredCollectible);
		}
	}
}