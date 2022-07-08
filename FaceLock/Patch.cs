using HarmonyLib;

namespace FaceLock
{
    public static class Patch
    {
        [HarmonyPatch(typeof(scrPlanet), "ChangeFace")]
        public static class ChangeFacePatch
        {
            public static void Postfix(scrPlanet __instance)
            {
                if (!__instance.Get<bool>("faceMode"))
                    return;
                if (Main.Settings.faceIndex == -1)
                    return;
                __instance.Set("faceIndex", Main.Settings.faceIndex);
                __instance.faceSprite.sprite = RDConstants.data.faceBaseSprites[Main.Settings.faceIndex];
                __instance.faceDetails.sprite = RDConstants.data.faceBaseDetailSprites[Main.Settings.faceIndex];
            }
        }
    }
}