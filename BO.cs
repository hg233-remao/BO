using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Content.another.Magic.Magic_System;
using BO.Content.another.spearglobalsets;
using BO.Content.Items.Swords.Living_Wooden_sword;
using BO.Content.Tiles.black_obstacle;
using BO.Content.Tiles.sun_maker_flower;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BO
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class BO : Mod
	{
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) 
            {
                string a = reader.ReadString();
                if (a == "sun_maker_flower_seedling_grow_msg")
                {
                    int b = reader.ReadInt32(), c = reader.ReadInt32();
                    Main.tile[b, c].TileFrameX = 0;
                    Main.tile[b, c].TileType = (ushort)ModContent.TileType<sun_maker_flower>();
                }
                if (a == "SpearHitSyncToMC")
                {
                    int playerindex = reader.ReadInt32();
                    float x = reader.ReadSingle(), y = reader.ReadSingle();
                    Main.player[playerindex].velocity = new Vector2(x, y);
                }
                if (a == "Living_Wooden_Sword_p1SyncToMC")
                {
                    if (Main.projectile[reader.ReadInt32()].ModProjectile is Living_Wooden_Sword_p1 p1)
                    {
                        p1.targetn = reader.ReadInt32();
                    }
                }
                if (a == "Crystal_Angle_To_Zero")
                {
                    Magic_Slot_Sets.Crystal_Angle_Set(0);
                }
                if (a == "Crystal_State_Sync_SToMC")
                {
                    int Active_Power = reader.ReadInt32();
                    int Order = reader.ReadInt32();
                    int Crystal_Num = reader.ReadInt32();
                    int Index = reader.ReadInt32();
                    Main.player[Index].GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Magic_Slots[Order].Active_Power = Active_Power;
                    Main.player[Index].GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Magic_Slots[Order].Check_state(0, Crystal_Num);
                }
            }
            if (Main.netMode == NetmodeID.Server)
            {
                string a= reader.ReadString();
                if (a == "SpearHitSyncToServer")
                {
                    ModPacket to = GetPacket();
                    int playerindex = reader.ReadInt32();
                    float x = reader.ReadSingle(), y = reader.ReadSingle();
                    Main.player[playerindex].velocity = new Vector2(x, y);
                    to.Write("SpearHitSyncToMC");
                    to.Write(playerindex);
                    to.Write(x);
                    to.Write(y);
                    to.Send();
                }
                if (a == "Living_Wooden_Sword_p1SyncToServer")
                {
                    int b = reader.ReadInt32(), c = reader.ReadInt32();
                    if (Main.projectile[b].ModProjectile is Living_Wooden_Sword_p1 p1)
                    {
                        p1.targetn = c;
                    }
                    ModPacket Living_Wooden_Sword_p1SyncToMC = GetPacket();
                    Living_Wooden_Sword_p1SyncToMC.Write("Living_Wooden_Sword_p1SyncToMC");
                    Living_Wooden_Sword_p1SyncToMC.Write(b);
                    Living_Wooden_Sword_p1SyncToMC.Write(c);
                    Living_Wooden_Sword_p1SyncToMC.Send();
                }
                if (a == "Crystal_State_Sync_MCToS")
                {
                    int Active_Power = reader.ReadInt32();
                    int Order = reader.ReadInt32();
                    int Crystal_Num=reader.ReadInt32();
                    Main.player[whoAmI].GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Magic_Slots[Order].Active_Power = Active_Power;
                    Main.player[whoAmI].GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Magic_Slots[Order].Check_state(0, Crystal_Num);
                    ModPacket Crystal_State_Sync_SToMC_Packet = GetPacket();
                    Crystal_State_Sync_SToMC_Packet.Write("Crystal_State_Sync_SToMC");
                    Crystal_State_Sync_SToMC_Packet.Write(Active_Power);
                    Crystal_State_Sync_SToMC_Packet.Write(Order);
                    Crystal_State_Sync_SToMC_Packet.Write(Crystal_Num);
                    Crystal_State_Sync_SToMC_Packet.Write(whoAmI);
                    Crystal_State_Sync_SToMC_Packet.Send(ignoreClient: whoAmI);
                }
            }
        }
    }
}
/*∞°∞°∞°Œ“–¡–¡ø‡ø‡–¥µƒÕ¨≤Ω∞°
if (a == "SpearSlotSyncTS")
{
    //ModPacket SpearSlotSyncTMC=GetPacket();
    int playerindex = reader.ReadInt32(), b = reader.ReadInt32(), c = reader.ReadInt32(), d = reader.ReadInt32();
    Main.player[playerindex].GetModPlayer<setspearslot>().c.type = b;
    Main.player[playerindex].GetModPlayer<setspearslot>().c.prefix = c;
    Main.player[playerindex].GetModPlayer<setspearslot>().c.stack = d;
    SpearSlotSyncTMC.Write("SpearSlotSyncTMC");
    SpearSlotSyncTMC.Write(playerindex);
    SpearSlotSyncTMC.Write(b);
    SpearSlotSyncTMC.Write(c);
    SpearSlotSyncTMC.Write(d);
    SpearSlotSyncTMC.Send();
}
*/