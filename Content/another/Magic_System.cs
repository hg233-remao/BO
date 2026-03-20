using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BO.Content.another.spearglobalsets;
using BO.Content.Items.Magic.Spell_Books.Book_Of_Leaves;
using BO.Content.Items.Magic.Wands.Wand_Of_Sparking;
using Humanizer;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
namespace BO.Content.another.Magic.Magic_System
{
    public class Magic_Slot_Template
    {
        //一个槽的结构体
        public class Magic_Barrier_Crystal_In_Slots
        {
            //该槽的水晶type
            public int Magic_Barrier_Crystal_Type = 0;
            //当前活力值
            public int Active_Power = 0;
            //这个水晶的活力值上限
            public int Internal_Max_Active = 0;
            //这个水晶在所有水晶中的排序，按照先后顺序来，主要用来绘制水晶旋转时的角度不乱转，和槽的序列一致
            public int Order = 0;
            //这个槽绑定的水晶的索引
            public int? Projectile_Index = null;
            //初始化时告诉这个槽它的顺序
            public Magic_Barrier_Crystal_In_Slots(int order)
            { 
                Order = order;
            }
            //检测一次自身状态，一般在自己的活力值改变时使用，因此我将改变水晶活力值的地方集成在这个方法上了
            public void Check_state(int active,int Crystal_Num)
            { 
                Active_Power += active;
                if (Magic_Barrier_Crystal_Type != 0)
                {
                    //气死我了，写到这里发现要多人同步的话还得写个水晶弹幕的基类
                    Crystal_Projectile Crystal_Projectile_C = Main.projectile[(int)Projectile_Index].ModProjectile as Crystal_Projectile;
                    Crystal_Projectile_C.Sync(Crystal_Num, Active_Power);
                }
                else if (Projectile_Index != null) 
                {
                    Main.projectile[(int)Projectile_Index].Kill();
                    Projectile_Index = null;
                }
            }
            //为这个槽设置一个水晶，并根据给予活力值刷新一次
            //创建水晶的射弹时最后三个参数告诉射弹槽序列目前水晶数量这个槽的活力值
            public void Set_Crystal(int type,int active,int Crystal_Num)
            {
                Magic_Barrier_Crystal_Type = type;
                Internal_Max_Active = Crystal_Whose_Space(type);
                if (Magic_Barrier_Crystal_Type != 0)
                {
                    Projectile_Index = Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Center, Vector2.Zero, type, 1, 1, Main.myPlayer, Order, Crystal_Num, Active_Power);
                }
                if (active <= Internal_Max_Active)
                    Check_state(active, Crystal_Num);
                else
                    Check_state(Internal_Max_Active, Crystal_Num);
            }
            public void Clear_Crystal(int Crystal_Num)
            {
                Magic_Barrier_Crystal_Type = 0;
                Internal_Max_Active = Crystal_Whose_Space(0);
                Check_state(-Active_Power, Crystal_Num);
            }
        }
        //允许玩家同时使用最多54个水晶，当然这个上限随便改，取决于mod之后的战力体系
        public Magic_Barrier_Crystal_In_Slots[] Magic_Slots = new Magic_Barrier_Crystal_In_Slots[54];
        //初始化54个水晶槽
        public Magic_Slot_Template()
        {
            for (int i = 0; i < Magic_Slots.Length; i++)
            {
                Magic_Slots[i] = new Magic_Barrier_Crystal_In_Slots(i);
            }
        }
        //物品type转化射弹type
        public static int Crystal_Convert_Projecile(int type)
        {
            if (type == ItemID.None)
                return 0;
            if (type == ModContent.ItemType<Book_Of_Leaves_Crystal_d>())
                return ModContent.ProjectileType<Book_Of_Leaves_Crystal>();
            return 0;
        }
        //射弹type查询占用空间
        public static int Crystal_Whose_Space(int type)
        {
            if (type == ProjectileID.None)
                return 0;
            if (type == ModContent.ProjectileType<Book_Of_Leaves_Crystal>())
                return 2;
            return 0;
        }
        //这里太乱了导致我写代码经常搞糊涂自己，因此，全部重写！
        //首先澄清几个定义吧
        //活力值，每个水晶都有自己的最大活力值，活力值不同，效果也就不同
        //在游戏里面以栏位的形式体现，一个可以使用的活力值等于一个栏位，同时剩余栏位不足以到达一个水晶的最大活力值时这个水晶无法被创建
        //玩家当前的活力值上限，默认为2吧，还是调2更适合调试
        public int Max_Active = 2;
        //玩家当前的活力值
        public int Active = 0;
        //呃然后好像不需要写别的变量了？？？那就写一写那些烦人的方法吧
        //检查目前水晶使用着多少活力值
        public int Using_Active()
        {
            int s = 0;
            for (int i = 0; i < Magic_Slots.Length; i++) 
            {
                s += Magic_Slots[i].Active_Power;
            }
            return s;
        }
        //检查目前占用了多少栏位
        public int Max_Using_Active()
        {
            int s = 0;
            for (int i = 0; i < Magic_Slots.Length; i++)
            {
                s += Magic_Slots[i].Internal_Max_Active;
            }
            return s;
        }
        //检查目前有多少水晶
        public int Crystal_Num()
        {
            int s = 0;
            for (int i = 0; i < Magic_Slots.Length; i++)
            {
                if (Magic_Slots[i].Magic_Barrier_Crystal_Type != 0)
                    s++;
            }
            return s;
        }
        //增加一点活力值
        public void Add_Active()
        {
            if (Active < Max_Active)
            {
                if (Using_Active() < Max_Using_Active())
                {
                    for (int i = 0; i < Magic_Slots.Length; i++)
                    {
                        if (Magic_Slots[i].Active_Power < Magic_Slots[i].Internal_Max_Active)
                        {
                            Magic_Slots[i].Check_state(1, Crystal_Num());
                            break;
                        }
                    }
                }
                Active++;
            }
        }
        //减少一点活力值
        public void Remove_Active()
        {
            if (Active > 0) 
            {
                if (Using_Active() > 0) 
                {
                    for (int i = 0; i < Magic_Slots.Length; i++)
                    {
                        if (Magic_Slots[i].Active_Power > 0)
                        {
                            Magic_Slots[i].Check_state(-1, Crystal_Num());
                            break;
                        }
                        else
                        {
                            Magic_Slots[i - 1].Check_state(-1, Crystal_Num());
                            break;
                        }
                    }
                }
                Active--;
            }
        }
        //减少全部活力值
        public void Remove_All_Active()
        {
            for (int i = Active; i > 0; i--)
                Remove_Active();
        }
        //在最右侧槽设置一个水晶
        public void Set_Crystal(int type)
        {
            //type = Crystal_Convert_Projecile(type);
            if (Max_Using_Active() + Crystal_Whose_Space(type) <= Max_Active)
                for (int i = 0; i < Magic_Slots.Length; i++)
                {
                    if (Magic_Slots[i].Magic_Barrier_Crystal_Type == 0)
                    {
                        Magic_Slots[i].Set_Crystal(type, Active - Using_Active(), Crystal_Num());
                        break;
                    }
                }
        }
        //在最右侧删除一个水晶
        public void Clear_Crystal()
        {
            //Main.NewText("i was cleared");
            if (Magic_Slots[0].Magic_Barrier_Crystal_Type != 0) 
                for (int i = 0; i < Magic_Slots.Length; i++)
                {
                    if (Magic_Slots[i].Magic_Barrier_Crystal_Type == 0)
                    {
                        Magic_Slots[i - 1].Clear_Crystal(Crystal_Num());
                        break;
                    }
                }
        }
    }
    //设置玩家相关的魔力方面的特性
    public class Magic_Slot_Sets : ModPlayer
    {
        //用来调整所有水晶的角度
        public static int Crystal_Angle = 0;
        //一个魔法栏位的实例
        public Magic_Slot_Template Magic_Slot;
        //法杖充能量
        public int Magic_Ammo = 0;
        public bool Full_Entity_Power = false;
        //生成魔力聚合素的冷却
        public int Magic_Power_Cooldown = 0;
        public int Hold_Item_Before;
        //用来存储玩家是否学习了某个水晶
        public bool[] Has_Learned_Magic = new bool[54];
        //默认的活力值提升速度
        public int Active_Add_Per_Frame = 100;
        //当前冷却充能
        public int Current_Cooldown = 0;
        //充能上限
        public const int Max_Cooldown = 60000;
        //保存加载用的中间变量
        int[] ALL = new int[108];
        //检查玩家是否在游戏中，因为总是出现奇怪的数组重置现象，所以只能另寻它径
        bool Is_In_Game = false;
        //将学习水晶的数组序列与水晶介绍物品的type映射起来
        public static int Index_To_Crystal_Item_Type(int Index)
        {
            if (Index == 0)
                return 0;
            if (Index == 1)
                return ModContent.ItemType<Book_Of_Leaves_Crystal_d>();
            return 0;
        }
        //受伤时减少一点活力值
        public override void OnHurt(Player.HurtInfo info)
        {
            if (Main.LocalPlayer.whoAmI != Player.whoAmI || Main.netMode == NetmodeID.Server) return;
            Magic_Slot.Remove_Active();
            Current_Cooldown = 0;
        }
        //死掉时移除所有活力值
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (Main.LocalPlayer.whoAmI != Player.whoAmI || Main.netMode == NetmodeID.Server) return;
            Magic_Slot.Remove_All_Active();
        }
        //检测玩家上一帧所持物品,以及水晶转动角度
        public override void PreUpdate()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Hold_Item_Before = Player.HeldItem.type;
            if (Crystal_Angle < 360)
                Crystal_Angle++;
            else Crystal_Angle = 0;
        }
        //cd恢复
        public override void PostUpdateEquips()
        {
            if (Main.LocalPlayer.whoAmI != Player.whoAmI || Main.netMode == NetmodeID.Server) return;
            Current_Cooldown += Active_Add_Per_Frame;
            if (Current_Cooldown < 0)
                Current_Cooldown = 0;
            if (Current_Cooldown >= Max_Cooldown)
            {
                Current_Cooldown = 0;
                Magic_Slot.Add_Active();
            }
        }
        //装备加成重置
        public override void ResetEffects()
        {
            Magic_Slot.Max_Active = 2;
            Active_Add_Per_Frame = 100;
        }
        //对活力值上限的影响
        public void Max_Slots_Addition(int Addition)
        {
            Magic_Slot.Max_Active = 1 + Addition;
        }
        //对cd恢复速率的影响，单位1%
        public void Cooldown_Addition(int Addition)
        {
            Active_Add_Per_Frame = 100 + 100 * Addition;
        }
        //添加一个水晶，即要使用的水晶
        public void Add_Crystal(int ID)
        {
            Magic_Slot.Set_Crystal(ID);
        }
        //初始化
        public override void Initialize()
        {
            if (Main.LocalPlayer == null)
                return;
            Magic_Slot = new Magic_Slot_Template();
        }
        //保存这个玩家学习的水晶以及设置的水晶
        public override void SaveData(TagCompound tag)
        {
            for (int i = 0; i < Has_Learned_Magic.Length; i++) 
            {
                if (Has_Learned_Magic[i] == true) 
                    ALL[i] = 1;
                else
                    ALL[i] = 0;
            }
            if (Is_In_Game)  
            {
                for (int i = 0; i < Magic_Slot.Magic_Slots.Length; i++)
                {
                    ALL[54 + i] = Crystal_ID_To_Custom_ID(Magic_Slot.Magic_Slots[i].Magic_Barrier_Crystal_Type);
                }
                Is_In_Game = false;
            }
            
            tag["ALL"] = ALL;
        }
        //加载这个玩家学习的水晶以及设置的水晶
        public override void LoadData(TagCompound tag)
        {
            if (tag.Get<int[]>("ALL") == null)
                return;
            for (int i = 0; i < Has_Learned_Magic.Length; i++)
            {
                if (tag.Get<int[]>("ALL")[i] == 1)
                    Has_Learned_Magic[i] = true;
                else
                    Has_Learned_Magic[i] = false;
            }
            for (int i = 0; i < Magic_Slot.Magic_Slots.Length; i++)
            {
                ALL[54 + i] = tag.Get<int[]>("ALL")[54 + i];
            }
        }
        //清空ui状态,以及防止一些ui空引用行为,以及进入游戏后读取水晶设置状态
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Is_In_Game = true;
            //我咋感觉这样不大行。。。试了试还真行
            ModContent.GetInstance<Magic_Slot_UI_System>().a.Clear_Learned_Crystal();
            for (int i = 0; i < Magic_Slot.Magic_Slots.Length; i++)
            {
                Magic_Slot.Set_Crystal(Custom_ID_To_Crystal_ID(ALL[54 + i]));
            }
        }
        //自定义ID转水晶弹幕ID，防止重加载mod崩档
        public int Custom_ID_To_Crystal_ID(int Custom_ID)
        {
            if (Custom_ID == 0)
                return 0;
            if (Custom_ID == 1)
                return ModContent.ProjectileType<Book_Of_Leaves_Crystal>();
            return 0;
        }
        //水晶弹幕ID转自定义ID，防止重加载mod崩档
        public int Crystal_ID_To_Custom_ID(int Crystal_ID)
        {
            if (Crystal_ID == 0)
                return 0;
            if (Crystal_ID == ModContent.ProjectileType<Book_Of_Leaves_Crystal>())
                return 1;
            return 0;
        }
        //设置水晶角度
        public static void Crystal_Angle_Set(int Angle)
        { 
            Crystal_Angle = Angle;
        }
        //多人水晶角度同步
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == NetmodeID.Server && newPlayer) 
            {
                ModPacket Crystal_Angle_To_Zero_Packet = Mod.GetPacket();
                Crystal_Angle_To_Zero_Packet.Write("Crystal_Angle_To_Zero");
                Crystal_Angle_To_Zero_Packet.Send();
            }
        }
    }
    //全局水晶角度，主要用来多人同步水晶角度，我打算给每个玩家的水晶角度都弄一样了，稍微节省点性能，开发起来也容易
    public class Crystal_Angle_set : ModSystem
    {
        public static int Crystal_Angle = 0;
        public override void OnModLoad()
        {
        }
        public override void NetSend(BinaryWriter writer)
        {
            if (Main.netMode != NetmodeID.Server) return;
            writer.Write(Crystal_Angle);
        }
        public override void NetReceive(BinaryReader reader)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            Magic_Slot_Sets.Crystal_Angle_Set(reader.ReadInt32());
        }
    }
    //魔法相关ui的那啥，对
    public class Magic_Slot_UI_System : ModSystem
    { 
        public UserInterface Magic_Slot_UI_UserInterface;
        private GameTime BeforeTime;
        public Magic_Slot_UIState a;
        public LocalizedText Create_Crystal;
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Magic_Slot_UI_UserInterface = new UserInterface();
            a=new Magic_Slot_UIState();
            a.Activate();
            Magic_Slot_UI_UserInterface.SetState(a);
            Create_Crystal = Mod.GetLocalization(nameof(Create_Crystal));
        }
        public override void UpdateUI(GameTime gameTime)
        {
            BeforeTime = gameTime;
            if (Magic_Slot_UI_UserInterface?.CurrentState != null) Magic_Slot_UI_UserInterface.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int Index = layers.FindIndex(layers => layers.Name.Equals("Vanilla: Mouse Text"));
            if (Index != -1)
            {
                layers.Insert(Index, new LegacyGameInterfaceLayer("Magic_Slot_UI", delegate
                {
                    if (BeforeTime != null && Magic_Slot_UI_UserInterface?.CurrentState != null)
                    {
                        Magic_Slot_UI_UserInterface.Draw(Main.spriteBatch, BeforeTime);
                    }
                    return true;
                }, InterfaceScaleType.UI
                ));
            }
        }
    }
    //负责绘制魔力水晶构建界面的地方
    public class Magic_Slot_UIState : UIState
    {
        /*
        0 1 2
        3 4 5
        6 7 8
        3列3行够了吗，应该够吧，来吧，写一下米妮
        */
        bool IsHide = true;
        Single_Slot_UI Single_Slot_UI = new Single_Slot_UI();
        Crystal_Adding_UI Crystal_Adding_UI = new Crystal_Adding_UI();
        UIPanel back = new UIPanel();
        UIPanel backu = new UIPanel();
        UIPanel backd = new UIPanel();
        UIPanel[] Nine_Crystal_Panel = new UIPanel[9];
        UIPanel Crystal_Delete_Back = new UIPanel();
        Crystal_Using_State Crystal_Using_State = new Crystal_Using_State();
        public void Clear_Learned_Crystal()
        {
            Crystal_Adding_UI.Clear();
        }
        public override void OnInitialize()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Crystal_Adding_UI.Top.Set(70, 0f);
            Crystal_Adding_UI.Width.Set(225, 0f);
            Crystal_Adding_UI.Height.Set(180, 0f);
            Crystal_Adding_UI.Activate();
            Width.Set(0, 1f);
            Height.Set(0, 1f);
            Left.Set(0, 0f);
            Top.Set(0, 0f);
            Append(Single_Slot_UI);
            Append(Crystal_Using_State);
            //Single_Slot_UI.OnInitialize();
            back.Top.Set(70, 0f);
            back.Width.Set(225, 0f);
            back.Height.Set(180, 0f);
            //Append(back);
            backu.Top.Set(70, 0f);
            backu.Width.Set(45, 0f);
            backu.Height.Set(45, 0f);
            //Append(backu);
            backd.Top.Set(205, 0f);
            backd.Width.Set(45, 0f);
            backd.Height.Set(45, 0f);
            //Append(backd);
            Crystal_Delete_Back.Top.Set(155, 0f);
            Crystal_Delete_Back.Width.Set(45, 0f);
            Crystal_Delete_Back.Height.Set(45, 0f);
            for (int i = 0; i <= 8; i++)
                Nine_Crystal_Panel[i] = new UIPanel();
            for (int i = 0; i <= 8; i++)
            {
                Nine_Crystal_Panel[i].Height.Set(44, 0f);
                Nine_Crystal_Panel[i].Width.Set(44, 0f);
            }
            for (int i = 0; i <= 2; i++)
                Nine_Crystal_Panel[i].Top.Set(83, 0f);
            for (int i = 3; i <= 5; i++)
                Nine_Crystal_Panel[i].Top.Set(138, 0f);
            for (int i = 6; i <= 8; i++)
                Nine_Crystal_Panel[i].Top.Set(193, 0f);
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.netMode == NetmodeID.Server) return;
            back.Left.Set(Main.screenWidth * 0.45f - 95f, 0f);
            backu.Left.Set(Main.screenWidth * 0.45f + 85f, 0f);
            backd.Left.Set(Main.screenWidth * 0.45f + 85f, 0f);
            Crystal_Delete_Back.Left.Set(Main.screenWidth * 0.45f + 85f, 0f);
            Nine_Crystal_Panel[0].Left.Set(Main.screenWidth * 0.45f - 84, 0f);
            Nine_Crystal_Panel[1].Left.Set(Main.screenWidth * 0.45f - 29, 0f);
            Nine_Crystal_Panel[2].Left.Set(Main.screenWidth * 0.45f + 26, 0f);
            Nine_Crystal_Panel[3].Left.Set(Main.screenWidth * 0.45f - 84, 0f);
            Nine_Crystal_Panel[4].Left.Set(Main.screenWidth * 0.45f - 29, 0f);
            Nine_Crystal_Panel[5].Left.Set(Main.screenWidth * 0.45f + 26, 0f);
            Nine_Crystal_Panel[6].Left.Set(Main.screenWidth * 0.45f - 84, 0f);
            Nine_Crystal_Panel[7].Left.Set(Main.screenWidth * 0.45f - 29, 0f);
            Nine_Crystal_Panel[8].Left.Set(Main.screenWidth * 0.45f + 26, 0f);
            if (Main.mouseX > Main.screenWidth * 0.45f && Main.mouseX < Main.screenWidth * 0.45f + 35 && Main.mouseY > 15 && Main.mouseY < 66 && Main.LocalPlayer.itemAnimation == 0) 
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (Main.mouseX > Main.screenWidth * 0.45f - 95 && Main.mouseX < Main.screenWidth * 0.45f + 130 && Main.mouseY > 70 && Main.mouseY < 250 && Main.LocalPlayer.itemAnimation == 0 && !IsHide)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Main.mouseX > Main.screenWidth * 0.45f && Main.mouseX < Main.screenWidth * 0.45 + 35 && Main.mouseY > 15 && Main.mouseY < 66 && Main.LocalPlayer.itemAnimation == 0)
            {//若存在，则点击一次消失，否则反之，等我干完活回来就写的
                if (IsHide)
                {
                    Append(back);
                    Append(backu);
                    Append(backd);
                    Apall();
                    Append(Crystal_Delete_Back);
                    Append(Crystal_Adding_UI);
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    IsHide = false;
                }
                else
                {
                    RemoveChild(back);
                    RemoveChild(backu);
                    RemoveChild(backd);
                    Rmall();
                    RemoveChild(Crystal_Delete_Back);
                    RemoveChild(Crystal_Adding_UI);
                    SoundEngine.PlaySound(SoundID.MenuClose);
                    IsHide = true;
                }
            }
        }
        public void Apall()
        {
            for (int i = 0; i <= 8; i++)
                Append(Nine_Crystal_Panel[i]);
        }
        public void Rmall()
        {
            for (int i = 0; i <= 8; i++)
                RemoveChild(Nine_Crystal_Panel[i]);
        }
    }
    //负责绘制魔力量和魔法构建按钮的地方
    public class Single_Slot_UI : UIElement
    {
        Vector2 vector2 = new Vector2(0, 25);
        Asset<Texture2D> tex1, tex2, Slotf,Slote;
        public override void OnInitialize()
        {
            ModContent.GetInstance<BO>().Logger.Info("Single_Slot_UI OnInitialize called");
            tex2 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Magic_Crystal_Edit_UI2");
            tex1 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Magic_Crystal_Edit_UI1");
            Slotf = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Mana_Full");
            Slote = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Mana_Empty");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            vector2.X = Main.screenWidth * 0.45f;
            vector2.Y = 25;
            if (Main.mouseX > Main.screenWidth * 0.45f && Main.mouseX < Main.screenWidth * 0.45f + 35 && Main.mouseY > 15 && Main.mouseY < 66 && Main.LocalPlayer.itemAnimation == 0)
            {
                spriteBatch.Draw(tex2.Value, vector2, null, Color.White, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
                Main.hoverItemName = (string)ModContent.GetInstance<Magic_Slot_UI_System>().Create_Crystal;
            }
            else
                spriteBatch.Draw(tex1.Value, vector2, null, Color.White, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
            if (Main.LocalPlayer.HeldItem.type == ItemID.None) return;
            for (int i = 0, h = Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo; i < Main.LocalPlayer.HeldItem.GetGlobalItem<Magic_Weapon_Sets>().Max_Magic_Ammo; i++, h--) 
            {
                vector2.X = Main.screenWidth * 0.45f - 26 * (i + 1);
                vector2.Y = 35;
                if (h > 0)
                {
                    spriteBatch.Draw(Slotf.Value, vector2, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(Slote.Value, vector2, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
            }
        }
    }
    //翻页按钮和水晶构建
    public class Crystal_Adding_UI : UIElement
    {
        Asset<Texture2D> arrow1,arrow2,De1,De2;
        Vector2 su = new Vector2(Main.screenWidth * 0.45f + 92f, 76),xu = new Vector2(Main.screenWidth * 0.45f + 92f,211);
        Vector2 de = new Vector2(Main.screenWidth * 0.45f + 92f, 161);
        //记录已学习水晶并用于翻页显示
        Item[,] Available_Crystal = new Item[6, 9];
        int Page = 0;
        //不知道为什么绘制的时候总是会向右下偏一像素，为了美观只能往左上写一个像素了
        Vector2 C_0 = new Vector2(Main.screenWidth * 0.45f - 63f, 105f),
                C_1 = new Vector2(Main.screenWidth * 0.45f - 8f, 105f),
                C_2 = new Vector2(Main.screenWidth * 0.45f + 47f, 105f),
                C_3 = new Vector2(Main.screenWidth * 0.45f - 63f, 160f),
                C_4 = new Vector2(Main.screenWidth * 0.45f - 8f, 160f),
                C_5 = new Vector2(Main.screenWidth * 0.45f + 47f, 160f),
                C_6 = new Vector2(Main.screenWidth * 0.45f - 63f, 215f),
                C_7 = new Vector2(Main.screenWidth * 0.45f - 8f, 215f),
                C_8 = new Vector2(Main.screenWidth * 0.45f + 47f, 215f),
                //删除按钮的绘制位置，待填·······
                C_DE = new Vector2();
        public override void OnInitialize()
        {
            arrow1 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/arrow1");
            arrow2 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/arrow2");
            De1 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Remove_Crystal_1");
            De2 = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Remove_Crystal_2");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            //主要是绘制贴图和文本，左键交互什么的交给uistate处理，晚些时间再写，傻逼学校留了一堆笔记我还要补呢
            //哈哈，暑假过了一个多月我才再一次打开这个，妈的时间没有了，总之赶紧写吧
            //第一部分，翻页箭头绘制
            base.Draw(spriteBatch);
            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 70 && Main.mouseY < 115 && Main.LocalPlayer.itemAnimation == 0)
            {
                spriteBatch.Draw(arrow2.Value, su, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                Main.LocalPlayer.mouseInterface = true;
            }
            else
                spriteBatch.Draw(arrow1.Value, su, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 205 && Main.mouseY < 250 && Main.LocalPlayer.itemAnimation == 0)
            {
                spriteBatch.Draw(arrow2.Value, xu, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
                Main.LocalPlayer.mouseInterface = true;
            }
            else
                spriteBatch.Draw(arrow1.Value, xu, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
            //绷不住了，这一行代码写完半年后我才回来继续写，早忘了要写的是什么了,总之赶紧写
            //然后是，水晶的召唤和翻页互动渲染，我擦啊，要写九个
            /*
             * 按照
             * 0 1 2 
             * 3 4 5
             * 6 7 8
             * 的顺序写喽
             */
            Check_Learned_Crystal();
            D_Cryastal(spriteBatch, Page, 0, Main.screenWidth * 0.45f - 84, 83, C_0);
            D_Cryastal(spriteBatch, Page, 1, Main.screenWidth * 0.45f - 29, 83, C_1);
            D_Cryastal(spriteBatch, Page, 2, Main.screenWidth * 0.45f + 26, 83, C_2);
            D_Cryastal(spriteBatch, Page, 3, Main.screenWidth * 0.45f - 84, 138, C_3);
            D_Cryastal(spriteBatch, Page, 4, Main.screenWidth * 0.45f - 29, 138, C_4);
            D_Cryastal(spriteBatch, Page, 5, Main.screenWidth * 0.45f + 26, 138, C_5);
            D_Cryastal(spriteBatch, Page, 6, Main.screenWidth * 0.45f - 84, 193, C_6);
            D_Cryastal(spriteBatch, Page, 7, Main.screenWidth * 0.45f - 29, 193, C_7);
            D_Cryastal(spriteBatch, Page, 8, Main.screenWidth * 0.45f + 26, 193, C_8);
            Remove_An_Crystal_Draw(spriteBatch, de);
        }
        //绘制水晶介绍及贴图的简便方法
        public void D_Cryastal(SpriteBatch spriteBatch, int Page, int Index, float L, float U, Vector2 C)
        {
            if (Available_Crystal[Page, Index] != null)
            {
                spriteBatch.Draw(TextureAssets.Item[Available_Crystal[Page, Index].type].Value, C, null, Color.White, 0, TextureAssets.Item[Available_Crystal[Page, Index].type].Size() * 0.5f, 1, SpriteEffects.None, 0);
                if (Main.mouseX > L && Main.mouseX < L + 44 && Main.mouseY > U && Main.mouseY < U + 44 && Main.LocalPlayer.itemAnimation == 0) 
                {
                    Main.HoverItem = Available_Crystal[Page, Index].Clone();
                    Main.hoverItemName = Main.HoverItem.Name;
                }
            }
        }
        //翻页按钮交互音效
        //以及后来追加的添加水晶互动，大的要来喽
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            //上翻
            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 70 && Main.mouseY < 115 && Main.LocalPlayer.itemAnimation == 0)
            {
                if (Page > 0)
                {
                    Page--;
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
            }
            //下翻
            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 205 && Main.mouseY < 250 && Main.LocalPlayer.itemAnimation == 0 && Page < 5) 
            {
                if (Available_Crystal[Page + 1, 0] != null)
                {
                    Page++;
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
            }
            Add_Crystal_UI(Main.screenWidth * 0.45f - 84, 83, 0);
            Add_Crystal_UI(Main.screenWidth * 0.45f - 29, 83, 1);
            Add_Crystal_UI(Main.screenWidth * 0.45f + 26, 83, 2);
            Add_Crystal_UI(Main.screenWidth * 0.45f - 84, 138, 3);
            Add_Crystal_UI(Main.screenWidth * 0.45f - 29, 138, 4);
            Add_Crystal_UI(Main.screenWidth * 0.45f + 26, 138, 5);
            Add_Crystal_UI(Main.screenWidth * 0.45f - 84, 193, 6);
            Add_Crystal_UI(Main.screenWidth * 0.45f - 29, 193, 7);
            Add_Crystal_UI(Main.screenWidth * 0.45f + 26, 193, 8);
            Remove_An_Crystal_Action();
        }
        //添加水晶的互动方法
        public void Add_Crystal_UI(float L,float U,int Index)
        {
            if (Main.LocalPlayer == null)
                return;
            if (Main.mouseX > L && Main.mouseX < L + 44 && Main.mouseY > U && Main.mouseY < U + 44 && Main.LocalPlayer.itemAnimation == 0 && Available_Crystal[Page, Index] != null) 
            {
                Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Add_Crystal(Magic_Slot_Template.Crystal_Convert_Projecile(Available_Crystal[Page, Index].type));
            }
        }
        //交互区域设置
        public override bool ContainsPoint(Vector2 point)
        {
            if (Main.mouseX > Main.screenWidth * 0.45f - 95f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY < 250 && Main.mouseY > 70)
                return true;
            return false;
        }
        //重置ui横坐标防止错位
        public override void Recalculate()
        {
            base.Recalculate();
            if (Parent != null)
            {
                Main.instance.Window.Title = "controling player:" + Main.LocalPlayer.name;
                su.X = Main.screenWidth * 0.45f + 92f;
                xu.X = Main.screenWidth * 0.45f + 92f;
                C_0.X = Main.screenWidth * 0.45f - 63f;
                de.X = Main.screenWidth * 0.45f + 92f;
            }  
        }
        //读取玩家已学习水晶并放入二维数组
        public void Check_Learned_Crystal()
        {
            int a = 0, b = 0;
            for (; a < 54; a++)
                if (Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Has_Learned_Magic[a] == true)
                {
                    Available_Crystal[b / 9, b % 9] = new Item(Magic_Slot_Sets.Index_To_Crystal_Item_Type(a));
                    b++;
                }
        }
        //清空二维数组，防止本地玩家ui状态混淆
        public void Clear()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 9; j++)
                    Available_Crystal[i, j] = null;
        }
        //移除最右侧水晶的按钮绘制
        public void Remove_An_Crystal_Draw(SpriteBatch spriteBatch,Vector2 C)
        {

            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 155 && Main.mouseY < 200 && Main.LocalPlayer.itemAnimation == 0)
            {
                spriteBatch.Draw(De2.Value, C, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                Main.LocalPlayer.mouseInterface = true;
            }
            else
                spriteBatch.Draw(De1.Value, C, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        //移除最右侧水晶的按钮互动
        public void Remove_An_Crystal_Action()
        {
            if (Main.mouseX > Main.screenWidth * 0.45f + 85f && Main.mouseX < Main.screenWidth * 0.45f + 130f && Main.mouseY > 155 && Main.mouseY < 200 && Main.LocalPlayer.itemAnimation == 0 && Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Max_Using_Active() > 0) 
            {
                Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Magic_Slot.Clear_Crystal();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
    }
    //绘制当前添加的水晶数量，活力值，活力上限等属性，总之是个很大的状态栏
    public class Crystal_Using_State : UIElement
    {
        //一种水晶一个变量
        Asset<Texture2D>[] Crystal_Empty,Crystal_Book_Of_Leaves;
        float Draw_position = Main.screenWidth * 0.45f + 50f;
        //获取本地玩家的水晶用的变量
        Magic_Slot_Template My_Player_Magic_Slot;
        public override void OnInitialize()
        {
            Crystal_Empty =
            [
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Empty_0"),
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Empty_1")
            ];
            Crystal_Book_Of_Leaves =
            [
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Book_Of_Leaves_0"),
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Book_Of_Leaves_1")
            ];
        }
        //绘制逻辑，最囊的地方
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (Main.LocalPlayer != null)
                My_Player_Magic_Slot = Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Magic_Slot;
            else
                return;
            Draw_position = Main.screenWidth * 0.45f + 50f;
            for (int i = 0; true; i++)
            {
                for (int j = 0; j < My_Player_Magic_Slot.Magic_Slots[i].Active_Power; j++, Draw_position += 19f) 
                {
                    spriteBatch.Draw(Get_Texture(My_Player_Magic_Slot.Magic_Slots[i].Magic_Barrier_Crystal_Type, 1), new Vector2(Draw_position, 50f), null, Color.White, 0f, Get_Texture(My_Player_Magic_Slot.Magic_Slots[i].Magic_Barrier_Crystal_Type, 1).Size() * 0.5f, 1f, SpriteEffects.None, 0);
                }
                for (int j = 0; j < My_Player_Magic_Slot.Magic_Slots[i].Internal_Max_Active - My_Player_Magic_Slot.Magic_Slots[i].Active_Power; j++, Draw_position += 19f) 
                {
                    spriteBatch.Draw(Get_Texture(My_Player_Magic_Slot.Magic_Slots[i].Magic_Barrier_Crystal_Type, 0), new Vector2(Draw_position, 50f), null, Color.White, 0f, Get_Texture(My_Player_Magic_Slot.Magic_Slots[i].Magic_Barrier_Crystal_Type, 0).Size() * 0.5f, 1f, SpriteEffects.None, 0);
                }
                for (int j = 0; j < My_Player_Magic_Slot.Active - My_Player_Magic_Slot.Max_Using_Active(); j++, Draw_position += 19f)
                {
                    spriteBatch.Draw(Get_Texture(My_Player_Magic_Slot.Magic_Slots[i + 1].Magic_Barrier_Crystal_Type, 1), new Vector2(Draw_position, 50f), null, Color.White, 0f, Get_Texture(My_Player_Magic_Slot.Magic_Slots[i + 1].Magic_Barrier_Crystal_Type, 1).Size() * 0.5f, 1f, SpriteEffects.None, 0);
                }
                for (int j = 0; j < My_Player_Magic_Slot.Max_Active - My_Player_Magic_Slot.Active - My_Player_Magic_Slot.Max_Using_Active(); j++, Draw_position += 19f) 
                {
                    spriteBatch.Draw(Get_Texture(My_Player_Magic_Slot.Magic_Slots[i + 1].Magic_Barrier_Crystal_Type, 0), new Vector2(Draw_position, 50f), null, Color.White, 0f, Get_Texture(My_Player_Magic_Slot.Magic_Slots[i + 1].Magic_Barrier_Crystal_Type, 0).Size() * 0.5f, 1f, SpriteEffects.None, 0);
                }
                if (My_Player_Magic_Slot.Magic_Slots[i].Internal_Max_Active > My_Player_Magic_Slot.Magic_Slots[i].Active_Power || My_Player_Magic_Slot.Magic_Slots[i].Internal_Max_Active == 0)  
                {
                    break;
                }
            }
        }
        //ui贴图的话，我打算宽度强制限制在9，为了清晰度以及贴图大小匹配的话，那就是18，高度随意，可能要在竖直方向上加一点创意元素，不过都是要以中间为原点，而且必须为4的倍数，就算是2的倍数不是4的倍数也得留两个空凑一凑
        //初始化获取贴图
        public Crystal_Using_State() 
        {
            Crystal_Empty =
            [
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Empty_0"),
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Empty_1")
            ];
            Crystal_Book_Of_Leaves =
            [
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Book_Of_Leaves_0"),
                ModContent.Request<Texture2D>("BO/Content/another/Magic_State_Image/Crystal_Book_Of_Leaves_1")
            ];
        }
        //返回所需要的贴图，0代表无活力，非0代表有活力
        public Texture2D Get_Texture(int crystal_Type, int active) 
        {
            if (active != 0)
                active = 1;
            if (crystal_Type == 0)
            {
                return Crystal_Empty[active].Value;
            }
            if (crystal_Type == ModContent.ProjectileType<Book_Of_Leaves_Crystal>())
            {
                return Crystal_Book_Of_Leaves[active].Value;
            }
            return null;
        }
    }
    //改变全物品的魔力机制，同时设定特定武器的可收集魔力量上限
    public class Magic_Weapon_Sets : GlobalItem
    {
        //这里的Max_Magic_Ammo仅用来绘制界面，真正的控制逻辑我放在了各个法杖各自的源码里面
        public int Max_Magic_Ammo = 0;
        public override void SetDefaults(Item item)
        {
            item.mana = 0;
            if (item.type == ItemID.WandofSparking) Max_Magic_Ammo = 1;
        }
        public override bool InstancePerEntity => true;
    }
    //把原版的魔力绘制隐藏
    public class Hide_Mana : ModResourceOverlay
    {
        public override bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText)
        {
            if (drawingLife)
            {
                drawText = true;
                return true;
            }
            else
            {
                drawText = false;
                return false;
            }
        }
    }
    //便于进行多人同步的水晶弹幕基类，以后写水晶弹幕必须继承这个，否则魔力系统绝对会报错，还有就是这里还得多写点多人同步的代码
    public abstract class Crystal_Projectile : ModProjectile
    {
        protected int Crystal_num = 0, Active_Power = 0;
        public void Sync(int Crystal_num_S, int Active_Power_S)
        {
            Crystal_num = Crystal_num_S;
            Active_Power = Active_Power_S;
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Crystal_num);
            writer.Write(Active_Power);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Crystal_num = reader.ReadInt32();
            Active_Power = reader.ReadInt32();
        }
    }
}