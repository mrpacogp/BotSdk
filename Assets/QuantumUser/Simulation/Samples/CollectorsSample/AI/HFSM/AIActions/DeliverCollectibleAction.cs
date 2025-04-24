namespace Quantum
{
  using System;
  using Photon.Deterministic;

  [Serializable]
  public unsafe partial class DeliverCollectibleAction : AIAction
  {
    private FPVector2 _deliveryPos = new FPVector2(0, 5);

    public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
    {
      var bot = frame.Unsafe.GetPointer<Bot>(e);
      var guyPosition = frame.Get<Transform2D>(e).Position;

      bot->Input.Movement = _deliveryPos - guyPosition;
    }
  }
}