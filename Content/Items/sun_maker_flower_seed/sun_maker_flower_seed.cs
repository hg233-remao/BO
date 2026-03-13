using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BO.Content.Items.sun_maker_flower_seed
{
    public class sun_maker_flower_seed : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.createTile = ModContent.TileType<Tiles.sun_maker_flower_seedling.sun_maker_flower_seedling>();
        }
    }
}