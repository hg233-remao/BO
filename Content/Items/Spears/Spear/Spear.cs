using System;
using BO.Content.another.spearglobalsets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace BO.Content.Items.Spears.Spear
{
    public class Spear : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.Spear) return;
            item.damage = 30;
            item.autoReuse = false;
            item.useTime = 61;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.GetInstance<SpearProjectile>().Type;
            item.shootSpeed = 1f;
            item.noMelee = true;
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.Spear && Main.LocalPlayer.HeldItem == item) return false;
            return base.CanUseItem(item, player);
        }
    }
    public class SpearProjectile : ModProjectile 
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 30;
            Projectile.width = 20;
            Projectile.height = 20;
            DrawOffsetX = 0;
            DrawOriginOffsetX = -18;
            DrawOriginOffsetY = 0;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ownerHitCheck = true;
        }
        public override void AI()
        {
            if (Projectile.timeLeft > 20)
                Main.player[Projectile.owner].velocity = Projectile.velocity * 0.3f;
            Projectile.velocity /= Projectile.velocity.Length();
            Projectile.velocity *= 25f;
            Projectile.rotation = MathHelper.ToRadians(135f) + (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            Projectile.position = Main.player[Projectile.owner].position + Projectile.velocity;
            Projectile.position.Y += 13;
            Main.player[Projectile.owner].armorEffectDrawShadow = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].velocity = Vector2.Zero;
            Projectile.timeLeft = 0;
            Main.player[Projectile.owner].itemAnimation = 0;
            if (Main.netMode == NetmodeID.MultiplayerClient) 
            {
                ModPacket sync = Mod.GetPacket();
                sync.Write("SpearHitSyncToServer");
                sync.Write(Projectile.owner);
                sync.Write(Main.player[Projectile.owner].velocity.X);
                sync.Write(Main.player[Projectile.owner].velocity.Y);
                sync.Send();
            }
        }
    }
}