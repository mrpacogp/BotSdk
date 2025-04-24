using Photon.Deterministic;
using Quantum.Core;

namespace Quantum
{
  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, IKCCCallbacks3D, ISignalOnEntityPrototypeMaterialized
  {
    public struct Filter
    {
      public EntityRef Entity;
      public CharacterController3D* KCC;
      public Transform3D* Transform;
      public Movement* Movement;
    }

    public void OnEntityPrototypeMaterialized(Frame frame, EntityRef entity, EntityPrototypeRef prototypeRef)
    {
      if (frame.Unsafe.TryGetPointer<CharacterController3D>(entity, out var kcc) == true
          && frame.Unsafe.TryGetPointer<Movement>(entity, out var movement) == true)
      {
        movement->OriginalSpeed = kcc->MaxSpeed;
      }
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      if (frame.Unsafe.TryGetPointer<SpeedModifier>(filter.Entity, out var speedModifier) == true)
      {
        filter.KCC->MaxSpeed = filter.Movement->OriginalSpeed * speedModifier->Multiplier;
        speedModifier->Time -= frame.DeltaTime;
        if(speedModifier->Time <= 0)
        {
          frame.Remove<SpeedModifier>(filter.Entity);
          filter.KCC->MaxSpeed = filter.Movement->OriginalSpeed;
        }
      }

      filter.KCC->Move(frame, filter.Entity, FPVector3.Back, this);
    }

    public bool OnCharacterCollision3D(FrameBase frame, EntityRef character, Physics3D.Hit3D hit)
    {
      return true;
    }

    public void OnCharacterTrigger3D(FrameBase frame, EntityRef character, Physics3D.Hit3D hit)
    {
      if(hit.IsDynamic == true && frame.Has<Castle>(hit.Entity) == true)
      {
        var damage = frame.Get<Fighter>(character).Power;
        frame.Unsafe.GetPointer<Health>(character)->ChangeHealth((Frame)frame, -100, character);
        frame.Unsafe.GetPointer<Health>(hit.Entity)->ChangeHealth((Frame)frame, -damage, hit.Entity);
      }
    }
  }
}
