using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using System.IO;
using Terraria.Localization;

namespace BO.Content.Tiles.sun_maker_flower
{
    public class sun_maker_flower : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type]= true;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = 112;
            TileObjectData.newTile.CoordinateHeights = [192];
            TileObjectData.newTile.DrawYOffset = -174;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
            DustType = 56;
            HitSound = SoundID.Dig;
            AddMapEntry(Color.Cyan);
            MinPick = 40;
            MineResist = 6f;
            AnimationFrameHeight = 192;
            TileObjectData.addTile(Type);
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            ModContent.GetInstance<sun_maker_flower_entity>().Place(i, j);
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, entity.ID);
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            if (entity != null && entity.hasSmile == true)
            {
                entity.hasSmile = false;
                if (i % 2 != 0)
                {
                    for (int n = 0; n < Main.rand.Next(3, 9); n++)
                        Item.NewItem(new EntitySource_TileBreak(i, j), (i + 3) * 16, (j - 1) * 16, 1, 1, 23);
                    for (int n = 0; n < Main.rand.Next(3, 9); n++)
                        Item.NewItem(new EntitySource_TileBreak(i, j), (i - 1) * 16, (j - 6) * 16, 1, 1, 23);
                }
                else
                {
                    for (int n = 0; n < Main.rand.Next(3, 9); n++)
                        Item.NewItem(new EntitySource_TileBreak(i, j), (i - 2) * 16, (j - 1) * 16, 1, 1, 23);
                    for (int n = 0; n < Main.rand.Next(3, 9); n++)
                        Item.NewItem(new EntitySource_TileBreak(i, j), (i + 2) * 16, (j - 6) * 16, 1, 1, 23);
                }
            }
            if (fail == false) ModContent.GetInstance<sun_maker_flower_entity>().Kill(i, j);
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, entity.ID);
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            if (Main.netMode == NetmodeID.Server) return;
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            if (i % 2 == 0) frameXOffset = 112;
            if (entity != null && entity.hasSmile == true) 
            {
                frameYOffset = 192;
            }
            else
            {
                frameYOffset = 0;
            }
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            if (entity != null && entity.hasSmile == true) 
            {
                r = 0;
                g = 0.74902f;
                b = 1;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }

        }
        public override void RandomUpdate(int i, int j)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            if (entity != null)
                entity.tryGrow(i, j);
        }
        
        public override bool Slope(int i, int j)
        {
            return false;
        }
        public override bool CanPlace(int i, int j)
        {
            if (Main.tile[i, j + 1].TileType == TileID.Grass) return true;
            else return false;
        }
    }

    public class sun_maker_flower_entity : ModTileEntity
    {
        public bool hasSmile = true;
        public override bool IsTileValidForEntity(int x, int y)
        {
            return Main.tile[x, y].TileType == ModContent.TileType<sun_maker_flower>();
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(hasSmile);
        }
        public override void Update()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID);
        }
        public override void NetReceive(BinaryReader reader)
        {
            hasSmile = reader.ReadBoolean();
            //Main.NewText("shoudaole" + Position.X + "," + Position.Y);
        }
        public void tryGrow(int i,int j)
        {
            TileEntity.TryGet(i, j, out sun_maker_flower_entity entity);
            if (hasSmile == true||Main.netMode==NetmodeID.MultiplayerClient) return;
            Random rand = new();
            if (rand.Next(0, 3) == 0) hasSmile=true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, entity.ID);
        }
        public override void SaveData(TagCompound tag)
        {
                tag["hasSmile"] = hasSmile;
        }
        public override void LoadData(TagCompound tag)
        {
                hasSmile = tag.GetBool("hasSmile");
        }
    }
    public class sun_maker_flower_rule1 : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (Main.tile[i, j].TileType !=TileID.Grass && Main.tile[i, j - 1].TileType == ModContent.TileType<sun_maker_flower>()) WorldGen.KillTile(i, j - 1,false,false,true);
            //Main.NewText(i + "," + j);
        }
        public override bool CanPlace(int i, int j, int type)
        {
            if (Main.tile[i, j + 1].TileType == ModContent.TileType<sun_maker_flower>()|| Main.tile[i, j + 2].TileType == ModContent.TileType<sun_maker_flower>()|| Main.tile[i, j + 3].TileType == ModContent.TileType<sun_maker_flower>()|| Main.tile[i, j + 4].TileType == ModContent.TileType<sun_maker_flower>()) return false;
            else return true;
        }
    }
}