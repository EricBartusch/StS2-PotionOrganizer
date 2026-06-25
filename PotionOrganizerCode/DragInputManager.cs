using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using PotionOrganizer.PotionOrganizerCode.Patches;

namespace PotionOrganizer.PotionOrganizerCode;

public partial class DragInputManager : Node
{
    private static readonly AccessTools.FieldRef<NClickableControl, bool> _isPressedRef =
        AccessTools.FieldRefAccess<NClickableControl, bool>("_isPressed");

    // kill stuff if the game loses focus during drag
    public override void _Notification(int what)
    {
        if (what != (int)MainLoop.NotificationApplicationFocusOut) return;
        if (!DragManager.IsDragging) return;

        var potion = DragManager.DraggedPotion;
        if (potion == null || !IsInstanceValid(potion)) return;

        NPotionHolderPatches.isDragging.Set(potion, false);
        NPotionHolderPatches.hasMoved.Set(potion, false);
        
        _isPressedRef(potion) = false;
        
        var potions = DragManager.Potions;
        var xs = DragManager.PotionsXCoords;
        if (potions != null && xs.Length > 0)
        {
            float y = DragManager.PotionYCoord;
            for (int i = 0; i < potions.Count; i++)
            {
                NPotionHolderPatches.activeTween.Get(potions[i])?.Kill();
                potions[i].Position = new Vector2(xs[i], y);
            }
        }
        DragManager.EndDrag();
    }

    public override void _Input(InputEvent @event)
    {
        if (!DragManager.IsDragging) return;

        var potion = DragManager.DraggedPotion;
        if (potion == null || !IsInstanceValid(potion)) return;

        if (@event is InputEventMouseMotion motion)
        {
            var offset = NPotionHolderPatches.dragOffset.Get(potion);
            
            // this part keeps getting hit until the potion has gone past the threshold
            if (!NPotionHolderPatches.hasMoved.Get(potion))
            {
                var originalMousePos = potion.GlobalPosition + offset;
                if (motion.GlobalPosition.DistanceTo(originalMousePos) > Config.DragThreshold)
                    NPotionHolderPatches.hasMoved.Set(potion, true);
                else
                    return;
            }

            var desired = motion.GlobalPosition - offset;

            // prevent potions from leaving the potion area
            if (DragManager.Container != null)
            {
                var cPos = DragManager.Container.GlobalPosition;
                var cSize = DragManager.Container.Size;
                var hSize = potion.Size;
                desired.X = Mathf.Clamp(desired.X, cPos.X, cPos.X + cSize.X - hSize.X);
                desired.Y = Mathf.Clamp(desired.Y, cPos.Y, cPos.Y + cSize.Y - hSize.Y);
            }

            potion.GlobalPosition = desired;
            PotionReorderer.ReorderPotions(potion);
        }

        else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
        {
            if (NPotionHolderPatches.hasMoved.Get(potion))
            {
                _isPressedRef(potion) = false;
                PotionReorderer.FinalizeReorder();
            }

            NPotionHolderPatches.isDragging.Set(potion, false);
            NPotionHolderPatches.hasMoved.Set(potion, false);
            DragManager.EndDrag();
        }
    }
}
