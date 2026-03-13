using System;
using System.Numerics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;



using Terraria.GameContent.Items;
using Terraria.UI;
using System.Collections.Generic;

namespace BO.Content.Items.Grenade
{
    public class Grenade : GlobalItem
    {
        int CD = 300;
        LocalizedText GrenadeToolTip;
        public override bool InstancePerEntity => true;
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.Grenade) return; 
            item.accessory = true;
            item.consumable = false;
            item.maxStack = 1;
            GrenadeToolTip = Mod.GetLocalization(nameof(GrenadeToolTip));
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.Grenade) return false;
            return base.CanUseItem(item, player);
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (item.type != ItemID.Grenade || Main.netMode == NetmodeID.Server || player.whoAmI != Main.LocalPlayer.whoAmI) return;
            if (CD != 0) CD--;
            if (player.itemAnimation != 0 && CD == 0 && player.HeldItem.DamageType == DamageClass.Ranged)
            {
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Center, Microsoft.Xna.Framework.Vector2.Normalize(Main.MouseWorld - Main.LocalPlayer.Center) * 6f, ProjectileID.Grenade, 60, 1f);
                CD += 300;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type != ItemID.Grenade) return;
            foreach (TooltipLine line in tooltips) 
                if (line.Mod == "Terraria" && line.Name == "Tooltip0")
                    line.Text = (string)GrenadeToolTip;
        }
    }
    /*public class GrenadePlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer) return;
            if
        }   
    }*/
}