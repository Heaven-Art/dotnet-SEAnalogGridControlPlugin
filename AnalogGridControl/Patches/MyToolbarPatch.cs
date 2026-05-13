using System;
using HarmonyLib;
using AnanaceDev.AnalogGridControl.Util;
using Sandbox.Game.Entities;

namespace AnanaceDev.AnalogGridControl.Patches
{
  [HarmonyPatch(typeof(MyShipController), "UpdateToolbarBinding")]
  public static class ToolbarBindingPatch
  {
    static bool Prefix(MyShipController __instance)
    {
      if (!__instance.ShouldAnalogInput())
        return true;

      var analogInput = AnalogGridControlSession.Instance?.Input;
      if (analogInput == null)
        return true;
      
      // Prevents the game from detecting and acting on RZ analog input
      if (Math.Abs(analogInput.RotationVector.Z) > 0.05f)
      {
        return false; 
      }

      return true;
    }
  }
}
