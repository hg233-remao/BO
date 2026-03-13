using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BO.Content.Items.test
{
    public class test : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DirtBlock);
            //Item.createTile = ModContent.TileType<Tiles.sun_maker_flower.sun_maker_flower>();
        }
    }
}