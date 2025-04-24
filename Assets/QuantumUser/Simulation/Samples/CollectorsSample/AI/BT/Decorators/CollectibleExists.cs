using System;

namespace Quantum
{
  [Serializable]
  public unsafe partial class CollectibleExists : BTDecorator
  {
    public override Boolean CheckConditions(BTParams p, ref AIContext aiContext)
    {
      var f = p.Frame;
      var e = p.Entity;

      var collector = f.Unsafe.GetPointer<Collector>(e);

      bool targetExsits = f.Exists(collector->DesiredCollectible);
      return targetExsits;
    }
  }
}
