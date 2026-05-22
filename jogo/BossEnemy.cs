using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
using System.Collections.Generic;
using System;

namespace jogo
{
    public class BossEnemy : Enemy
    {
        private Texture2D[] bossFrames;
        private int currentBossFrame = 0;

        private float animationTimer = 0f;
        private float animationSpeed = 0.2f;

        // Bullet Hell Attack Variables
        public List<RangedEnemyBullet> BossBullets { get; private set; } = new List<RangedEnemyBullet>();
        private float attackTimer = 0f;
        private float timeBetweenAttacks = 3f;
        private int currentAttackType = 0; // 0: Circle, 1: Spiral, 2: Sweeping Stream

        // Pattern variables
        private float spiralAngle = 0f;
        private int streamBulletsFired = 0;
        private float streamAngle = -MathHelper.PiOver4;
        private bool streamDirectionRight = true;

        public void LoadBossFrames(Texture2D[] frames)
        {
            bossFrames = frames;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer >= animationSpeed)
            {
                currentBossFrame++;

                if (currentBossFrame >= bossFrames.Length)
                    currentBossFrame = 0;

                animationTimer = 0f;
            }
        }

        public BossEnemy(Vector2 initialPosition)
          : base(initialPosition)
        {
            this.Speed = 0.0f;
            this.Health = 750;
        }

        public void UpdateBossAttacks(GameTime gameTime, Vector2 playerWorldPosition)
        {
            attackTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (attackTimer >= timeBetweenAttacks)
            {
                PerformAttack(playerWorldPosition);
            }

            // Update all bullets
            for (int i = BossBullets.Count - 1; i >= 0; i--)
            {
                BossBullets[i].Update();
                // Optional: remove bullets that go too far off-screen to save memory
                if (Vector2.Distance(this.WorldPosition, BossBullets[i].Position) > 1000f)
                {
                    BossBullets.RemoveAt(i);
                }
            }
        }

        private void PerformAttack(Vector2 playerPosition)
        {
            if (currentAttackType == 0)
            {
                // Circle Attack (Nova)
                int numberOfBullets = 12; // Menos balas (mais fácil desviar)
                float angleStep = MathHelper.TwoPi / numberOfBullets;
                for (int i = 0; i < numberOfBullets; i++)
                {
                    float angle = i * angleStep;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    var bullet = new RangedEnemyBullet(new Vector2(Bounds.Center.X - 5, Bounds.Center.Y - 5), direction);
                    bullet.Damage = 15; // Mais dano
                    bullet.Velocity = direction * 3f; // Um pouco mais lentas
                    BossBullets.Add(bullet);
                }
                attackTimer = 0f;
                currentAttackType = 1; // Switch over to Spiral next
                timeBetweenAttacks = 0.15f; // Fast shooting for spiral mas mais espacado
                spiralAngle = 0f;
            }
            else if (currentAttackType == 1)
            {
                // Spiral Attack 
                Vector2 direction = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle));
                var bullet = new RangedEnemyBullet(new Vector2(Bounds.Center.X - 5, Bounds.Center.Y - 5), direction);
                bullet.Damage = 15; // Mais dano
                bullet.Velocity = direction * 3f; // Um pouco mais lentas
                BossBullets.Add(bullet);

                spiralAngle += 0.45f; // Maior folga no espiral (mais fácil desviar)
                attackTimer = 0f;

                if (spiralAngle > MathHelper.TwoPi * 3) // Do 3 full rotations
                {
                    currentAttackType = 2; // Switch to stream
                    timeBetweenAttacks = 0.2f; // Stream mais fluído e devagar de desviar
                    streamBulletsFired = 0;
                }
            }
            else if (currentAttackType == 2)
            {
                // Sweeping Stream Attack (aimed roughly at player)
                Vector2 baseDirection = playerPosition - this.WorldPosition;
                float baseAngle = (float)Math.Atan2(baseDirection.Y, baseDirection.X);

                float angleOffset = (float)Math.Sin(streamBulletsFired * 0.4f) * 1.5f; // Oscillate com freqs mais abertas

                float finalAngle = baseAngle + angleOffset;
                Vector2 finalDirection = new Vector2((float)Math.Cos(finalAngle), (float)Math.Sin(finalAngle));

                var bullet = new RangedEnemyBullet(new Vector2(Bounds.Center.X - 5, Bounds.Center.Y - 5), finalDirection);
                bullet.Damage = 15; // Mais dano
                bullet.Velocity = finalDirection * 3.5f; // Velocidade perigosa mas viavel
                BossBullets.Add(bullet);

                streamBulletsFired++;
                attackTimer = 0f;

                if (streamBulletsFired > 20) // Stop after 20 bullets in the stream em vez de 30 (menos tempo neste estado para janela de ataque)
                {
                    currentAttackType = 0; // Go back to Circle
                    timeBetweenAttacks = 2.5f; // Cooldown before next sequence ligeiramente maior
                }
            }
        }

        public new Rectangle Bounds
        {
            get => new Rectangle((int)this.WorldPosition.X, (int)this.WorldPosition.Y, 60, 60);
        }

        public new void Draw(
            SpriteBatch spriteBatch,
            Vector2 playerScreenPosition,
            Vector2 playerWorldPosition)
        {
            Vector2 position = this.WorldPosition - playerWorldPosition + playerScreenPosition;

            // Draw Bullets first so they are behind/infront as needed (simplistic here, just passing null to use the fallback square or logic)
            foreach (var bullet in BossBullets)
            {
                // Bullet draws using absolute world positions in RangedEnemyBullet by default
                // RangedEnemyBullet Draw function is using "Position" directly for spriteBatch
                // Let's modify its drawn position to be relative to screen
                Vector2 bulletScreenPos = bullet.Position - playerWorldPosition + playerScreenPosition;

                // Re-implementing a simple draw logic for the bullet within Boss scope to account for camera
                Texture2D debug = new Texture2D(spriteBatch.GraphicsDevice, 10, 10);
                Color[] data = new Color[10 * 10];
                for (int i = 0; i < data.Length; ++i) data[i] = Color.Orange; // Make boss bullets orange
                debug.SetData(data);
                spriteBatch.Draw(debug, bulletScreenPos, Color.White);
            }

            spriteBatch.Draw(
                bossFrames[currentBossFrame],
                new Rectangle((int)position.X, (int)position.Y, 60, 60),
                Color.White
            );

            if (this.Health <= 0)
                return;

            Texture2D texture1 = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            texture1.SetData<Color>(new Color[1] { Color.White });

            int width1 = 60;
            int height = 6;
            int num1 = -12;

            Rectangle destinationRectangle1 =
                new Rectangle((int)position.X, (int)position.Y + num1, width1, height);

            spriteBatch.Draw(texture1, destinationRectangle1, Color.Red);

            float num2 = (float)this.Health / 750f;

            int width2 = (int)(width1 * num2);

            Rectangle destinationRectangle2 =
                new Rectangle((int)position.X, (int)position.Y + num1, width2, height);

            spriteBatch.Draw(texture1, destinationRectangle2, Color.DarkGreen);
        }
    }
}