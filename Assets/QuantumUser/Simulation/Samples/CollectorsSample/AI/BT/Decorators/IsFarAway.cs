using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe class IsFarAway : BTDecorator
  {
    public AIBlackboardValueKey Target;

    public override bool CheckConditions(BTParams p, ref AIContext aiContext)
    {
      var f = p.Frame;
      var e = p.Entity;
      var bb = p.Blackboard;

      var position = f.Get<Transform2D>(e).Position;
      var targetEntity = bb->GetEntityRef(f, Target.Key);
      
      if (f.Exists(targetEntity) == false)
        return false;

      var targetPosition = f.Get<Transform2D>(targetEntity).Position;

      var distance = FPVector2.Distance(position, targetPosition);
      return distance > FP._0_10;
    }
  }
}
