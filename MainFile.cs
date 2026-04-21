using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;

namespace RienSang;

[ModInitializer(nameof(Initialize))]
public class MainFile 
{
    public const string ModId = "RienSang"; 

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ProjectSettings.SetSetting("rendering/viewport/hdr_2d", true);
        
        var root = (Window)Engine.GetMainLoop().Get("root");
        if (root != null)
        {
            root.UseHdr2D = true;
            RenderingServer.ViewportSetUseHdr2D(root.GetViewportRid(), true);
        }
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}