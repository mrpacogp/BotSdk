namespace Quantum
{
  public unsafe class SpeedSpellsSystem : SystemMainThreadFilter<SpeedSpellsSystem.Filter>, ISignalOnTrigger3D
  {
    public struct Filter
    {
      public EntityRef Entity;
      public SpeedSpell* SpeedSpell;
    }

    public void OnTrigger3D(Frame frame, TriggerInfo3D info)
    {
      if (frame.Has<Movement>(info.Other) == false || frame.Unsafe.TryGetPointer<SpeedSpell>(info.Entity, out var speedSpell) == false) return;

      if (frame.Has<SpeedModifier>(info.Other) == true) return;

      frame.Add(info.Other, new SpeedModifier { Multiplier = speedSpell->SpeedMultiplier, Time = speedSpell->MultiplierDuration });

      frame.Events.OnEnemyFreeze(info.Other, speedSpell->SpellType);
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      filter.SpeedSpell->TTL -= frame.DeltaTime;
      if(filter.SpeedSpell->TTL <= 0)
      {
        frame.Destroy(filter.Entity);
      }
    }
  }
}
