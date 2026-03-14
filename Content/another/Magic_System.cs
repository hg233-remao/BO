using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BO.Content.Items.Magic.Wands.Wand_Of_Sparking;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
public enum Crystal_ID
{
    None,
    Book_Of_Leaves_Crystal
}

namespace BO.Content.Items.Magic.Magic_System
{
    public class Magic_Slot_Template
    {
        int Max_Slots_Count = 0;
        int Current_Slot_Count = 0;
        int Current_Slot_Index = 0;
        int Active_Slot_Count = 0;
        int Active_Slot_Index = 0;
        public struct Magic_Barrier_Crystal_In_Slots
        {
            public Crystal_ID Magic_Barrier_Crystal_Type;
            public int Magic_Barrier_Crystal_Space;
            public bool Is_Active;
            public Magic_Barrier_Crystal_In_Slots()
            { 
                Magic_Barrier_Crystal_Type = 0;
                Magic_Barrier_Crystal_Space = 0;
                Is_Active = false;
            }
        }
        public Magic_Barrier_Crystal_In_Slots[] Magic_Slots = new Magic_Barrier_Crystal_In_Slots[50];
        public void Add_Crystal(int Adding_Type,int Adding_Space) 
        {
            if (Current_Slot_Count < Max_Slots_Count && Adding_Space + Current_Slot_Count <= Max_Slots_Count)  
            {
                Magic_Slots[Current_Slot_Index + 1].Magic_Barrier_Crystal_Type += Adding_Type;
                Magic_Slots[Current_Slot_Index + 1].Magic_Barrier_Crystal_Space += Adding_Space;
                Current_Slot_Index++;
            }
        }
        public void Remove_Last_Crystal()
        {
            if (Current_Slot_Count > 0 && Max_Slots_Count > 0)
            {
                Current_Slot_Count -= Magic_Slots[Current_Slot_Index].Magic_Barrier_Crystal_Space;
                Magic_Slots[Current_Slot_Index].Magic_Barrier_Crystal_Type = 0;
                Magic_Slots[Current_Slot_Index].Magic_Barrier_Crystal_Space = 0;
                Current_Slot_Index--;
            }
        }
        public void Add_Active()
        {
            Active_Slot_Index++;
        }
        public void Remove_An_Active()
        {
            if (Active_Slot_Index > 0) 
            Active_Slot_Index--;
        }
        public void Remove_All_Active()
        {
            Active_Slot_Index = 0;
        }
    }
    //设置玩家相关的魔力方面的特性
    public class Magic_Slot_Sets : ModPlayer
    {
        public Magic_Slot_Template Magic_Slot = new Magic_Slot_Template();
        public int Magic_Ammo = 0;
        public bool Full_Entity_Power = false;
        public int Magic_Power_Cooldown = 0;
        public int Hold_Item_Before;
        public bool[] Has_Learned_Magic = new bool[50];  
        public override void OnHurt(Player.HurtInfo info)
        {
            Magic_Slot.Remove_An_Active();
        }
        //检测玩家上一帧
        public override void PreUpdate()
        {
            if (Main.LocalPlayer.whoAmI != Player.whoAmI || Main.netMode == NetmodeID.Server) return;
            Hold_Item_Before = Player.HeldItem.type;
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
        bool IsHide=true;
        Single_Slot_UI Single_Slot_UI = new Single_Slot_UI();
        Crystal_Adding_UI Crystal_Adding_UI = new Crystal_Adding_UI();
        UIPanel back = new UIPanel();
        UIPanel backu = new UIPanel();
        UIPanel backd = new UIPanel();
        public override void OnInitialize()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Width.Set(0, 1f);
            Height.Set(0, 1f);
            Left.Set(0, 0f);
            Top.Set(0, 0f);
            Append(Single_Slot_UI);
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
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.netMode == NetmodeID.Server) return;
            back.Left.Set(Main.screenWidth * 0.45f - 95f, 0f);
            backu.Left.Set(Main.screenWidth * 0.45f + 85f, 0f);
            backd.Left.Set(Main.screenWidth * 0.45f + 85f, 0f);
            if (Main.mouseX > Main.screenWidth * 0.45f && Main.mouseX < Main.screenWidth * 0.45f + 35 && Main.mouseY > 15 && Main.mouseY < 66 && Main.LocalPlayer.itemAnimation == 0) 
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
                    Append(Crystal_Adding_UI);
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    IsHide = false;
                }
                else
                {
                    RemoveChild(back);
                    RemoveChild(backu);
                    RemoveChild(backd);
                    RemoveChild(Crystal_Adding_UI);
                    SoundEngine.PlaySound(SoundID.MenuClose);
                    IsHide = true;
                }
            }
        }
    }
    //负责绘制魔力量和魔法构建按钮的地方
    public class Single_Slot_UI : UIElement
    {
        Vector2 vector2 = new Vector2(0, 25);
        Texture2D tex, Slot;
        public override void Draw(SpriteBatch spriteBatch)
        {
            vector2.X = Main.screenWidth * 0.45f;
            vector2.Y = 25;
            if (Main.mouseX > Main.screenWidth * 0.45f && Main.mouseX < Main.screenWidth * 0.45f + 35 && Main.mouseY > 15 && Main.mouseY < 66 && Main.LocalPlayer.itemAnimation == 0)
            {
                tex = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Magic_Crystal_Edit_UI2").Value;
                Main.hoverItemName = (string)ModContent.GetInstance<Magic_Slot_UI_System>().Create_Crystal;
            }
            else
                tex = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Magic_Crystal_Edit_UI1").Value;
            spriteBatch.Draw(tex, vector2, null, Color.White, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
            if (Main.LocalPlayer.HeldItem.type == ItemID.None) return;
            for (int i = 0, h = Main.LocalPlayer.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo; i < Main.LocalPlayer.HeldItem.GetGlobalItem<Magic_Weapon_Sets>().Max_Magic_Ammo; i++, h--) 
            {
                vector2.X = Main.screenWidth * 0.45f - 26 * (i + 1);
                vector2.Y = 35;
                if (h > 0)
                {
                    Slot = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Mana_Full").Value;
                }
                else
                {
                    Slot = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/Mana_Empty").Value;
                }
                spriteBatch.Draw(Slot, vector2, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }
    }
    public class Crystal_Adding_UI : UIElement
    {
        Texture2D arrow;
        Vector2 pu = new Vector2(Main.screenWidth * 0.45f + 92f, 76);
        public override void Draw(SpriteBatch spriteBatch)
        {
            //主要是绘制贴图和文本，左键交互什么的交给uistate处理，晚些时间再写，傻逼学校留了一堆笔记我还要补呢
            //哈哈，暑假过了一个多月我才再一次打开这个，妈的时间没有了，总之赶紧写吧
            //第一部分，翻页箭头绘制
            arrow = ModContent.Request<Texture2D>("BO/Content/another/Magic_Image/arrow").Value;
            spriteBatch.Draw(arrow, pu, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            //绷不住了，这一行代码写完半年后我才回来继续写，早忘了要写的是什么了
        }
        public override void Recalculate()
        {
            if (Parent!=null)
                pu.X = Main.screenWidth * 0.45f + 92f;
            //base.Recalculate();
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
}