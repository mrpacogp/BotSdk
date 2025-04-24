using Quantum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellView : MonoBehaviour
{
  public GameObject ImpactVFX;

  public void OnExplosion(QuantumGame quantumGame)
  {
    Instantiate(ImpactVFX, transform.position, Quaternion.identity);
  }
}
