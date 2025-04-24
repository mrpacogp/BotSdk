using System;

namespace Quantum
{
	[Serializable]
	public unsafe partial class PickupCollectible : BTLeaf
	{
		protected override BTStatus OnUpdate(BTParams p, ref AIContext aiContext)
		{
      var f = p.Frame;
      var e = p.Entity;

      var bot = f.Unsafe.GetPointer<Bot>(e);
      var collector = f.Unsafe.GetPointer<Collector>(e);
      var position = f.Get<Transform2D>(e).Position;

      // If the Bot is Loaded, it already has a collectible
      // Bot Succeeded
      if(collector->HasCollectible == true)
      {
        return BTStatus.Success;
      }

      if (f.Exists(collector->DesiredCollectible) == false)
      {
        return BTStatus.Failure;
      }

      // Otherwise, the Bot is still trying, so Bot is Running
      var target = collector->DesiredCollectible;
      var targetPosition = f.Get<Transform2D>(target).Position;

      bot->Input.Movement = (targetPosition - position).Normalized;
      
      return BTStatus.Running;
    }
	}
}
