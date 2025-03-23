using System.Reflection;
using UnityEngine;

namespace dark_cheat
{
    public class FOVEditor : MonoBehaviour
    {
        private float fovValue = 70f;
        private bool fovEnabled = true;

        void Update()
        {
            if (!fovEnabled) return;

            var zoom = CameraZoom.Instance;
            if (zoom != null)
            {
                zoom.Reflect().SetValue("zoomPrev", fovValue);
                zoom.Reflect().SetValue("zoomNew", fovValue);
                zoom.Reflect().SetValue("zoomCurrent", fovValue);
                zoom.playerZoomDefault = fovValue;
            }
        }

        public void SetFOV(float value)
        {
            fovValue = value;
        }

        public float GetFOV()
        {
            return fovValue;
        }

        public void EnableFOV(bool state)
        {
            fovEnabled = state;
        }
    }

    public static class Reflector
    {
        public static ReflectionHelper<T> Reflect<T>(this T obj)
        {
            return new ReflectionHelper<T>(obj);
        }
    }

    public class ReflectionHelper<T>
    {
        private readonly T instance;
        private readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly System.Type type;

        public ReflectionHelper(T obj)
        {
            instance = obj;
            type = typeof(T);
        }

        public ReflectionHelper<T> SetValue(string fieldName, object value)
        {
            var field = type.GetField(fieldName, flags);
            if (field != null) field.SetValue(instance, value);
            return this;
        }

        public object GetValue(string fieldName)
        {
            var field = type.GetField(fieldName, flags);
            return field != null ? field.GetValue(instance) : null;
        }
    }
}
