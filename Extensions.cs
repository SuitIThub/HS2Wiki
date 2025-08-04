using System.IO;
using BepInEx;
using UnityEngine;

namespace HS2Wiki
{
    public static class Extensions
    {
        public static Texture2D LoadTexture(this string path)
        {
            string imagePath = Path.Combine(Paths.PluginPath, "HS2Wiki", path);
            Texture2D image = null;
            if (File.Exists(imagePath))
            {
                byte[] data = File.ReadAllBytes(imagePath);
                image = new Texture2D(2, 2);
                image.LoadImage(data);
            }
            return image;
        }
    }
}