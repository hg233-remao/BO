using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BO.Content.Items.Spears.Spear;
using BO.Content.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Graphics;
using Steamworks;
using Terraria.ModLoader.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.GameContent.RGB;
using System.Net.Mail;
using BO.Content.Items.Swords.Living_Wooden_sword;
using Terraria.Audio;
namespace BO.Content.another.spearglobalsets
{
    public class spearglobalsets : GlobalItem
    {
        public bool isSword = false;
        public bool isSpear = false;
        public int dashCD = 0;
        public override void SetDefaults(Item item)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (item.type == ItemID.Spear)
            {
                dashCD = 480;
                isSpear = true;
            }
            //上矛下剑
            if (item.type == ItemID.WoodenSword)
            { 
                isSword = true;
            }
            if (item.type == ModContent.GetInstance<Living_Wooden_Sword>().Type) 
            {
                isSword = true;
            }
        }
        public override bool InstancePerEntity => true;
    }
    public class setspearslot : ModPlayer
    {
        public Item[] spearslot = new Item[1];
        public Item c;
        int dashCD;
        //int a;
        public override void Initialize()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Player.whoAmI != Main.myPlayer) return;
            spearslot[0] = new Item();
            c = new Item();
        }
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Player.whoAmI != Main.myPlayer) return;
            dashCD = 0;
            c = new Item();
        }
        
        public override void PreUpdate()
        {
            //妈的，研究半天为什么一有多人玩家的冲刺就会出问题，一直在搞同步，结果最后发现加个下面这条就完美解决了，我草泥马
            if (Main.netMode == NetmodeID.Server || Player.whoAmI != Main.myPlayer) return;
            if (dashCD != 0) dashCD--;
            if (Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].type == ItemID.None || Main.LocalPlayer.HeldItem.type == ItemID.None) return;
            if (Main.mouseRight && Main.LocalPlayer.itemAnimation == 0 && dashCD == 0 && !Main.LocalPlayer.mouseInterface && !Main.LocalPlayer.dead && Main.mouseItem.type == ItemID.None && Main.LocalPlayer.HeldItem.GetGlobalItem<spearglobalsets>().isSword)  
            {
                if (c != Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0])
                c = Main.LocalPlayer.HeldItem.Clone();
                Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].Clone();
                Main.LocalPlayer.ApplyItemAnimation(Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].Clone());
                dashCD += Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].GetGlobalItem<spearglobalsets>().dashCD;
                /*if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket SpearSlotSync = Mod.GetPacket();
                    SpearSlotSync.Write("SpearSlotSyncTS");
                    SpearSlotSync.Write(Main.LocalPlayer.whoAmI);
                    SpearSlotSync.Write(Main.player[Main.LocalPlayer.whoAmI].GetModPlayer<setspearslot>().c.type);
                    SpearSlotSync.Write(Main.player[Main.LocalPlayer.whoAmI].GetModPlayer<setspearslot>().c.prefix);
                    SpearSlotSync.Write(Main.player[Main.LocalPlayer.whoAmI].GetModPlayer<setspearslot>().c.stack);
                    SpearSlotSync.Send();
                }*/
            }
            //Main.NewText(Main.LocalPlayer.GetModPlayer<setspearslot>().dashCD);
            if (Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].IsNotSameTypePrefixAndStack(c.Clone()) && !Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].IsNotSameTypePrefixAndStack(Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0]) && Main.LocalPlayer.itemAnimation == 0) 
            {
                Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] = c.Clone();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["spear"] = ItemIO.Save(spearslot[0]);
        }
        public override void LoadData(TagCompound tag)
        {
            spearslot[0] = ItemIO.Load(tag.Get<TagCompound>("spear"));
        }
    }
    public class spearimage : UIElement
    {
        //绘制矛物品槽贴图
        Vector2 vector2 = new Vector2(595, 212);
        Texture2D tex;
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            tex = (Texture2D)TextureAssets.Item[Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].type];
            spriteBatch.Draw(tex, vector2, null, Color.White, 0, tex.Size() * 0.5f, 1, SpriteEffects.None, 0);
            if (Main.mouseX >= 570 && Main.mouseX <= 620 && Main.mouseY >= 187 && Main.mouseY <= 237 && Main.LocalPlayer.itemAnimation == 0)
            {
                Main.HoverItem = Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].Clone();
                Main.hoverItemName = (string)ModContent.GetInstance<drawspearslot>().spearorlance;
            }
        }
    }
    public class spearslotui : UIState
    {
        //矛物品槽互动等功能
        UIPanel panel = new UIPanel();
        spearimage s=new spearimage();
        Item c;
        bool open = false;
        public override void OnInitialize()
        {
            if (Main.netMode == NetmodeID.Server) return;
            Width.Set(50, 0f);
            Height.Set(50, 0f);
            Left.Set(570, 0f);
            Top.Set(187, 0f);
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Main.mouseX >= 570 && Main.mouseX <= 620 && Main.mouseY >= 187 && Main.mouseY <= 237 && Main.LocalPlayer.itemAnimation == 0 && Main.playerInventory) 
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (Main.playerInventory)
            {
                panel.Width.Set(0, 1f);
                panel.Height.Set(0, 1f);
                if (!open) 
                {
                    open = true;
                    Append(panel);
                    Append(s);
                }
            }
            else
            {
                open = false;
                RemoveAllChildren();
            }
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            if (Main.netMode == NetmodeID.Server) return;
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Main.netMode == NetmodeID.Server || (Main.mouseItem.type != ItemID.None && !Main.mouseItem.GetGlobalItem<spearglobalsets>().isSpear)||!Main.playerInventory) return;
            if (Main.netMode == NetmodeID.Server || Main.LocalPlayer.itemAnimation != 0) return;
            c = Main.mouseItem.Clone();
            Main.mouseItem = Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].Clone();
            Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0] = c.Clone();
            if (!(Main.mouseItem.Clone().type == ItemID.None && Main.LocalPlayer.GetModPlayer<setspearslot>().spearslot[0].Clone().type == ItemID.None)) SoundEngine.PlaySound(SoundID.Grab);
        }
    }
    public class drawspearslot : ModSystem
    {
        //ui前置
        public UserInterface MyInterface;
        internal spearslotui s;
        private GameTime beforetimeui;
        public LocalizedText spearorlance;
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Main.netMode != NetmodeID.Server) MyInterface = new UserInterface();
            s = new spearslotui();
            //s.Activate();
            MyInterface.SetState(s);
            spearorlance = Mod.GetLocalization(nameof(spearorlance));
        }
        public override void Unload()
        {
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.netMode == NetmodeID.Server) return;
            beforetimeui = gameTime;
            if (MyInterface?.CurrentState != null)
                MyInterface.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.netMode == NetmodeID.Server) return;
            int a = layers.FindIndex(layers => layers.Name.Equals("Vanilla: Mouse Text"));
            if (a != -1)
            {
                layers.Insert(a, new LegacyGameInterfaceLayer("spear slot", delegate
                {
                    if (beforetimeui != null && MyInterface?.CurrentState != null)
                    {
                        MyInterface.Draw(Main.spriteBatch, beforetimeui);
                    }
                    return true;
                }, InterfaceScaleType.UI
                ));
            }
        }
    }
}