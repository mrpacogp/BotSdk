using Photon.Deterministic;

namespace Quantum
{
  public unsafe class SpawnEnemyCommand : DeterministicCommand
  {
    // [Bot SDK Sample]
    // Used to, from the Unity side, create deterministic commands for
    // spawning new enemies on the scene

    public AssetRef<EntityPrototype> EnemyPrototype;
    public FPVector2 SpawnPosition;

    public override void Serialize(BitStream stream)
    {
      stream.Serialize(ref EnemyPrototype);
      stream.Serialize(ref SpawnPosition);
    }
  }
}
