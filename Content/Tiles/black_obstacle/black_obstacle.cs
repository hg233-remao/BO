using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BO.Content.Tiles.black_obstacle
{
    public class black_obstacle : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            DustType = -1;
            AddMapEntry(Color.Black);
            MinPick = 9999;
            Main.tileBlockLight[Type] = true;
        }
        override public bool CanExplode(int i, int j)
        {
            return false;
        }
        public override bool Slope(int i, int j)
        {
            return false;
        }
    }

    public class kill : ModPlayer
    {
        public static LocalizedText killedByBOMessage
        {
            get; private set;
        }
        public override void SetStaticDefaults()
        {
            killedByBOMessage = Language.GetText(Mod.GetLocalizationKey("killedByBOMessage"));
        }
        public override void PreUpdate()
        {
            //密码比怎么这么长，找括号好悬没给我干晕过去
            if (Main.tile[(int)Player.Center.X / 16, (int)Player.Center.Y / 16].TileType == ModContent.TileType<black_obstacle>())
                Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromKey(Player.name+killedByBOMessage)),60000,0,false);
        }
    }
}