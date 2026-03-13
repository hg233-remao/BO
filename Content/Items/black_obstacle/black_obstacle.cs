using BO.Content.Tiles.black_obstacle;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BO.Content.Items.black_obstacle
{
    public class black_obstacle : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DirtBlock);
            Item.createTile=ModContent.TileType< Tiles.black_obstacle.black_obstacle> ();
        }
    }
}