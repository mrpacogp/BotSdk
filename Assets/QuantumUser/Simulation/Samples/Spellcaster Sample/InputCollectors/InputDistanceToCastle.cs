using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class InputDistanceToCastle : AIFunction<FP>
  {
    // [Bot SDK Sample]
    // Gets the distance from the closest enemy to the Castle
    // It does not matter if the enemy is alone or in a group

    public AIBlackboardValueKey DistanceToCastle;

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      var blackboardComponent = frame.Unsafe.GetPointer<AIBlackboardComponent>(entity);
      return blackboardComponent->GetFP(frame, DistanceToCastle.Key);
    }
  }
}
