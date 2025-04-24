using Quantum;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
  public GameObject SpawnEffect;

  public GameObject FrozenEffect;
  public GameObject SwampEffect;

  private void OnEnable()
  {
    QuantumEvent.Subscribe<EventOnEnemyFreeze>(this, OnEnemyFreeze);
  }

  private void OnDisable()
  {
    QuantumEvent.UnsubscribeListener(this);
  }

  public void OnInstantiated()
  {
    Instantiate(SpawnEffect, transform.position, Quaternion.identity);
  }

  private void OnEnemyFreeze(EventOnEnemyFreeze ev)
  {
    if (GetComponent<QuantumEntityView>().EntityRef != ev.Entity)
      return;
   
    if(ev.SpellType == ESpell.Ice)
    {
      GameObject frozenEffect = Instantiate(FrozenEffect, this.transform);
      frozenEffect.transform.localPosition = Vector3.zero;
    }

    if (ev.SpellType == ESpell.Swamp)
    {
      GameObject swampEffect = Instantiate(SwampEffect, this.transform);
      swampEffect.transform.localPosition = Vector3.zero;
    }
  }
}
