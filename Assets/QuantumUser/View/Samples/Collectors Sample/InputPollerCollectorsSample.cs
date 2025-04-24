using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class InputPollerCollectorsSample : MonoBehaviour
{
  private void Start()
  {
    QuantumCallback.Subscribe(this, (CallbackPollInput c) =>
    {
      Quantum.Input i = default;

      FP x = FP.FromFloat_UNSAFE(UnityEngine.Input.GetAxis("Horizontal"));
      FP y = FP.FromFloat_UNSAFE(UnityEngine.Input.GetAxis("Vertical"));
      i.Movement = new FPVector2(x, y).Normalized;

      c.SetInput(i, DeterministicInputFlags.Repeatable);
    });
  }
}
