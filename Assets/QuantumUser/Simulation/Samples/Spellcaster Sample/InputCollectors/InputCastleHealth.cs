using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class InputCastleHealth : AIFunction<FP>
  {
    // [Bot SDK Sample]
    // Gets the current Castle Health

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      var castleEntity = frame.GetSingletonEntityRef<Castle>();
      if (castleEntity == default)
        return 0;

      var castleHealth = frame.Unsafe.GetPointer<Health>(castleEntity);
      return castleHealth->Current;
    }
  }
}
