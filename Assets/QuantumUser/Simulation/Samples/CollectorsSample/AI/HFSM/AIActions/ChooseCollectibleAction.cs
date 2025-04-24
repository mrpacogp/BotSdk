namespace Quantum
{
	using System;
  using Photon.Deterministic;
  [Serializable]
	public unsafe partial class ChooseCollectibleAction : AIAction
	{
		public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
		{
			var collectibles = frame.GetComponentIterator<Collectible>();

			var guyTransform = frame.Unsafe.GetPointer<Transform2D>(e);

			EntityRef closestCollectible = default;
			FP min = FP.UseableMax;

			foreach (var (entity, collectible) in collectibles)
			{
				var collTransform = frame.Get<Transform2D>(entity);
				var distance = (guyTransform->Position - collTransform.Position).SqrMagnitude;

				if (closestCollectible == default || distance < min)
				{
					closestCollectible = entity;
					min = distance;
				}
			}

			if (closestCollectible != null)
			{
        frame.Unsafe.GetPointer<Collector>(e)->DesiredCollectible = closestCollectible;
			}
		}
	}
}