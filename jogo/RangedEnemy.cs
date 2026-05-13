using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace jogo
{
    public class RangedEnemy : Enemy
    {
        private float _attackTimer = 0.0f;
        private float _attackInterval = 2.5f;

        public RangedEnemy(Vector2 initialPosition)
          : base(initialPosition)
        {
            this.Speed = 1f;
            this.Health = 15;
        }

        public RangedEnemyBullet UpdateRanged(
          GameTime gameTime,
          Vector2 playerWorldPosition,
          List<Rectangle> obstacles)
        {
            this.Update(playerWorldPosition, obstacles);
            this._attackTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (this._attackTimer < this._attackInterval)
                return null;
            this._attackTimer = 0.0f;
            return new RangedEnemyBullet(this.WorldPosition + new Vector2(10f, 10f), playerWorldPosition - this.WorldPosition);
        }
    }
}
