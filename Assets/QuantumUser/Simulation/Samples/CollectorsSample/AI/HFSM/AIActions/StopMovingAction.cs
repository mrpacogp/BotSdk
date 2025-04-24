namespace Quantum
{
	using System;
	using Photon.Deterministic;

	[Serializable]
	public unsafe partial class StopMovingAction : AIAction
	{
		public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
		{
      if(frame.Unsafe.TryGetPointer<Bot>(e, out var bot) == true)
      {
			  bot->Input.Movement = FPVector2.Zero;
      }
		}
	}
}
