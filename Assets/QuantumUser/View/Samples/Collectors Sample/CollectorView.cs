using Quantum;
using UnityEngine;

public unsafe class CollectorView : MonoBehaviour
{
  public float AnimSpeedFactor = 0.5f;

  private UnityEngine.Animator _anim;
  private QuantumEntityView _view;

  // Use this for initialization
  void Start()
  {
    _anim = GetComponentInChildren<UnityEngine.Animator>();
    _view = GetComponent<QuantumEntityView>();
  }

  // Update is called once per frame
  void Update()
  {
    var body = QuantumRunner.Default.Game.Frames.Predicted.Get<PhysicsBody2D>(_view.EntityRef);
    float speed = body.Velocity.Magnitude.AsFloat;
    _anim.SetFloat("Speed", speed);
    if (speed > 0.1f)
      _anim.speed = speed * AnimSpeedFactor;
    else
      _anim.speed = 1;
  }
}
