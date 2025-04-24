using System.Collections.Generic;
using UnityEditor;
using Circuit;
using Photon.Deterministic;
using Quantum;

[InitializeOnLoad()]
public static class CircuitWindowUnityCallbacks
{
  static CircuitWindowUnityCallbacks()
  {
    CircuitWindow.Callbacks.OnRegisterAIFunctionTyps += OnRegisterAIFunctionTyps;
  }

  private static void OnRegisterAIFunctionTyps(List<Typ> aiFunctions)
  {
    aiFunctions.AddRange(
      new List<Typ>()
      {
        Typ.Create(typeof(AIFunction<bool>)),
        Typ.Create(typeof(AIFunction<byte>)),
        Typ.Create(typeof(AIFunction<int>)),
        Typ.Create(typeof(AIFunction<FP>)),
        Typ.Create(typeof(AIFunction<FPVector2>)),
        Typ.Create(typeof(AIFunction<FPVector3>)),
        Typ.Create(typeof(AIFunction<EntityRef>)),
        Typ.Create(typeof(AIFunction<string>)),
        Typ.Create(typeof(AIFunction<AssetRef>))
        // really call the Typ creating stuff from here? Or delegate this to the ai editor code?
      });
  }
}
