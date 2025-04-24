﻿namespace Quantum.BotSDK
{
  using System;
  using UnityEngine.Scripting;

  /// <summary>
  /// Using this system is optional. It is only used to aim the Debugger on the Unity side.
  /// It is also safe to copy logic from this system into your own systems, if it better suits your architecture.
  /// </summary>
  [Preserve]
  public class BotSDKDebuggerSystem : SystemMainThread
	{
    // ========== PUBLIC METHODS ==================================================================================

    /// <summary>
    /// Use this to add an entity to the Debugger Window on Unity.
    /// You can provide a custom label of your preference if you want to identify your bots in a custom way.
    /// Use the separator '/' on the custom label if you want to create an Hierarchy on the Debugger Window.
    /// </summary>
    public static void AddToDebugger<T>(Frame frame, EntityRef entity, T component, string customLabel = default)
      where T : IComponent, IBotSDKDebugInfoProvider
    {
      var componentIndex = ComponentTypeId.GetComponentIndex(typeof(T));
      Func<DelegateGetDebugInfo> debugInfoGetter = (component as IBotSDKDebugInfoProvider).GetDebugInfo;

      if (BotSDKDebuggerSystemCallbacks.SetEntityDebugLabel != null)
      {
        BotSDKDebuggerSystemCallbacks.SetEntityDebugLabel(frame, entity, customLabel, componentIndex, debugInfoGetter);
      }
    }

    // ========== SystemMainThread INTERFACE ======================================================================

    public override void Update(Frame frame)
		{
			if (frame.IsVerified)
			{
        BotSDKDebuggerSystemCallbacks.OnVerifiedFrame?.Invoke(frame);
			}
		}
	}
}
