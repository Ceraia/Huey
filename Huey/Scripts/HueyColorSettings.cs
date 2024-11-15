using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "ColorSettings", menuName = "Huey/Color Settings")]
    public class HueyColorSettings : ScriptableObject
    {
        [System.Serializable]
        public struct ColorSettings
        {
            public string name;
            [Range(0, 360)] public float hueShift;
            [Range(0, 1)] public float saturationScale;
            [Range(-100, 100)] public float lightnessPercent;
        }

        public ColorSettings[] colors;
    }
}
