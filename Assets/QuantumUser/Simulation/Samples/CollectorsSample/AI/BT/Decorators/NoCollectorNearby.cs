namespace Quantum
{
  [System.Serializable]
  public unsafe class NoCollectorNearby : BTDecorator
  {
    public AIBlackboardValueKey CollectorBlackboardRef;

    public override void OnStartedRunning(BTParams btParams, ref AIContext aiContext)
    {
      base.OnStartedRunning(btParams, ref aiContext);
      btParams.Blackboard->RegisterReactiveDecorator(btParams.Frame, CollectorBlackboardRef.Key, this);
    }

    public override void OnExit(BTParams p, ref AIContext aiContext)
    {
      base.OnExit(p, ref aiContext);
      p.Blackboard->UnregisterReactiveDecorator(p.Frame, CollectorBlackboardRef.Key, this);
    }

    public override bool CheckConditions(BTParams p, ref AIContext aiContext)
    {
      var target = p.Blackboard->GetEntityRef(p.Frame, CollectorBlackboardRef.Key);
      return target == default;
    }
  }
}
