using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace dark_cheat
{
    public static class TextureLoader
    {
        public static Texture2D LoadEmbeddedTexture(string resourceName)
        {
            try
            {
                Assembly assembly = typeof(TextureLoader).Assembly;
                string assemblyName = assembly.GetName().Name;
                string fullResourceName = resourceName.StartsWith(assemblyName)
                    ? resourceName
                    : assemblyName + "." + resourceName;

                using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream == null)
                    {
                        Debug.LogError("Embedded resource not found: " + fullResourceName);
                        return Texture2D.whiteTexture;
                    }

                    byte[] fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, fileData.Length);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    if (!tex.LoadImage(fileData))
                    {
                        Debug.LogError("Failed to load image data for: " + fullResourceName);
                    }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    return tex;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception while loading embedded texture: " + e);
                return Texture2D.whiteTexture;
            }
        }
    }
}
