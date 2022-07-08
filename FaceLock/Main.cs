using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FaceLock
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Harmony harmony;
        public static bool IsEnabled = false;
        public static Settings Settings;
        public static readonly Dictionary<string, Texture2D> face = new Dictionary<string, Texture2D>();

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            Logger.Log("Loading Settings...");
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Logger.Log("Load Completed!");

            for (int i = 0; i < RDConstants.data.faceBaseSprites.Length; i++)
            {
                Texture2D sprite = RDConstants.data.faceBaseSprites[i].texture.Copy();
                Texture2D texture = new Texture2D(sprite.width, sprite.height);
                Texture2D detail = RDConstants.data.faceBaseDetailSprites[i]?.texture.Copy();
                for (int w = 0; w < texture.width; w++)
                    for (int h = 0; h < texture.height; h++)
                    {
                        if (sprite.GetPixel(w, h).a != 0)
                            texture.SetPixel(w, h, Color.Lerp(sprite.GetPixel(w, h), sprite.GetPixel(w, h), sprite.GetPixel(w, h).a / 1.0f));
                        if (detail == null)
                            continue;
                        if (detail.GetPixel(w, h).a != 0)
                            texture.SetPixel(w, h, Color.Lerp(texture.GetPixel(w, h), detail.GetPixel(w, h), detail.GetPixel(w, h).a / 1.0f));
                    }
                texture.Apply();
                string name = RDConstants.data.faceBaseSprites[i].texture.name.Split('_')[1];
                face.Add(name, texture);
            }
        }
        public static Texture2D DrawCircle(Color color, int radius)
        {
            Texture2D texture = new Texture2D(radius * 2, radius * 2);
            float rSquared = radius * radius;
            for (int u = -radius; u < radius + 1; u++)
                for (int v = -radius; v < radius + 1; v++)
                    if (u * u + v * v < rSquared)
                        texture.SetPixel(u, v, color);
            return texture;
        }

        public static Texture2D Copy(this Texture2D texture)
        {
            if (texture == null)
                return null;
            RenderTexture renderTex = RenderTexture.GetTemporary(
               texture.width,
               texture.height,
               0,
               RenderTextureFormat.Default,
               RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(texture.width, texture.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
                harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        public static GUIStyle imageStyle;
        public static GUIStyle toggleStyle;

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (imageStyle == null)
            {
                imageStyle = new GUIStyle(GUI.skin.label);
                imageStyle.margin.left += 10;
                toggleStyle = new GUIStyle(GUI.skin.toggle);
                toggleStyle.alignment = TextAnchor.UpperLeft;
                toggleStyle.wordWrap = true;
                toggleStyle.overflow.top = 0;
                toggleStyle.padding.top = 0;
                toggleStyle.margin.bottom += 10;
            }
            for (int i = 0; i < face.Count; i++) {
                var pair = face.ElementAt(i);
                GUILayout.Label(pair.Value, imageStyle);
                bool toggle = GUILayout.Toggle(i == Settings.faceIndex, pair.Key, toggleStyle);
                if (toggle == (i == Settings.faceIndex))
                    continue;
                if (toggle)
                    Settings.faceIndex = i;
                else if (!toggle)
                    Settings.faceIndex = -1;
                scrController.instance.redPlanet.ChangeFace(false);
                scrController.instance.bluePlanet.ChangeFace(false);
                Logger.Log($"face index changed, now {Settings.faceIndex}");
                break;
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Logger.Log("Saving Settings...");
            Settings.Save(modEntry);
            Logger.Log("Save Completed!");
        }
    }
}