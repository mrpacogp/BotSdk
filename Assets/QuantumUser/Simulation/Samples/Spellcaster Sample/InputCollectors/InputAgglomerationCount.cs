using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class InputAgglomerationCount : AIFunction<FP>
  {
    // [Bot SDK Sample]
    // Returns the amount of enemies that are too close to the target entity
    // "How big is that agglomeration?"

    public AIBlackboardValueKey AgglomerationCount;

    public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
    {
      var blackboardComponent = frame.Unsafe.GetPointer<AIBlackboardComponent>(entity);
      return blackboardComponent->GetInteger(frame, AgglomerationCount.Key);
    }
  }
}
