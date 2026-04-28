using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;
using MegaCrit.Sts2.Core.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace RienSang;

[ModInitializer(nameof(Initialize))]
public class MainFile 
{
    public const string ModId = "RienSang"; 

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    private static PrescriptTransition? _transitionNode;

    public static void Initialize()
    {
        ProjectSettings.SetSetting("rendering/viewport/hdr_2d", true);
    
        var root = (Window)Engine.GetMainLoop().Get("root");
        if (root != null)
        {
            root.UseHdr2D = true;
            RenderingServer.ViewportSetUseHdr2D(root.GetViewportRid(), true);
        
            _transitionNode = new PrescriptTransition();
            root.CallDeferred(Node.MethodName.AddChild, _transitionNode);
        }
    
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(NTransition))]
    public static class TransitionInterceptorPatch
    {
        [HarmonyPatch(nameof(NTransition.FadeOut))]
        [HarmonyPrefix]
        public static bool PrefixFadeOut(ref Task __result, string transitionPath)
        {
            if (transitionPath == "res://RienSang/images/riensang/transitions/riensang_transition_mat.tres")
            {
                _transitionNode?.StartTransition();
                __result = Task.CompletedTask;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(NTransition.FadeIn))]
        [HarmonyPrefix]
        public static bool PrefixFadeIn(ref Task __result)
        {

            if (_transitionNode != null && _transitionNode.Visible)
            {
                __result = Task.CompletedTask;
                return false;
            }
            return true;
        }
    }
}