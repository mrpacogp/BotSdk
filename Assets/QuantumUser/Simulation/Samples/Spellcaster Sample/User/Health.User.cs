using Photon.Deterministic;

namespace Quantum
{
  partial struct Health
  {
    public void Init()
    {
      Current = MaxValue;
    }

    public void ChangeHealth(Frame frame, FP amount, EntityRef entity)
    {
      Current += amount;
      if(Current <= 0)
      {
        // If the castle is destroyed, it is game over
        if(frame.Has<Castle>(entity) == true)
        {
          frame.SystemDisable<EnemySpawnSystem>();
          frame.SystemDisable<SpellcasterSystem>();
          frame.SystemDisable<SpellsSystem>();
          frame.SystemDisable<MovementSystem>();
          frame.SystemDisable<SummonersSystem>();
          frame.SystemDisable<SpeedSpellsSystem>();
          frame.SystemDisable<HealthSystem>();
        }
        else
        {
          frame.Destroy(entity);
        }
      }

      if(Current > MaxValue)
      {
        Current = MaxValue;
      }
    }
  }
}
