using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using BO;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using BO.Content.Items.Magic.Magic_System;
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
    public class Book_Of_Leaves_Crystal : ModProjectile
    {
        int projectile_slot_index;
        //void 
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 18;
            Projectile.timeLeft = 60;
        }
        public override void AI()
        {
        }
    }
}