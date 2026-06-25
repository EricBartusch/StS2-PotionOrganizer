using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using PotionOrganizer.PotionOrganizerCode;

namespace PotionOrganizer;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "PotionOrganizer";
    
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new Config());
        Harmony harmony = new(ModId);
        harmony.PatchAll();
        
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree != null)
            tree.Root.CallDeferred(Node.MethodName.AddChild, new DragInputManager());
    }
}