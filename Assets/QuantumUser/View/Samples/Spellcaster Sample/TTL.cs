using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTL : MonoBehaviour
{
  public float TimeAlive = 1;
  
  private float _timer;

  void Update()
  {
    _timer += Time.deltaTime;
    if(_timer >= TimeAlive)
    {
      Destroy(this.gameObject);
    }
  }
}
