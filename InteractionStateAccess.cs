using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;

namespace Choose2Extract
{
    /// <summary>
    /// Reads and writes the interaction state of the player owner. It lives in a bindable
    /// state object whose accessors aren't public, hence the reflection.
    /// </summary>
    internal static class InteractionStateAccess
    {
        private static readonly FieldInfo HolderField =
            AccessTools.Field(typeof(GamePlayerOwner), "AvailableInteractionState");

        private static bool _resolved;
        private static MethodInfo _getter;
        private static MethodInfo _setter;
        private static FieldInfo _valueField;

        public static AvailableInteractionState Read(GamePlayerOwner owner)
        {
            var holder = HolderField.GetValue(owner);
            if (holder == null) return null;

            Resolve(holder);
            try
            {
                if (_getter != null) return _getter.Invoke(holder, null) as AvailableInteractionState;
                return _valueField?.GetValue(holder) as AvailableInteractionState;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Could not read the interaction state: {e.Message}");
                return null;
            }
        }

        public static void Write(GamePlayerOwner owner, AvailableInteractionState state)
        {
            var holder = HolderField.GetValue(owner);
            if (holder == null) return;

            Resolve(holder);
            if (_setter == null)
            {
                Plugin.Log.LogError("The interaction state can't be written, the prompt won't show");
                return;
            }

            try
            {
                _setter.Invoke(holder, new object[] { state });
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Could not write the interaction state: {e.Message}");
            }
        }

        private static void Resolve(object holder)
        {
            if (_resolved) return;
            _resolved = true;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var type = holder.GetType(); type != null && type != typeof(object); type = type.BaseType)
            {
                var property = type.GetProperty("Value", flags);
                if (property != null)
                {
                    _getter = _getter ?? property.GetGetMethod(true);
                    _setter = _setter ?? property.GetSetMethod(true);
                }

                _valueField = _valueField ??
                    type.GetFields(flags).FirstOrDefault(f => f.FieldType == typeof(AvailableInteractionState));
            }
        }
    }
}
