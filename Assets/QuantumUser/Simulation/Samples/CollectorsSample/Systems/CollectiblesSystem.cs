using Photon.Deterministic;

namespace Quantum
{
  public unsafe class CollectibleSystem : SystemSignalsOnly, ISignalOnTriggerEnter2D
  {
    private const int COLLECTIBLES_COUNT = 12;

    public override void OnInit(Frame frame)
    {
      for (int i = 0; i < COLLECTIBLES_COUNT; i++)
      {
        var collectibleEntity = frame.Create(frame.RuntimeConfig.CollectorsSampleConfig.CollectiblePrototype);

        var transform = frame.Unsafe.GetPointer<Transform2D>(collectibleEntity);
        var x = frame.RNG->Next(-FP._1, FP._1) * 5;
        var y = frame.RNG->Next(-FP._1, FP._1) * 5;
        transform->Position = new FPVector2(x, y);
      }
    }

    public void OnTriggerEnter2D(Frame frame, TriggerInfo2D info)
    {
      var collector = frame.Unsafe.GetPointer<Collector>(info.Entity);

      // If the collision was dynamic, then the character is colliding with a collectible
      if (info.IsStatic == false)
      {
        // Don't to anything if the character already has a collectible
        if (collector->HasCollectible)
          return;

        // Otherwise, destroy the collectible and update the bot data accordingly
        frame.Destroy(info.Other);

        collector->DesiredCollectible = default;
        collector->HasCollectible = true;
      }
      else
      {
        // If the collision was static, then it is the box in which collectibles are delivered
        // If the entity had a collectible, deliver it
        if (collector->HasCollectible)
        {
          collector->HasCollectible = false;
        }
      }
    }
  }
}
