using System;
//using Microsoft.Office.Interop.Excel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BO.Content.Items.Swords.Wooden_Sword
{
    public class Wooden_Sword : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type != ItemID.WoodenSword) return;
            item.useStyle = ItemUseStyleID.Shoot;
            item.noMelee = true;
            item.shoot = ModContent.GetInstance<Wooden_Sword_p1>().Type;
            item.shootSpeed = 1f;
            item.noUseGraphic = true;
            item.useTime = 60;
            item.useAnimation = 60;
        }
    }
    public class Wooden_Sword_p1 : ModProjectile
    {
        UnifiedRandom r = new UnifiedRandom();
        public override void SetDefaults()
        {
            Projectile.timeLeft = 12;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            DrawOriginOffsetY = -8;
            Main.projFrames[Projectile.type] = 6;
        }
        public override void AI()
        {
            Projectile.frame = (12 - Projectile.timeLeft) / 2;
            Projectile.Center = Main.player[Projectile.owner].Center + (Projectile.velocity / Projectile.velocity.Length()) * 30f;
            Projectile.position.Y += 3;
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            if (Projectile.rotation > Math.Atan2(1, 0) || Projectile.rotation < (float)Math.Atan2(-1, 0))  
            {
                Projectile.spriteDirection = Projectile.direction;
                Projectile.rotation += MathHelper.ToRadians(180f);
            }
            if (Projectile.Center.X >= Main.player[Projectile.owner].Center.X) Main.player[Projectile.owner].direction = 1;
            else Main.player[Projectile.owner].direction = -1;
        }
        //我是弄点木屑好呢，还是弄个击中特效好呢，还是都弄上才好呢
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++) 
            Dust.NewDust(target.position, target.width, target.height, DustID.t_LivingWood);

            Projectile.NewProjectile(Main.player[Projectile.owner].GetSource_FromThis(), new Vector2(target.position.X + r.Next(0, target.width), target.position.Y + r.Next(0, target.height)), Vector2.Zero, ModContent.GetInstance<Wooden_Sword_Projectile_Effect>().Type, 0, 0);
        }
    }
    public class Wooden_Sword_Projectile_Effect : ModProjectile
    {
        UnifiedRandom r = new UnifiedRandom();
        public override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.tileCollide = false;
            Main.projFrames[Projectile.type] = 5;
        }
        public override void AI()
        {
            Projectile.frame = (10 - Projectile.timeLeft) / 2;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = MathHelper.ToRadians(r.NextFloat(0f, 360f));
        }
    }
}