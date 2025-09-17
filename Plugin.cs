using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Pigeon.Movement;  // Namespace from the code
using Pigeon.Math;  // For EaseFunctions

[BepInPlugin("com.yourname.mycopunk.disableaimfov", "DisableAimFov", "1.0.0")]
[MycoMod(null, ModFlags.IsClientSide)]
public class DisableAimFOVPlugin : BaseUnityPlugin
{
    internal static ConfigEntry<bool> disableFOVChange;

    private void Awake()
    {
        disableFOVChange = Config.Bind("General", "DisableFOVChange", true, "If true, disables FOV zoom changes when aiming.");

        AimPatches.defaultFOVGetter = AccessTools.PropertyGetter(typeof(PlayerLook), "DefaultFOV");
        if (AimPatches.defaultFOVGetter == null)
        {
            //Logger.LogError("DefaultFOV getter not found in PlayerLook class. Mod may not work.");
        }

        AimPatches.isAimingPLField = AccessTools.Field(typeof(PlayerLook), "isAiming");
        if (AimPatches.isAimingPLField == null)
        {
            //Logger.LogError("isAiming field not found in PlayerLook class. Mod may not work.");
        }

        AimPatches.fovField = AccessTools.Field(typeof(PlayerLook), "_fov");
        if (AimPatches.fovField == null)
        {
            //Logger.LogError("_fov field not found in PlayerLook class. Mod may not work.");
        }

        AimPatches.aimFOVPLField = AccessTools.Field(typeof(PlayerLook), "aimFOV");
        if (AimPatches.aimFOVPLField == null)
        {
            //Logger.LogError("aimFOV field not found in PlayerLook class. Mod may not work.");
        }

        AimPatches.aimDurationPLField = AccessTools.Field(typeof(PlayerLook), "aimDuration");
        if (AimPatches.aimDurationPLField == null)
        {
            //Logger.LogError("aimDuration field not found in PlayerLook class. Mod may not work.");
        }

        AimPatches.aimStateChangeTimeField = AccessTools.Field(typeof(PlayerLook), "aimStateChangeTime");
        if (AimPatches.aimStateChangeTimeField == null)
        {
            //Logger.LogError("aimStateChangeTime field not found in PlayerLook class. Mod may not work.");
        }

        var harmony = new Harmony("com.yourname.mycopunk.disableaimfov");

        Logger.LogInfo($"{harmony.Id} loaded!");

        // Patch PlayerLook.UpdateAiming to force default FOV if disabled
        MethodInfo updateAimingMethod = AccessTools.Method(typeof(PlayerLook), "UpdateAiming");
        if (updateAimingMethod != null)
        {
            harmony.Patch(updateAimingMethod, prefix: new HarmonyMethod(typeof(AimPatches), nameof(AimPatches.UpdateAimingPrefix)));
        }
        else
        {
            //Logger.LogError("Could not find PlayerLook.UpdateAiming to patch. Mod may not work.");
        }

        // Patch PlayerLook.UpdateCameraFOV to force default FOV if disabled and aiming
        MethodInfo updateCameraFOVMethod = AccessTools.Method(typeof(PlayerLook), "UpdateCameraFOV");
        if (updateCameraFOVMethod != null)
        {
            harmony.Patch(updateCameraFOVMethod, postfix: new HarmonyMethod(typeof(AimPatches), nameof(AimPatches.UpdateCameraFOVPostfix)));
        }
        else
        {
            //Logger.LogError("Could not find PlayerLook.UpdateCameraFOV to patch. Mod may not work.");
        }
    }
}

internal class AimPatches
{
    internal static MethodInfo defaultFOVGetter;
    internal static FieldInfo isAimingPLField;
    internal static FieldInfo fovField;
    internal static FieldInfo aimFOVPLField;
    internal static FieldInfo aimDurationPLField;
    internal static FieldInfo aimStateChangeTimeField;

    public static bool UpdateAimingPrefix(PlayerLook __instance)
    {
        BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("DisableAimFOV");
        //log.LogInfo("UpdateAimingPrefix called");
        if (DisableAimFOVPlugin.disableFOVChange.Value && isAimingPLField != null && aimStateChangeTimeField != null && aimDurationPLField != null && aimFOVPLField != null && defaultFOVGetter != null && fovField != null)
        {
            bool isAiming = (bool)isAimingPLField.GetValue(__instance);
            if (isAiming)
            {
                float aimStateChangeTime = (float)aimStateChangeTimeField.GetValue(__instance);
                aimStateChangeTime = Mathf.Min(aimStateChangeTime + Time.deltaTime / (float)aimDurationPLField.GetValue(__instance), 1f);
                aimStateChangeTimeField.SetValue(__instance, aimStateChangeTime);
                float defaultFOV = (float)defaultFOVGetter.Invoke(__instance, null);
                aimFOVPLField.SetValue(__instance, defaultFOV);
                fovField.SetValue(__instance, Mathf.LerpUnclamped(defaultFOV, defaultFOV, EaseFunctions.EaseInOutCubic(aimStateChangeTime)));
            }
            else if ((float)aimStateChangeTimeField.GetValue(__instance) > 0f)
            {
                float aimStateChangeTime = (float)aimStateChangeTimeField.GetValue(__instance);
                aimStateChangeTime = Mathf.Max(aimStateChangeTime - Time.deltaTime / (float)aimDurationPLField.GetValue(__instance), 0f);
                aimStateChangeTimeField.SetValue(__instance, aimStateChangeTime);
                float defaultFOV = (float)defaultFOVGetter.Invoke(__instance, null);
                aimFOVPLField.SetValue(__instance, defaultFOV);
                fovField.SetValue(__instance, Mathf.LerpUnclamped(defaultFOV, defaultFOV, EaseFunctions.EaseInOutCubic(aimStateChangeTime)));
            }
            //log.LogInfo("UpdateAimingPrefix: skipped FOV lerp, forced default");
            return false;  // Skip original UpdateAiming
        }
        return true;
    }

    public static void UpdateCameraFOVPostfix(PlayerLook __instance)
    {
        BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("DisableAimFOV");
        //log.LogInfo("UpdateCameraFOVPostfix called");
        if (DisableAimFOVPlugin.disableFOVChange.Value && isAimingPLField != null && fovField != null && defaultFOVGetter != null)
        {
            object isAimingObj = isAimingPLField.GetValue(__instance);
            if (isAimingObj is bool isAiming && isAiming)
            {
                fovField.SetValue(__instance, defaultFOVGetter.Invoke(__instance, null));
                //log.LogInfo("UpdateCameraFOVPostfix: forced _fov to default during aiming");
            }
        }
    }
}