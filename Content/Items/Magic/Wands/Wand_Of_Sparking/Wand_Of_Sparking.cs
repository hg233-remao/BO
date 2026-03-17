using System;
using System.Text;
using BO.Content.another.Magic.Magic_System;
using Humanizer;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;


namespace  BO.Content.Items.Magic.Wands.Wand_Of_Sparking
{
    public class Wand_Of_Sparking : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.WandofSparking) return;
            item.shoot = ProjectileID.Flames;
            item.useTime = 5;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useAnimation = 80;
            item.UseSound = SoundID.Item74;
        }
        //手持火花魔杖的时候进行的魔力充能生成逻辑
        public override void HoldItem(Item item, Player player)
        {
            if (item.type != ItemID.WandofSparking || player != Main.LocalPlayer) return;
            if (player.GetModPlayer<Magic_Slot_Sets>().Hold_Item_Before != item.type)
            {
                player.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo = 0;
                player.GetModPlayer<Magic_Slot_Sets>().Magic_Power_Cooldown = 600;
            }
            if (player.GetModPlayer<Player_Has_Wand_Of_Sparking_Ammo_Projectile>().Has_Wand_Of_Sparking_Ammo_Projectile || (player.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo > 0)) 
                return;
            if (player.GetModPlayer<Magic_Slot_Sets>().Magic_Power_Cooldown > 0)
            {
                player.GetModPlayer<Magic_Slot_Sets>().Magic_Power_Cooldown--;
                return;
            }
            Vector2 a = new Vector2();
            UnifiedRandom r = new UnifiedRandom();
            a.X = r.NextFloat(-1, 1);
            a.Y = r.NextFloat(-2, 1);
            if (!player.GetModPlayer<Magic_Slot_Sets>().Full_Entity_Power)
                Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, a / a.Length() * 10f, ModContent.GetInstance<Wand_Of_Sparking_Ammo_Projectile>().Type, 0, 0);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type != ItemID.WandofSparking) return base.CanUseItem(item, player);
            if (player.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo > 0) return true;
            return false;
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type != ItemID.WandofSparking) return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
            if (player.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo > 0)
            {
                player.GetModPlayer<Magic_Slot_Sets>().Magic_Ammo--;
                player.GetModPlayer<Magic_Slot_Sets>().Magic_Power_Cooldown = 600;
            }
            return true;
        }
    }
    public class Wand_Of_Sparking_Ammo : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DirtBlock);
        }
    }
    //“魔力聚合素”的行动逻辑
    public class Wand_Of_Sparking_Ammo_Projectile : ModProjectile
    {
        bool Transform = false;
        Vector3 c = new Vector3(255, 255, 0);
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60;
            Main.projFrames[Projectile.type] = 7;
            Projectile.frame = 6;
            Projectile.width = 32;
            Projectile.height = 48;
            Projectile.tileCollide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Main.player[Projectile.owner].GetModPlayer<Player_Has_Wand_Of_Sparking_Ammo_Projectile>().Has_Wand_Of_Sparking_Ammo_Projectile = true;
        }
        public override void AI()
        {
            if (Main.LocalPlayer != Main.player[Projectile.owner]|| (Projectile.Center - Main.player[Projectile.owner].Center).Length() >= 640) Projectile.Kill();
            Projectile.timeLeft += 2;
            if (Transform == false)
            {
                if ((Projectile.Center - Main.player[Projectile.owner].Center).Length() >= 320)
                {
                    Transform = true;
                    SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
                    Projectile.velocity = Vector2.Zero;
                }
            }
            if (Transform == true)
            {
                Projectile.frame = (int)Main.time % 24 / 4;
                if ((Projectile.Center - Main.player[Projectile.owner].Center).Length() > 50) 
                    Projectile.velocity = Vector2.Zero;
                else 
                    Projectile.velocity = (Main.player[Projectile.owner].Center - Projectile.Center) / (Main.player[Projectile.owner].Center - Projectile.Center).Length() * 5f;
                Lighting.AddLight(Projectile.Center, c * 0.003f);
                if ((Projectile.Center - Main.player[Projectile.owner].Center).Length() <= 20)
                {
                    Main.player[Projectile.owner].GetModPlayer<Magic_Slot_Sets>().Magic_Ammo++;
                    SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                    Projectile.Kill();
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Transform = true;
            SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            Main.player[Projectile.owner].GetModPlayer<Player_Has_Wand_Of_Sparking_Ammo_Projectile>().Has_Wand_Of_Sparking_Ammo_Projectile = false;
        }
    }
    public class Player_Has_Wand_Of_Sparking_Ammo_Projectile : ModPlayer
    {
        public bool Has_Wand_Of_Sparking_Ammo_Projectile = false;
    }



}


