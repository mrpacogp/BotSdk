using Photon.Deterministic;
using Quantum.BotSDK;

namespace Quantum
{
  /// <summary>
  /// Update all the Collector entities AI and steering
  /// </summary>
  public unsafe class CollectorsAISystem : SystemMainThreadFilter<CollectorsAISystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Collector* Collector;
      public Bot* Bot;
      public Transform2D* Transform;
      public PhysicsBody2D* PhysicsBody;
    }

    public override void OnInit(Frame frame)
    {
      var bots = frame.RuntimeConfig.CollectorsSampleConfig.Bots;
      for (int i = 0; i < bots.Length; i++)
      {
        var botPrototype = bots[i];
        var botEntity = frame.Create(botPrototype);

        frame.Unsafe.GetPointer<Transform2D>(botEntity)->Position = new FPVector2(i * FP._2, -FP._2);

        if (frame.TryGet<HFSMAgent>(botEntity, out var hfsmAgent) == true)
        {
          BotSDKDebuggerSystem.AddToDebugger(frame, botEntity, hfsmAgent);
        }

        if (frame.TryGet<BTAgent>(botEntity, out var btAgent) == true)
        {
          BotSDKDebuggerSystem.AddToDebugger(frame, botEntity, btAgent);
        }
      }
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      // Depending on the type of AI the entity has, a different AI manager updates it

      if (frame.Has<HFSMAgent>(filter.Entity) == true)
      {
        HFSMManager.Update(frame, frame.DeltaTime, filter.Entity);
      }

      if (frame.Has<BTAgent>(filter.Entity) == true)
      {
        var blackboard = frame.Unsafe.GetPointer<AIBlackboardComponent>(filter.Entity);
        BTManager.Update(frame, filter.Entity, blackboard);
      }

      if (frame.Has<UTAgent>(filter.Entity) == true)
      {
        UTManager.Update(frame, default, filter.Entity);
      }

      // At this point, the AI update already filled the bot input struct
      // So we read it in order to steer the entity
      var physicsBody = filter.PhysicsBody;
      var movementDirection = filter.Bot->Input.Movement.Normalized;
      physicsBody->Velocity = movementDirection * filter.Collector->Speed;

      // Only rotate the entity if it is moving
      if (physicsBody->Velocity != FPVector2.Zero)
      {
        var transform = frame.Unsafe.GetPointer<Transform2D>(filter.Entity);
        transform->Rotation = FPMath.Atan2(-physicsBody->Velocity.X, physicsBody->Velocity.Y);
      }
    }
  }
}
