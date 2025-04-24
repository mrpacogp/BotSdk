  using Photon.Deterministic;

namespace Quantum
{
  [System.Serializable]
  public unsafe partial class ScanAction : AIAction
  {
    // [Bot SDK Sample]
    // Used to gather information about the game environment
    // It stores relevant data into the blackboard so the Agent can just read it in order
    // to make decisions

    // What is the entity that is the closest to the Castle?
    public AIBlackboardValueKey TargetEntity;

    // Considering that entity, how big is the agglomeration of other entities around it?
    public AIBlackboardValueKey AgglomerationCount;

    // What is the sum of the health points on that agglomeration?
    public AIBlackboardValueKey HealthsSum;

    // What is the distance of that enemy/agglomeration to the Castle?
    public AIBlackboardValueKey DistanceToCastle;

    // The fixed Z position of the castle, just for distance calculation purposes
    private readonly FP CastleZPos = -(FP._8 + FP._0_50);

		// dummy update method to avoid warning
		public void Update()
		{
		}

		public override void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
    {
      var blackboardComponent = frame.Unsafe.GetPointer<AIBlackboardComponent>(e);

      var closestEnemy = FindClosestEnemy(frame);
      // If we don't find any enemy around, then we can reset all data
      if (closestEnemy == default)
      {
        blackboardComponent->Set(frame, TargetEntity.Key, default(EntityRef));
        blackboardComponent->Set(frame, AgglomerationCount.Key, default(int));
        blackboardComponent->Set(frame, HealthsSum.Key, default(FP));
        blackboardComponent->Set(frame, DistanceToCastle.Key, default(FP));
        return;
      }

      Scan(frame, closestEnemy, out var enemiesCount, out var healthsSum, out var distanceToBase);

      blackboardComponent->Set(frame, TargetEntity.Key, closestEnemy);
      blackboardComponent->Set(frame, AgglomerationCount.Key, enemiesCount);
      blackboardComponent->Set(frame, HealthsSum.Key, healthsSum);
      blackboardComponent->Set(frame, DistanceToCastle.Key, distanceToBase);
    }

    private EntityRef FindClosestEnemy(Frame frame)
    {
      var allEnemies = frame.Filter<Fighter, Transform3D>();
      var lowestDistance = FP.UseableMax;
      EntityRef closestEntity = default;
      while(allEnemies.NextUnsafe(out var entity, out var enemy, out var transform))
      {
        var distance = FPMath.Abs(transform->Position.Z - CastleZPos);
        if(distance < lowestDistance)
        {
          lowestDistance = distance;
          closestEntity = entity;
        }
      }

      return closestEntity;
    }

    private void Scan(Frame frame, EntityRef targetEntity, out int enemiesCount, out FP healthsSum, out FP distanceToCastle)
    {
      enemiesCount = default;
      healthsSum = default;

      var targetPosition = frame.Get<Transform3D>(targetEntity).Position;
      var hits = frame.Physics3D.OverlapShape(targetPosition, FPQuaternion.Identity, Shape3D.CreateSphere(5));
      for (int i = 0; i < hits.Count; i++)
      {
        var hit = hits[i];
        if (frame.Has<Fighter>(hit.Entity) == false)
          continue;

        enemiesCount += 1;
        healthsSum += frame.Get<Health>(hit.Entity).Current;
      }

      distanceToCastle = FPMath.Abs(targetPosition.Z - CastleZPos);
    }
  }
}
