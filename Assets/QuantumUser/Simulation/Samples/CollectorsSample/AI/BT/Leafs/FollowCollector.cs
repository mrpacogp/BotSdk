using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe class FollowCollector : BTLeaf
  {
    public AIBlackboardValueKey CollectorBlackboardRef;

    protected override BTStatus OnUpdate(BTParams p, ref AIContext aiContext)
    {
      var f = p.Frame;
      var e = p.Entity;
      var bb = p.Blackboard;

      var bot = f.Unsafe.GetPointer<Bot>(e);
      var guyPosition = f.Get<Transform2D>(e).Position;

      var targetEntity = bb->GetEntityRef(f, CollectorBlackboardRef.Key);

      if (targetEntity == default)
      {
        return BTStatus.Failure;
      }
      else
      {
        var targetPosition = f.Get<Transform2D>(targetEntity).Position;
        bot->Input.Movement = (targetPosition - guyPosition).Normalized;

        var distance = FPVector2.Distance(guyPosition, targetPosition);
        if (distance < FP._0_10)
        {
          return BTStatus.Success;
        }
        else
        {
          return BTStatus.Running;
        }
      }
    }

    public override void OnExit(BTParams p, ref AIContext aiContext)
    {
      var bot = p.Frame.Unsafe.GetPointer<Bot>(p.Entity);
      bot->Input.Movement = default;

      var blackboard = p.Blackboard;
      var targetEntity = blackboard->Set(p.Frame, CollectorBlackboardRef.Key, default(EntityRef));
    }
  }
}
