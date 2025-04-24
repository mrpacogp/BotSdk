using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputSpellcaster : MonoBehaviour
{
  private UnityEngine.Plane _plane;

  private void Start()
  {
    QuantumCallback.Subscribe(this, (CallbackPollInput c) =>
    {
      Quantum.Input i = default;
      c.SetInput(i, DeterministicInputFlags.Repeatable);
    });

    _plane = new UnityEngine.Plane(Vector3.up, Vector3.zero);
  }

  private void Update()
  {
    if (EventSystem.current.IsPointerOverGameObject() == false &&  UnityEngine.Input.GetMouseButtonDown(0) == true)
    {
      var mousePosition = GetPositionFromMouse();
      var enemyProtoype = EnemyPickerUI.Instance.GetSelectedPrefab();
      SpawnEnemyCommand cmd = new SpawnEnemyCommand { EnemyPrototype = enemyProtoype, SpawnPosition = mousePosition };
      QuantumRunner.Default.Game.SendCommand(cmd);
    }

    FPVector2 GetPositionFromMouse()
    {
      Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

      float enter = 0;

      if (_plane.Raycast(ray, out enter))
      {
        return ray.GetPoint(enter).ToFPVector2();
      }

      return default;
    }
  }
}
