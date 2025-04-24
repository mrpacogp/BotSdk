namespace Quantum
{
	using System;

  [Serializable]
  public class AnyCollectibleExistFunction : AIFunction<bool>
  {
    public override unsafe bool Execute(Frame frame, EntityRef e, ref AIContext aiContext)
    {
      var collectiblesCount = frame.ComponentCount<Collectible>();
      return collectiblesCount > 0;
    }
  }
}
