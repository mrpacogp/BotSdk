using Photon.Deterministic;
using System;

namespace Quantum
{
  [Serializable]
  public unsafe partial class DeliverCollectible : BTLeaf
  {
    protected override BTStatus OnUpdate(BTParams p, ref AIContext aiContext)
    {
      var f = p.Frame;
      var e = p.Entity;

      var bot = f.Unsafe.GetPointer<Bot>(e);
      var collector = f.Unsafe.GetPointer<Collector>(e);

      if (collector->HasCollectible == false)
      {
        bot->Input.Movement = default;
        return BTStatus.Success;
      }

      var guyPosition = f.Get<Transform2D>(e).Position;
      var deliverPosition = new FPVector2(0, 5);
      bot->Input.Movement = deliverPosition - guyPosition;

      return BTStatus.Running;
    }
  }
}
