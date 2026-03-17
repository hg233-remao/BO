using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BO.Content.another.Projectile_Function
{
    public abstract class BO_Projectile : ModProjectile
    {
        //通用函数，用来定位radius范围内距离最近的npc
        public NPC Find_Closest_Enemy_In_Distance(float radius)
        {
            NPC Closest_Enemy = null;
            for (int i = 0; i < Main.maxNPCs-1; i++)
            {
                if (Vector2.DistanceSquared(Main.npc[i].Center, Projectile.Center) > radius * radius) continue;
                if ((Main.npc[i]).CanBeChasedBy() && Closest_Enemy == null)
                {
                    Closest_Enemy = Main.npc[i];
                    continue;
                }
                if (Closest_Enemy != null && Vector2.DistanceSquared(Closest_Enemy.Center, Projectile.Center) > Vector2.DistanceSquared(Main.npc[i].Center, Projectile.Center) && (Main.npc[i]).CanBeChasedBy())
                    Closest_Enemy = Main.npc[i];
            }
            return Closest_Enemy;
        }
    }
}