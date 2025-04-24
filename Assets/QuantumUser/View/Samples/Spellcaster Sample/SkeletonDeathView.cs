using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonDeathView : MonoBehaviour
{
  public GameObject[] Remains;
  public float ExplosionForce;
  public float ExplosionRadius;

  private bool _appQuit;

  void OnApplicationQuit()
  {
    _appQuit = true;
  }

  public void OnDeath()
  {
    if (_appQuit == true) return;

    Vector3 position = transform.position;
    for (int i = 0; i < Remains.Length; i++)
    {
      var remain = Instantiate(Remains[i], position, Quaternion.Euler(0, 180, 0));
      remain.GetComponent<Rigidbody>().AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, 1, ForceMode.Impulse);
      position.y += 1.5f;
    }
  }
}
