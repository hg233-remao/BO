using System;
using BO;
using BO.Content.another.Magic.Magic_System;
using BO.Content.another.Projectile_Function;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace BO.Content.Items.Magic.Spell_Books.Book_Of_Leaves
{
    public class Book_Of_Leaves : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 180;
            Item.useAnimation = 100;
            Item.UseSound = new SoundStyle("BO/Content/Sounds/Book");
            //Item.shoot = ModContent.GetInstance<Book_Of_Leaves_Crystal>().Type;
        }
        public override bool? UseItem(Player player)
        {
            player.GetModPlayer<Magic_Slot_Sets>().Has_Learned_Magic[1] = true;
            return true;
        }
    }
    public class Book_Of_Leaves_Crystal_d : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 5;
            Item.DamageType = DamageClass.Magic;
            //Item.tooltipContext.
        }
        public override string Texture => "BO/Content/Items/Magic/Spell_Books/Book_Of_Leaves/Book_Of_Leaves_Crystal_d";
    }
    public class Book_Of_Leaves_Crystal : Crystal_Projectile
    {
        int Leaves_Cool_Down = 0;
        public int? Son_Index = null;
        Vector2 Create_position;
        public override void SetDefaults()
        {
            //必须狠狠尽孝绘制偏移爹
            Projectile.width = 10;
            Projectile.height = 18;
            DrawOffsetX = -Projectile.width / 2;
            DrawOriginOffsetY = -Projectile.height / 2;
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Main.projFrames[Projectile.type] = 3;
        }
        public override void AI()
        {
            Projectile.timeLeft = 60;
            if (Active_Power == 0)
                Projectile.frame = 0;
            if (Active_Power == 1)
                Projectile.frame = 1;
            if (Active_Power == 2)
                Projectile.frame = 2;
            Projectile.position.X = Main.player[Projectile.owner].Center.X + (float)Math.Cos(Magic_Slot_Sets.Crystal_Angle * Math.PI / 180) * 40f - 1;
            //完美解决了单格障碍物离散绘制的问题，我太聪明了和额呵呵呵呵呵呵呵呵呵
            Projectile.position.Y = Main.player[Projectile.owner].Center.Y + Main.player[Projectile.owner].gfxOffY + (float)Math.Sin(Magic_Slot_Sets.Crystal_Angle * Math.PI / 180) * 40f;
            if (Son_Index == null && Active_Power != 0) 
                if (Leaves_Cool_Down == 0)
                {
                    Son_Index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y - Projectile.height / 2), Vector2.Zero, ModContent.ProjectileType<Book_Of_Leaves_Crystal_Leaves>(), 5, 1f, Projectile.owner, Projectile.whoAmI);
                    if (Active_Power == 2)
                        Leaves_Cool_Down = 70;
                    if (Active_Power == 1)
                        Leaves_Cool_Down = 150;
                    if (Active_Power == 0)
                        Leaves_Cool_Down = 150;
                }
                else
                {
                    Leaves_Cool_Down--;
                }
            else
            {
                if (Active_Power == 2)
                    Leaves_Cool_Down = 70;
                if (Active_Power == 1)
                    Leaves_Cool_Down = 150;
                if (Active_Power == 0)
                    Leaves_Cool_Down = 150;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Son_Index != null)
            {
                Main.projectile[(int)Son_Index].Kill();
            }
        }

    }
    public class Book_Of_Leaves_Crystal_Leaves : BO_Projectile
    {
        Vector2[] a = new Vector2[5];
        Vector3 b = new Vector3(34, 139, 34);
        bool Chasing = false;
        int Parent_Index;
        Book_Of_Leaves_Crystal Parent_Projectile;
        NPC Enemy = null;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 80;
            Projectile.frameCounter = 80;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.scale = 1.2f;
            Projectile.DamageType = DamageClass.Magic;
            Main.projFrames[Projectile.type] = 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Parent_Index = (int)Projectile.ai[0];
            Parent_Projectile = Main.projectile[Parent_Index].ModProjectile as Book_Of_Leaves_Crystal;
        }
        public override void AI()
        {
            Projectile.frame = Projectile.frameCounter % 24 / 6 - 1;
            if (Projectile.frameCounter == 0)
                Projectile.frameCounter = 80;
            else
                Projectile.frameCounter--;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.ToRadians(90f);
            for (int i = 3; i >= 0; i--)
                a[i + 1] = a[i];
            a[0] = Projectile.Center;
            Lighting.AddLight(Projectile.Center, b * 0.002f);
            if (Chasing)
            {
                Projectile.velocity *= 0.98f;
            }
            else
            {
                if (Parent_Projectile == null)
                    Projectile.Kill();
                Projectile.timeLeft = 80;
                Projectile.position.X = Parent_Projectile.Projectile.position.X - Projectile.width / 2 + (float)Math.Cos(Projectile.frameCounter * 9 / 2 * Math.PI / 180) * 20f;
                Projectile.position.Y = Parent_Projectile.Projectile.position.Y - Projectile.height / 2 + (float)Math.Sin(Projectile.frameCounter * 9 / 2 * Math.PI / 180) * 20f;
                Projectile.velocity.X = -(float)Math.Sin(Projectile.frameCounter * 9 / 2 * Math.PI / 180) * 0.01f;
                Projectile.velocity.Y = (float)Math.Cos(Projectile.frameCounter * 9 / 2 * Math.PI / 180) * 0.01f;
                Enemy = Find_Closest_Enemy_In_Distance(230);
                if (Enemy != null)
                {
                    Projectile.velocity = Vector2.Normalize(Enemy.Center - Projectile.Center) * 7f;
                    Chasing = true;
                    Parent_Projectile.Son_Index = null;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < 5 && Projectile.timeLeft > 30; i++)
                Main.spriteBatch.Draw(ModContent.Request<Texture2D>("BO/Content/Items/Bow/Vine_Bow/trail").Value, a[i] - Main.screenPosition, null, Color.White * 0.1f * (5 - i), (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.ToRadians(90f), ModContent.Request<Texture2D>("BO/Content/Items/Bow/Vine_Bow/trail").Value.Size() * 0.5f, 1.2f, SpriteEffects.None, 0);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
            for (int i = 0; i < 3; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades);
            if (Parent_Projectile.Son_Index == Projectile.whoAmI)
                Parent_Projectile.Son_Index = null;
        }
    }
}