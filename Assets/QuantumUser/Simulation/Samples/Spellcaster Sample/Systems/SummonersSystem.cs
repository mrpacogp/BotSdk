using Photon.Deterministic;

namespace Quantum
{
  public unsafe class SummonersSystem : SystemMainThreadFilter<SummonersSystem.Filter>, ISignalOnComponentAdded<Summoner>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Summoner* Summoner;
      public Transform3D* Transform;
    }

    public void OnAdded(Frame frame, EntityRef entity, Summoner* summoner)
    {
      summoner->Timer = summoner->Interval;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      var summoner = filter.Summoner;

      summoner->Timer -= frame.DeltaTime;
      if (summoner->Timer <= 0)
      {
        summoner->Timer = summoner->Interval;

        for (int i = 0; i < 4; i++)
        {
          var radians = 2 * FP.Pi / 4 * i;

          var vertical = FPMath.Sin(radians);
          var horizontal = FPMath.Cos(radians);

          var spawnDir = new FPVector3(horizontal, 0, vertical);

          var spawnPos = filter.Transform->Position + spawnDir * 2;

          var newCreature = frame.Create(summoner->ToSummon);
          var creatureTransform = frame.Unsafe.GetPointer<Transform3D>(newCreature);
          creatureTransform->Position = spawnPos;
          creatureTransform->Rotation = FPQuaternion.Euler(0, 180, 0);
        }
      }
    }
  }
}
