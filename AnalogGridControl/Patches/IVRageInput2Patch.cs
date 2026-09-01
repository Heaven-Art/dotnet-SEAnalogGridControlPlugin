using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRage.Input;

namespace AnanaceDev.AnalogGridControl.Patches
{
    public static class ModernInputNukePatch
    {
        public static void ApplyPatch(Harmony harmonyInstance)
        {
            // Locate every class in the current execution space that implements IVRageInput2
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Safely skip dynamic or system assemblies to prevent loading exceptions
                if (assembly.IsDynamic) continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(IVRageInput2).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            // We found a concrete input engine class! Now target its methods:
                            var connectedMethod = type.GetMethod(nameof(IVRageInput2.IsJoystickConnected));
                            var stateMethod = type.GetMethod(nameof(IVRageInput2.GetJoystickState));
                            var namesMethod = type.GetMethod(nameof(IVRageInput2.EnumerateJoystickNames));

                            // Bind our custom interceptors
                            var prefixFalse = new HarmonyMethod(typeof(ModernInputNukePatch).GetMethod(nameof(PrefixJoystickConnected)));
                            var prefixState = new HarmonyMethod(typeof(ModernInputNukePatch).GetMethod(nameof(PrefixGetJoystickState)));
                            var prefixNames = new HarmonyMethod(typeof(ModernInputNukePatch).GetMethod(nameof(PrefixEnumerateNames)));

                            if (connectedMethod != null)
                                harmonyInstance.Patch(connectedMethod, prefix: prefixFalse);
                                
                            if (stateMethod != null)
                                harmonyInstance.Patch(stateMethod, prefix: prefixState);

                            if (namesMethod != null)
                                harmonyInstance.Patch(namesMethod, prefix: prefixNames);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assemblies that fail to load type definitions (common in modded environments)
                }
            }
        }

        // Force the game engine to believe no joysticks are physically connected
        public static bool PrefixJoystickConnected(ref bool __result)
        {
            __result = false;
            return false; // Skips the real VRAGE hardware checking execution entirely
        }

        // Return a completely empty string list if the game tries to search for devices
        public static bool PrefixEnumerateNames(ref List<string> __result)
        {
            __result = new List<string>();
            return false; // Suppresses native enumeration loops
        }

        public static bool PrefixGetJoystickState(ref MyJoystickState state)
        {
          return false; 
        }
    }
}
