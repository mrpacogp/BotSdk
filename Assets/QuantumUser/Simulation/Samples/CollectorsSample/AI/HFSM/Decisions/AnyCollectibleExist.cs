namespace Quantum
{
	using System;

  [Serializable]
  public partial class AnyCollectibleExist : HFSMDecision
  {
    public override unsafe bool Decide(Frame frame, EntityRef e, ref AIContext aiContext)
    {
      var collectiblesCount = frame.ComponentCount<Collectible>();
      return collectiblesCount > 0;
    }
  }
}
