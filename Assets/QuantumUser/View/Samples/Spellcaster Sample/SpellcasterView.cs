using UnityEngine;
using Quantum;

public class SpellcasterView : MonoBehaviour
{
  private Animator _animator;

  private void Awake()
  {
    _animator = GetComponentInChildren<Animator>();
  }

  private void OnEnable()
  {
    QuantumEvent.Subscribe<EventOnSpellUsed>(this, OnSpellUsed);
  }

  private void OnDisable()
  {
    QuantumEvent.UnsubscribeListener(this);
  }

  private void OnSpellUsed(EventOnSpellUsed e)
  {
    _animator.SetTrigger("OnAttack");
  }
}
