using System;
using System.IO.Pipelines;
using BO.Content.Items.Swords.Wooden_Sword;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using Steamworks;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.RGB;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
//破木剑浪费我好长时间啊玛德

namespace BO.Content.Items.Swords.Living_Wooden_sword
{
    public class Living_Wooden_Sword : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.damage = 10;
            Item.DamageType = DamageClass.Melee;
            //Item.shoot = ModContent.GetInstance<Living_Wooden_Sword_p1>().Type;
            Item.shoot = ModContent.GetInstance<Living_Wooden_Sword_p2>().Type;
            Item.shootSpeed = 12f;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item1;
        }
        public override void HoldItem(Player player)
        {
            if (!player.GetModPlayer<Living_Wooden_Sword_Status>().hasp1) Item.shoot = ModContent.GetInstance<Living_Wooden_Sword_p1>().Type;
            else
                Item.shoot = ModContent.GetInstance<Living_Wooden_Sword_p2>().Type;
        }
    }
    public class Living_Wooden_Sword_p1 : ModProjectile
    {
        Texture2D tex = ModContent.Request<Texture2D>("BO/Content/Items/Swords/Living_Wooden_sword/Vine").Value;
        public int? targetn = null;
        int hitcount = 0;
        bool back = false;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 30;
            Projectile.width = 12;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.localNPCHitCooldown = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Main.player[Projectile.owner]==null)Projectile.Kill();
            if (back)
            {
                Projectile.timeLeft = 30;
                Projectile.velocity = (Main.player[Projectile.owner].Center - Projectile.Center) / (Main.player[Projectile.owner].Center - Projectile.Center).Length() * 12f;
                if ((Main.player[Projectile.owner].Center - Projectile.Center).Length() <= 20 || Main.player[Projectile.owner].dead) 
                    Projectile.Kill();
                return;
            }
            if (targetn != null && !Main.npc[(int)targetn].active || Projectile.timeLeft <= 15)
            {
                back = true;
            }
            if (targetn != null && Main.npc[(int)targetn].active)
            {
                Projectile.timeLeft = 30;
                Projectile.Center = Main.npc[(int)targetn].Center;
            }
            if (Main.player[Projectile.owner].dead) Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (targetn == null)
                targetn = target.whoAmI;
            if (targetn != null && targetn == target.whoAmI) hitcount++;
            if (hitcount >= 5) back = true;
            if (Main.netMode == NetmodeID.MultiplayerClient && targetn != null && Main.LocalPlayer.whoAmI == Projectile.owner)  
            {
                ModPacket Living_Wooden_Sword_p1SyncToMC = Mod.GetPacket();
                Living_Wooden_Sword_p1SyncToMC.Write("Living_Wooden_Sword_p1SyncToServer");
                Living_Wooden_Sword_p1SyncToMC.Write(Projectile.whoAmI);
                Living_Wooden_Sword_p1SyncToMC.Write(target.whoAmI);
                Living_Wooden_Sword_p1SyncToMC.Send();
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (back || target.friendly) return false;
            if (targetn != null && targetn == target.whoAmI) return true;
            else if (targetn == null) return true;
            else return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            back = true;
            Projectile.tileCollide = false;
            return false;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 direction = (Main.player[Projectile.owner].Center - Projectile.Center) / (Main.player[Projectile.owner].Center - Projectile.Center).Length();
            for (int a = 1; a < (Main.player[Projectile.owner].Center - Projectile.Center).Length() / 16; a++)
                Main.EntitySpriteDraw(tex, Projectile.Center + direction * 16 * a - Main.screenPosition, null, lightColor, (float)Math.Atan2(direction.Y, direction.X) + float.Pi / 2, tex.Size() * 0.5f, 1, SpriteEffects.None);
            return true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Main.player[Projectile.owner].GetModPlayer<Living_Wooden_Sword_Status>().hasp1 = true;
        }
        public override void OnKill(int timeLeft)
        {
            Main.player[Projectile.owner].GetModPlayer<Living_Wooden_Sword_Status>().hasp1 = false;
        }
        
    }
    public class Living_Wooden_Sword_p2 : ModProjectile
    {
        UnifiedRandom r = new UnifiedRandom();
        public override void SetDefaults()
        {
            Projectile.timeLeft = 16;
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 9;
            Projectile.usesLocalNPCImmunity = true;
            DrawOffsetX = -29;
            DrawOriginOffsetY = -35;
            Main.projFrames[Projectile.type] = 8;
        }
        public override void AI()
        {
            Projectile.frame = (16 - Projectile.timeLeft) / 2;
            Projectile.Center = Main.player[Projectile.owner].Center + (Projectile.velocity / Projectile.velocity.Length()) * 15f;
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) - MathHelper.ToRadians(22.5f);
            if (Projectile.rotation > Math.Atan2(1, 0) - MathHelper.ToRadians(22.5f) || Projectile.rotation < (float)Math.Atan2(-1, 0) - MathHelper.ToRadians(22.5f)) 
            {
                Projectile.spriteDirection = Projectile.direction;
                Projectile.rotation += MathHelper.ToRadians(225f);
            }
            if (Projectile.Center.X >= Main.player[Projectile.owner].Center.X) Main.player[Projectile.owner].direction = 1;
            else Main.player[Projectile.owner].direction = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (16 - Projectile.timeLeft) / 2;
            Projectile.Center = Main.player[Projectile.owner].Center + (Projectile.velocity / Projectile.velocity.Length()) * 15f;
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) - MathHelper.ToRadians(22.5f);
            if (Projectile.rotation > Math.Atan2(1, 0) - MathHelper.ToRadians(22.5f) || Projectile.rotation < (float)Math.Atan2(-1, 0) - MathHelper.ToRadians(22.5f))
            {
                Projectile.spriteDirection = Projectile.direction;
                Projectile.rotation += MathHelper.ToRadians(225f);
            }
            if (Projectile.Center.X >= Main.player[Projectile.owner].Center.X) Main.player[Projectile.owner].direction = 1;
            else Main.player[Projectile.owner].direction = -1;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDust(target.position, target.width, target.height, DustID.t_LivingWood);
            Projectile.NewProjectile(Main.player[Projectile.owner].GetSource_FromThis(), new Vector2(target.position.X + r.Next(0, target.width), target.position.Y + r.Next(0, target.height)), Vector2.Zero, ModContent.GetInstance<Wooden_Sword_Projectile_Effect>().Type, 0, 0);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 dline = Projectile.velocity * 5.2f;
            Vector2 hline = new Vector2(Main.player[Projectile.owner].Center.X, Main.player[Projectile.owner].Center.Y + 5) + dline.RotatedBy(MathHelper.ToRadians(-59f));
            for (int i = 1; i < 9; i++)
            {
                if (Collision.CheckAABBvLineCollision(new Vector2(targetHitbox.X, targetHitbox.Y), new Vector2(targetHitbox.Width, targetHitbox.Height), Main.player[Projectile.owner].position, hline))
                    return true;
                hline = new Vector2(Main.player[Projectile.owner].Center.X, Main.player[Projectile.owner].Center.Y+5) + dline.RotatedBy(MathHelper.ToRadians(-59f) + MathHelper.ToRadians(16.875f) * i);
            }
            return false;
        }
    }
    public class Living_Wooden_Sword_Status : ModPlayer
    {
        public bool hasp1;
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (Player.whoAmI != Main.myPlayer) return;
            hasp1 = false;
        }
    }
}
