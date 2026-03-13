using System;
using System.Runtime.InteropServices;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BO.Content.Items.Vine_Bow
{
    public class Vine_Bow : ModItem
    {
        UnifiedRandom r = new UnifiedRandom();
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.damage = 10;
            Item.knockBack = 1.2f;
            Item.UseSound = SoundID.Item5;
            Item.shootSpeed = 7f;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = AmmoID.Arrow;
            Item.DamageType = DamageClass.Ranged;
            Item.rare = ItemRarityID.Blue;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (r.Next(0, 5) == 0)
            {
                Projectile.NewProjectile(player.GetSource_FromThis(), position, velocity * 1.3f, ModContent.GetInstance<Vine_Bow_p1>().Type, damage, knockback * 1.5f);
            }
            return true;
        }
    }
    public class Vine_Bow_p1 : ModProjectile
    {
        Vector2[] a = new Vector2[5];
        Vector3 b = new Vector3(34, 139, 34);
        public override void SetDefaults()
        {
            Projectile.timeLeft = 80;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.scale = 1.2f;
            Main.projFrames[Projectile.type] = 4;
        }
        public override void AI()
        {
            Projectile.frame = Projectile.timeLeft % 24 / 6 - 1;
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.ToRadians(90f);
            Projectile.velocity *= 0.96f;
            for (int i = 3; i >= 0; i--)
                a[i + 1] = a[i];
            a[0] = Projectile.Center;
            Lighting.AddLight(Projectile.Center, b * 0.002f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < 5 && Projectile.timeLeft > 30; i++) 
                Main.spriteBatch.Draw(ModContent.Request<Texture2D>("BO/Content/Items/Vine_Bow/trail").Value, a[i] - Main.screenPosition, null, Color.White * 0.1f * (5 - i), (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.ToRadians(90f), ModContent.Request<Texture2D>("BO/Content/Items/Vine_Bow/trail").Value.Size() * 0.5f, 1.2f, SpriteEffects.None, 0);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
            for (int i = 0; i < 3; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades);
        }
    }
}