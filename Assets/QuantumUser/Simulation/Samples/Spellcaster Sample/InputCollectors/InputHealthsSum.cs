using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class InputHealthsSum : AIFunction<FP>
  {
    // [Bot SDK Sample]
    // Based on the position of the closest enemy, gets the sum of the health values
    // from that enemy and the ones surrounding it

    public AIBlackboardValueKey HealthsSum;

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      var blackboardComponent = frame.Unsafe.GetPointer<AIBlackboardComponent>(entity);
      return blackboardComponent->GetFP(frame, HealthsSum.Key);
    }
  }
}
