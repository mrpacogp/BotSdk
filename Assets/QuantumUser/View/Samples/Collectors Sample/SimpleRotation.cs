using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
  public float RotationFactor = 1;

  // Update is called once per frame
  void Update()
  {
    transform.Rotate(0, RotationFactor * Time.deltaTime, 0);
  }
}
