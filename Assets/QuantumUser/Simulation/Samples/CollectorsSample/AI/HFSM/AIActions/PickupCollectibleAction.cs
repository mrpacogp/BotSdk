namespace Quantum
{
	using System;

	[Serializable]
	public unsafe partial class PickupCollectibleAction : AIAction
	{
		public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
		{
			var bot = frame.Unsafe.GetPointer<Bot>(e);
			var collector = frame.Unsafe.GetPointer<Collector>(e);
			var guyPosition = frame.Get<Transform2D>(e).Position;

			if (frame.Exists(collector->DesiredCollectible) == false)
			{
				return;
			}

			var target = collector->DesiredCollectible;
			var targetPosition = frame.Get<Transform2D>(target).Position;

			if (target != default)
			{
				bot->Input.Movement = targetPosition - guyPosition;
			}
		}
	}
}