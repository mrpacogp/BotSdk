namespace Quantum
{
  public unsafe class HealthSystem : SystemSignalsOnly, ISignalOnComponentAdded<Health>
  {
    public void OnAdded(Frame frame, EntityRef entity, Health* health)
    {
      health->Init();
    }
  }
}
