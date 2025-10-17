using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Pigeon.Movement;
using Pigeon.Math;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class DisableAimFOVPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.disableaimfov";
    public const string PluginName = "DisableAimFOV";
    public const string PluginVersion = "1.0.1";

    internal static ConfigEntry<bool> disableFOVChange;

    private void Awake()
    {
        disableFOVChange = Config.Bind("General", "DisableFOVChange", true, "If true, disables FOV zoom changes when aiming.");

        AimPatches.defaultFOVGetter = AccessTools.PropertyGetter(typeof(PlayerLook), "DefaultFOV");
        AimPatches.isAimingPLField = AccessTools.Field(typeof(PlayerLook), "isAiming");
        AimPatches.fovField = AccessTools.Field(typeof(PlayerLook), "_fov");
        AimPatches.aimFOVPLField = AccessTools.Field(typeof(PlayerLook), "aimFOV");
        AimPatches.aimDurationPLField = AccessTools.Field(typeof(PlayerLook), "aimDuration");
        AimPatches.aimStateChangeTimeField = AccessTools.Field(typeof(PlayerLook), "aimStateChangeTime");

        var harmony = new Harmony(PluginGUID);
        Logger.LogInfo($"{PluginName} loaded successfully.");

        MethodInfo updateAimingMethod = AccessTools.Method(typeof(PlayerLook), "UpdateAiming");
        if (updateAimingMethod != null)
        {
            harmony.Patch(updateAimingMethod, prefix: new HarmonyMethod(typeof(AimPatches), nameof(AimPatches.UpdateAimingPrefix)));
        }

        MethodInfo updateCameraFOVMethod = AccessTools.Method(typeof(PlayerLook), "UpdateCameraFOV");
        if (updateCameraFOVMethod != null)
        {
            harmony.Patch(updateCameraFOVMethod, postfix: new HarmonyMethod(typeof(AimPatches), nameof(AimPatches.UpdateCameraFOVPostfix)));
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
            return false;
        }
        return true;
    }

    public static void UpdateCameraFOVPostfix(PlayerLook __instance)
    {
        if (DisableAimFOVPlugin.disableFOVChange.Value && isAimingPLField != null && fovField != null && defaultFOVGetter != null)
        {
            object isAimingObj = isAimingPLField.GetValue(__instance);
            if (isAimingObj is bool isAiming && isAiming)
            {
                fovField.SetValue(__instance, defaultFOVGetter.Invoke(__instance, null));
            }
        }
    }
}
