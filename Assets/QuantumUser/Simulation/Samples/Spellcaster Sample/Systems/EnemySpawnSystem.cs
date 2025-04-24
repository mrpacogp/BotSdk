using Photon.Deterministic;

namespace Quantum
{
  public unsafe class EnemySpawnSystem : SystemMainThread
  {
    // Spawn Enemies based on Commands that comes from Unity
    public override void Update(Frame frame)
    {
      var spawnCommand = frame.GetPlayerCommand(0) as SpawnEnemyCommand;
      if(spawnCommand != null)
      {
        var newEnemy = frame.Create(spawnCommand.EnemyPrototype);
        var enemyTransform = frame.Unsafe.GetPointer<Transform3D>(newEnemy);
        enemyTransform->Position = spawnCommand.SpawnPosition.XOY;
        enemyTransform->Rotation = FPQuaternion.Euler(0, 180, 0);
      }
    }
  }
}
