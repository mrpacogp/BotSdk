using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
  public QuantumEntityView EntityView;

  public Image FilledBar;

  void Update()
  {
    if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null)
      return;

    var frame = QuantumRunner.Default.Game.Frames.Predicted;

    if (frame == null)
      return;

    var health = frame.Get<Health>(EntityView.EntityRef);
    FilledBar.fillAmount = (health.Current / health.MaxValue).AsFloat;
  }
}
