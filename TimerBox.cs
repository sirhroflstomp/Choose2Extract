using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Choose2Extract
{
    /// <summary>
    /// The countdown panel is shared with transits (orange). While the extract hold runs, its box
    /// is painted in the extraction timer's color, and put back afterwards.
    /// </summary>
    internal static class TimerBox
    {
        private static readonly Color FallbackColor = new Color32(0x7C, 0xB7, 0x04, 0xDC);
        private static readonly Dictionary<int, Color> OriginalColors = new Dictionary<int, Color>();

        private static Type _imageType;
        private static PropertyInfo _colorProperty;
        private static bool _failed;

        public static void Apply(Component panel, bool extractStyle)
        {
            if (_failed || panel == null || !FindImageType()) return;

            try
            {
                foreach (var image in panel.GetComponentsInChildren(_imageType, true))
                {
                    int id = image.GetInstanceID();
                    var current = (Color)_colorProperty.GetValue(image, null);

                    if (!OriginalColors.TryGetValue(id, out var original))
                    {
                        // Only the colored box is touched, not the white icon and glow
                        if (!IsColored(current)) continue;
                        original = current;
                        OriginalColors[id] = current;
                    }

                    _colorProperty.SetValue(image, extractStyle ? GetBoxColor(original) : original, null);
                }
            }
            catch (Exception e)
            {
                _failed = true;
                Plugin.Log.LogWarning($"Could not recolor the hold timer: {e.Message}");
            }
        }

        private static Color GetBoxColor(Color original)
        {
            string value = Settings.HoldBoxColor.Value ?? "";

            if (!ColorUtility.TryParseHtmlString(value, out var color)) return FallbackColor;

            bool hasAlpha = value.TrimStart('#').Length == 8;
            if (!hasAlpha) color.a = original.a;
            return color;
        }

        private static bool IsColored(Color color)
        {
            Color.RGBToHSV(color, out _, out float saturation, out float value);
            return color.a > 0.05f && saturation > 0.35f && value > 0.3f;
        }

        // UnityEngine.UI isn't referenced by the project, so the Image type is looked up at runtime
        private static bool FindImageType()
        {
            if (_imageType != null) return true;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _imageType = assembly.GetType("UnityEngine.UI.Image", false);
                if (_imageType != null) break;
            }

            _colorProperty = _imageType?.GetProperty("color");
            return _imageType != null && _colorProperty != null;
        }
    }
}
