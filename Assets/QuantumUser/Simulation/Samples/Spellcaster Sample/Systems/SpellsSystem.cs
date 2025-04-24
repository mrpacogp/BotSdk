using Photon.Deterministic;

namespace Quantum
{
  public unsafe class SpellsSystem : SystemMainThreadFilter<SpellsSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Transform3D* Transform;
      public Spell* Spell;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      // We check if the Spell has a target entity set
      // If not, this means that it's target entity was already destroyed
      // In this case, we find another one to be the new target so the spell is not wasted
      // If we don't find any target, then the spell is destroyed
      var targetEntity = filter.Spell->TargetEntity;

      if (frame.Exists(targetEntity) == false)
      {
        targetEntity = FindClosestEnemy(frame);

        if(targetEntity == default)
        {
          frame.Destroy(filter.Entity);
        }
        else
        {
          filter.Spell->TargetEntity = targetEntity;
        }
        return;
      }

      var targetPosition = frame.Get<Transform3D>(targetEntity).Position;
      filter.Transform->Position = FPVector3.MoveTowards(filter.Transform->Position, targetPosition, frame.DeltaTime * filter.Spell->Speed);
      if(FPVector3.Distance(filter.Transform->Position, targetPosition) < FP.Epsilon)
      {
        ProjectileImpact(frame, filter.Entity, filter.Spell, filter.Transform);
      }
    }

    private EntityRef FindClosestEnemy(Frame frame)
    {
      var allEnemies = frame.Filter<Fighter, Transform3D>();
      var lowestDistance = FP.UseableMax;
      EntityRef closestEntity = default;
      while (allEnemies.NextUnsafe(out var entity, out var enemy, out var transform))
      {
        var distance = FPMath.Abs(transform->Position.Z - (FP._8 + FP._0_50));
        if (distance < lowestDistance)
        {
          lowestDistance = distance;
          closestEntity = entity;
        }
      }

      return closestEntity;
    }

    private void ProjectileImpact(Frame frame, EntityRef spellEntity, Spell* spell, Transform3D* transform)
    {
      // Damage to the spell's target
      var targetEntity = spell->TargetEntity;
      if (targetEntity != default)
      {
        var targetHealth = frame.Unsafe.GetPointer<Health>(targetEntity);
        targetHealth->ChangeHealth(frame, spell->ApplyToTarget, targetEntity);
      }

      // Damage to other targets in the area
      var hits = frame.Physics3D.OverlapShape(transform->Position, FPQuaternion.Identity, Shape3D.CreateSphere(spell->Radius));
      for (int i = 0; i < hits.Count; i++)
      {
        var hit = hits[i];
        if (hit.Entity == targetEntity)
          continue;

        if (frame.Unsafe.TryGetPointer<Health>(hit.Entity, out var health) == true && frame.Has<Castle>(hit.Entity) == false)
        {
          health->ChangeHealth(frame, spell->ApplyToOthers, hit.Entity);
        }
      }

      HandleSpellSequence(frame, spell, transform);

      frame.Destroy(spellEntity);
    }

    // A spell can create another one as part of it's effect
    // This is how a spell creates a slow area, for example
    private void HandleSpellSequence(Frame frame, Spell* spell, Transform3D* transform)
    {
      var spellSequence = spell->SpellSequence;
      if (spellSequence == default)
        return;

      var effectEntity = frame.Create(spellSequence);
      frame.Unsafe.GetPointer<Transform3D>(effectEntity)->Position = transform->Position;
    }
  }
}
