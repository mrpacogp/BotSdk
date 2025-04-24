using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe class SampleInput : AIFunction<FP>
  {
    // Inherit from AIFunction<FP> to create your Input assets
    // Implement the Execute method. Return any desired input, which will then be inserted into
    // the FPAnimationCurve in order to get the utility of such curve

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      return FP._0;
    }
  }
}
