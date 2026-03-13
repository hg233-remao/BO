using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using BO.Content.Tiles.black_obstacle;
using BO.Content.Tiles.sun_maker_flower;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
namespace BO.Content.World
{
    public class worldCreate : ModSystem
    {
        public static LocalizedText replaceALLLLL
        {
            get; private set;
        }
        public static LocalizedText clearALLLLL
        {
            get;private set;
        }
        public static LocalizedText createBlackObstaclesPassMessage
        {
            get; private set;
        }
        public static LocalizedText createForestPassMessage
        {
            get; private set;
        }
        public override void SetStaticDefaults()
        {
            replaceALLLLL = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(replaceALLLLL)}"));
            clearALLLLL = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(clearALLLLL)}"));
            createBlackObstaclesPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(createBlackObstaclesPassMessage)}"));
            createForestPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(createForestPassMessage)}"));
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            tasks.RemoveAt(tasks.FindIndex(pass => pass.Name == "Guide"));
            //tasks.clear();//调试时用的，生成世界比较快
            tasks.Add(new replaceALLLLL("1", 0));
            tasks.Add(new clearALLLLL("1", 1));
            tasks.Add(new createBlackObstaclesPass("1", 2));
            tasks.Add(new createForestPass("1", 3));
        }
    }
    //直接taskClear会导致多人房主出问题，我也不知道为什么，所以只能在原版世界生成步骤完成后替换物块清空世界了，嗯，这样的话最大的问题就是生成世界的时间可能会长不少
    public class replaceALLLLL : GenPass
    {
        public replaceALLLLL(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = worldCreate.replaceALLLLL.Value;
            Main.maxTilesX = 4200;
            Main.maxTilesY = 1200;
            Main.spawnTileX = 500;
            Main.spawnTileY = 240;
            Main.worldSurface = 400;
            Main.rockLayer = 700;
            for (int x = 0, y = 0; x <= Main.maxTilesX; x++)
                for (y = 0; y <= Main.maxTilesY; y++)
                {
                    Main.tile[x, y].TileType = 0;
                    Main.tile[x, y].WallType = 0;
                }
        }

    }
    public class clearALLLLL : GenPass
    {
        public clearALLLLL(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath61);
            progress.Message = worldCreate.clearALLLLL.Value;
            for (int x = 0, y = 0; x <= Main.maxTilesX; x++)
                for (y = 0; y <= Main.maxTilesY; y++)
                {
                    WorldGen.KillTile(x, y);
                    WorldGen.KillWall(x, y);
                    Main.tile[x, y].LiquidAmount = 0;
                }
        }

    }
    //放黑障过程
    public class createBlackObstaclesPass : GenPass
    {
        public createBlackObstaclesPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = worldCreate.createBlackObstaclesPassMessage.Value;
            //每隔343放竖向黑障
            for (int x = 41; x <= 4158; x += 343)
            {
                for (int y = 156; y <= 998; y++)
                    WorldGen.PlaceTile(x, y, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
            }
            for (int x = 0, dy = WorldGen.genRand.Next(3, 23), ey = WorldGen.genRand.Next(3, 23); x <= Main.maxTilesX; x++) 
            {
                //上层黑障形成刺过程
                WorldGen.PlaceTile(x, 156, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
                if (dy > 3 && dy < 22) dy += WorldGen.genRand.Next(-5, 6);
                if (dy <= 3) dy += WorldGen.genRand.Next(0, 6);
                if (dy>=22) dy += WorldGen.genRand.Next(-5,1);
                    for (int sy = 0; sy < dy; sy++)
                    {
                        WorldGen.PlaceTile(x, 156 + sy, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
                    }
                //下层黑障形成刺过程
                WorldGen.PlaceTile(x, 998, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
                if (ey > 3 && ey < 22) ey += WorldGen.genRand.Next(-5, 6);
                if (ey <= 3) ey += WorldGen.genRand.Next(0, 6);
                if (ey >= 22) ey += WorldGen.genRand.Next(-5, 1);
                for (int sy = 0; sy < ey; sy++)
                {
                    //防止在火山与地狱连接区域形成黑障
                    if (2442 <= x && x <= 2785) continue;
                    WorldGen.PlaceTile(x, 998 + sy, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
                }
            }
            //隔绝森林与洞穴
            for (int x=384;x<=727 ;x++ )
            {
                WorldGen.PlaceTile(x, 400, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
            }
            //移除在火山地狱连接处的黑障，可以优化，但我懒得写了，反正影响不大
            for (int x = 2442; x <= 2785; x++)
            {
                WorldGen.KillTile(x, 998);
            }
        }
    }
    //森林
    //横向385-726
    //纵向157-399
    public class createForestPass : GenPass
    {
        bool cwall = true;
        public createForestPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = worldCreate.createForestPassMessage.Value;
            //放土
            for (int x = 385, y = 248 + WorldGen.genRand.Next(-1, 2); x < 727; x++)
            {
                if (WorldGen.genRand.Next(0, 21) > 18) y = y + WorldGen.genRand.Next(-1, 2);
                for (int y1 = y; y1 < 400; y1++)
                    WorldGen.PlaceTile(x, y1, TileID.Dirt, false, false, -1, 1);
            }
            //放粘土
            for (int x = 400; x < 670; x++)
            {
                if (WorldGen.genRand.Next(0, 20) == 0)
                    WorldGen.TileRunner(x, WorldGen.genRand.Next(300, 370), 10, 20, TileID.ClayBlock);
            }
            //随机扣4个洞
            for (int x = WorldGen.genRand.Next(385, 727), y = WorldGen.genRand.Next(290, 400), c = 0; c < 4; x = WorldGen.genRand.Next(385, 727), y = WorldGen.genRand.Next(290, 400), c++)
                WorldGen.TileRunner(x, y, 10, 16, -1);
            //用于生成大洞
            for (int y = 310, lx = 550 + WorldGen.genRand.Next(-1, 2), rx = 560 + WorldGen.genRand.Next(-1, 2); y <= 399; y++)
            {
                if (WorldGen.genRand.Next(0, 4) == 0) lx += WorldGen.genRand.Next(-1, 2);
                else if (y < 360) lx += WorldGen.genRand.Next(-3, 0);
                else if (y > 385) lx -= WorldGen.genRand.Next(-6, 0);
                if (WorldGen.genRand.Next(0, 4) == 0) rx += WorldGen.genRand.Next(-1, 2);
                else if (y < 360) rx += WorldGen.genRand.Next(1, 4);
                else if (y > 385) rx -= WorldGen.genRand.Next(1, 7);
                for (int x = lx; x <= rx; x++)
                    WorldGen.KillTile(x, y);
            }
            //扣隧道
            for (int x = 555, y = 390, a = 0; y > 260;)
            {
                if (WorldGen.genRand.Next(0, 3) == 0)
                    y += WorldGen.genRand.Next(-1, 1);
                if (WorldGen.genRand.Next(0, 150) == 0)
                {
                    for (; a == 0; a = WorldGen.genRand.Next(-1, 2)) ;
                    a *= -1;
                }
                x += a;
                if (!(x > 390 && x < 715)) a *= -1;
                WorldGen.KillTile(x - 1, y - 2);
                WorldGen.KillTile(x - 0, y - 2);
                WorldGen.KillTile(x + 1, y - 2);
                WorldGen.KillTile(x - 2, y - 1);
                WorldGen.KillTile(x - 1, y - 1);
                WorldGen.KillTile(x - 0, y - 1);
                WorldGen.KillTile(x + 1, y - 1);
                WorldGen.KillTile(x + 2, y - 1);
                WorldGen.KillTile(x - 2, y);
                WorldGen.KillTile(x - 1, y);
                WorldGen.KillTile(x - 0, y);
                WorldGen.KillTile(x + 1, y);
                WorldGen.KillTile(x + 2, y);
                WorldGen.KillTile(x - 2, y + 1);
                WorldGen.KillTile(x - 1, y + 1);
                WorldGen.KillTile(x - 0, y + 1);
                WorldGen.KillTile(x + 1, y + 1);
                WorldGen.KillTile(x + 2, y + 1);
                WorldGen.KillTile(x - 1, y + 1);
                WorldGen.KillTile(x - 0, y + 1);
                WorldGen.KillTile(x + 1, y + 1);
                WorldGen.KillTile(x - 1, y + 2);
                WorldGen.KillTile(x - 0, y + 2);
                WorldGen.KillTile(x + 1, y + 2);
            }
            //再放一次黑障防止扣墙步骤有黑障被扣掉
            for (int y = 157; y <= 399; y++)
            {
                WorldGen.PlaceTile(384, y, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
                WorldGen.PlaceTile(727, y, ModContent.TileType<black_obstacle>(), false, false, -1, 1);
            }
            for(int x=384;x<=727 ;x++)
            {
                WorldGen.PlaceTile(x, 400, ModContent.TileType<black_obstacle>(), false, false, -1, 1); 
            }
            //悬空方块,草方块处理,放墙
            for (int y = 158, x = 385; y <= 399; y++)
                for (x = 385; x <= 726; x++)
                {
                    if (Main.tile[x, y - 1].HasTile == false && Main.tile[x - 1, y].HasTile == false && Main.tile[x + 1, y].HasTile == false && Main.tile[x, y + 1].HasTile == false)
                        WorldGen.KillTile(x, y);
                    if (Main.tile[x, y].TileType == TileID.Dirt && (Main.tile[x - 1, y - 1].HasTile == false || Main.tile[x, y - 1].HasTile == false || Main.tile[x + 1, y - 1].HasTile == false || Main.tile[x - 1, y].HasTile == false || Main.tile[x + 1, y].HasTile == false || Main.tile[x - 1, y + 1].HasTile == false || Main.tile[x, y + 1].HasTile == false || Main.tile[x + 1, y + 1].HasTile == false))
                        WorldGen.PlaceTile(x, y, TileID.Grass, false, false, -1, 1);
                    if (y >= 255)
                    {
                        if (Main.tile[x, y].HasTile == true)
                            Main.tile[x, y].WallType = WallID.DirtUnsafe;
                        else if (y > 310 || WorldGen.genRand.Next(0, 20) == 0)
                            Main.tile[x, y].WallType = WallID.FlowerUnsafe;
                        else
                            Main.tile[x, y].WallType = WallID.GrassUnsafe;
                    }
                }
            //遍历地表,放小石块,土块，倒木，还有树
            for (int x = 390, y = 240,sc=0; x <= 720; x++)
            {
                if (sc != 0) sc--;
                for (y = 240; y < 300; y++) 
                {
                    if (Main.tile[x, y].TileType == TileID.Grass && Main.tile[x, y - 1].HasTile == false)
                    {
                        if (WorldGen.genRand.Next(0, 100) == 0)
                        {
                            WorldGen.Place3x2(x, y - 1, TileID.LargePiles2, WorldGen.genRand.Next(14, 16));
                            break;
                        }
                        if (WorldGen.genRand.Next(0, 100) == 0)
                        {
                            WorldGen.Place3x2(x, y - 1, TileID.FallenLog);
                            break;
                        }
                        if (WorldGen.genRand.Next(0, 30) == 0)
                        {
                            WorldGen.Place1x1(x, y - 1, TileID.SmallPiles, 0);
                            Main.tile[x, y - 1].TileFrameX = (short)(18 * WorldGen.genRand.Next(0, 12));
                            break;
                        }
                        if (WorldGen.genRand.Next(0, 100) == 0)
                        {
                            WorldGen.Place2x1(x, y - 1, TileID.SmallPiles, WorldGen.genRand.Next(38, 41));
                            Main.tile[x, y - 1].TileFrameY = (short)18;
                            Main.tile[x + 1, y - 1].TileFrameY = (short)18;
                            break;
                        }
                        if (WorldGen.genRand.Next(0, 10) == 0)
                        {
                            WorldGen.GrowTree(x, y);
                            break;
                        }
                        if (WorldGen.genRand.Next(0, 50) == 0 && sc == 0) 
                        {
                            if (WorldGen.PlaceTile(x, y - 1, ModContent.TileType<sun_maker_flower>())) TileEntity.PlaceEntityNet(x, y-1, ModContent.TileEntityType<sun_maker_flower_entity>());
                            sc = 4;
                            break;
                        }
                    }
                }
            }
            //放水
            for (int x = 545, y; x <= 565; x++)
                for (y = 300; y <= 390; y++) 
                    Main.tile[x, y].LiquidAmount = 255;
        }
    }
}