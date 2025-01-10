using System.Runtime.Serialization;
using System.Text;
using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;
using Serilog;

namespace ChaoticAdditions;

public class ExampleScriptMod(Config config, ILogger logger) : NiceScriptMod(new() {
    {
        "res://Scenes/Map/Props/water_main.gdc",
        (ref TokenCursor cur) => {
            if (!cur.TryMatchAhead(new FunctionWaiter("_ready"))) {
                logger.Warning("Failed to match _ready");
                return;
            }
                            
            cur.Patch(ScriptTokenizer.Tokenize("""
                $StaticBody/CollisionShape.pause_mode = PAUSE_MODE_PROCESS
                $StaticBody/CollisionShape.translation.y -= 0.5
                $StaticBody.set_collision_layer_bit(0, true)
                """, 1
            ));
        }
    },
    {
        "res://Scenes/HUD/Shop/ShopButtons/button_item.gdc",
        (ref TokenCursor cur) => {
            if (!config.WaterToWine) return;
            if (!cur.TryMatchAhead(new FunctionWaiter("_custom_purchase"))) {
                logger.Warning("Failed to match _custom_purchase");
                return;
            }
            cur.Patch(ScriptTokenizer.Tokenize("""
                if item_id == "potion_revert":
                    item_id = "potion_wine"
                    slot_name = Globals.item_data["potion_wine"]["file"].item_name
                """, 1
            ));
        }
    },
    {
        "res://Scenes/Map/Zones/death_transport_zone.gdc",
        (ref TokenCursor cur) => {
            if (!cur.TryMatchAhead(new FunctionWaiter("_ready"))) {
                logger.Warning("Failed to match _ready");
                return;
            }

            cur.Patch(ScriptTokenizer.Tokenize("""
                set_collision_layer_bit(0, true)
                set_collision_layer_mask(0, true)
                add_to_group("water")
                """, 1
            ));

            logger.Information(cur.Display());
        }
    }
}) {}