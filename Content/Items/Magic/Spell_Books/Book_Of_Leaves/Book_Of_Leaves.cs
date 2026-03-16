using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using BO;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using BO.Content.Items.Magic.Magic_System;
using Microsoft.CodeAnalysis;
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
            Item.damage = 3;
            Item.DamageType = DamageClass.Magic;
            //Item.tooltipContext.
        }
        public override string Texture => "BO/Content/Items/Magic/Spell_Books/Book_Of_Leaves/Book_Of_Leaves_Crystal_d";
    }
    public class Book_Of_Leaves_Crystal : Crystal_Projectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 18;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
        }
        public override void AI()
        {
            Projectile.timeLeft = 60;
            Projectile.position.X = Main.player[Projectile.owner].Center.X + (float)Math.Cos(Magic_Slot_Sets.Crystal_Angle * Math.PI / 180) * 40f;
            Projectile.position.Y = Main.player[Projectile.owner].Center.Y + (float)Math.Sin(Magic_Slot_Sets.Crystal_Angle * Math.PI / 180) * 40f;
        }
        public override void OnKill(int timeLeft)
        {
        }
    }
}