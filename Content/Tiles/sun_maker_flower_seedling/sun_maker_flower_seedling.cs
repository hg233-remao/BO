using System;
using System.IO;
using BO.Content.Tiles.sun_maker_flower;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace BO.Content.Tiles.sun_maker_flower_seedling
{
    public class sun_maker_flower_seedling : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [18];
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.WaterDeath = true;
            Main.tileBlockLight[Type] = false;
            DustType = 56;
            HitSound = SoundID.Grass;
            MinPick = 1;
            MineResist = 0.1f;
            TileObjectData.addTile(Type);
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (new Random().Next(0,2)==0) Main.tile[i, j].TileFrameX =18;
            else Main.tile[i, j].TileFrameX = 0;
            return false;
        }
        public override bool CanPlace(int i, int j)
        {
            if (Main.tile[i, j + 1].TileType == TileID.Grass && Main.tile[i, j + 1].Slope == SlopeType.Solid) return true;
            else return false;
        }
        public override bool CanDrop(int i, int j)
        {
            return false;
        }
        public override void RandomUpdate(int i, int j)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (new Random().Next(0, 3) == 0 && Main.tile[i, j - 1].HasTile == false && Main.tile[i, j - 2].HasTile == false && Main.tile[i, j - 3].HasTile == false && Main.tile[i, j - 4].HasTile == false)
            {
                Main.tile[i, j].TileFrameX = 0;
                ModContent.GetInstance<sun_maker_flower_entity>().Place(i, j);
                Main.tile[i, j].TileType = (ushort)ModContent.TileType<sun_maker_flower.sun_maker_flower>();
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket sun_maker_flower_seedling_grow = Mod.GetPacket() ;
                    sun_maker_flower_seedling_grow.Write("sun_maker_flower_seedling_grow_msg");
                    sun_maker_flower_seedling_grow.Write(i);
                    sun_maker_flower_seedling_grow.Write(j);
                    sun_maker_flower_seedling_grow.Send();
                }
            }
        }
    }
    public class sun_maker_flower_seedling_rule : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (Main.tile[i, j - 1].TileType == ModContent.TileType<sun_maker_flower_seedling>()) WorldGen.KillTile(i, j - 1, false, false, true);
            //Main.NewText(i + "," + j);
        }
    }
}