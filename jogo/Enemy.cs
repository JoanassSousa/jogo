using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace jogo
{
    public class Enemy
    {
        public Vector2 WorldPosition { get; set; }
        public float Speed { get; set; } = 2f;
        public Vector2 direction;
        public Texture2D[] Texture { get; set; }
        private int currentFrame = 0;

        private float animationTimer = 0f;
        private float animationSpeed = 0.2f;

        public int Health { get; set; } = 20;
        public int MaxHealth { get; private set; } = 20;

        public Rectangle Bounds => new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, 20, 20);

        public void TakeDamage(int delayOrDamageAmmount)
        {
            Health -= delayOrDamageAmmount;
        }

        public Enemy(Vector2 initialPosition)
        {
            WorldPosition = initialPosition;
        }

        // Propriedades do Knockback passadas
        public bool IsBeingKnockedBack { get; private set; }
        private Vector2 _knockbackDirection;
        private float _knockbackSpeed;
        private float _knockbackFriction = 0.90f;

        public void ApplyKnockback(Vector2 direction, float force)
        {
            IsBeingKnockedBack = true;
            _knockbackDirection = direction;
            if (_knockbackDirection != Vector2.Zero)
                _knockbackDirection.Normalize();

            _knockbackSpeed = force;
        }

        public void Update(Vector2 playerWorldPosition, System.Collections.Generic.List<Rectangle> obstacles)
        {
            if (IsBeingKnockedBack)
            {
                WorldPosition += _knockbackDirection * _knockbackSpeed;
                _knockbackSpeed *= _knockbackFriction;

                if (_knockbackSpeed < 0.5f)
                {
                    IsBeingKnockedBack = false;
                }

                return;
            }

            direction = playerWorldPosition - WorldPosition;
            if (direction != Vector2.Zero)
            {
                // Animação de movimento
                _animationCounter++;
                if (_animationCounter >= _animationDelay)
                {
                    _animationCounter = 0;
                    _currentFrame++;
                    if (_currentFrame >= _totalFrames)
                        _currentFrame = 0;
                }

                // Força o movimento apenas para o eixo de maior distância (Caminho mais Linear - Grade)
                if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
                {
                    direction = new Vector2(System.Math.Sign(direction.X), 0);
                }
                else
                {
                    direction = new Vector2(0, System.Math.Sign(direction.Y));
                }

                direction.Normalize();

                // Tentativa de movimento na direção linear forçada
                Vector2 newPosition = WorldPosition + direction * Speed;
                Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y, 20, 20);

                bool collision = false;
                foreach (var obstacle in obstacles)
                {
                    if (newBounds.Intersects(obstacle))
                    {
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    WorldPosition = newPosition;
                }
                else
                {
                    // Deslizamento e Caminho alternativo caso obstáculo
                    if (direction.X != 0) // Se bateu indo em X, tenta ir em Y
                    {
                        Vector2 backupY = new Vector2(0, System.Math.Sign(playerWorldPosition.Y - WorldPosition.Y));
                        if (backupY != Vector2.Zero)
                        {
                            Vector2 newPositionY = WorldPosition + backupY * Speed;
                            Rectangle newBoundsY = new Rectangle((int)newPositionY.X, (int)newPositionY.Y, 20, 20);

                            bool collisionY = false;
                            foreach (var obs in obstacles) if (newBoundsY.Intersects(obs)) collisionY = true;

                            if (!collisionY) WorldPosition = newPositionY;
                        }
                    }
                    else if (direction.Y != 0) // Se bateu indo em Y, tenta em X
                    {
                        Vector2 backupX = new Vector2(System.Math.Sign(playerWorldPosition.X - WorldPosition.X), 0);
                        if (backupX != Vector2.Zero)
                        {
                            Vector2 newPositionX = WorldPosition + backupX * Speed;
                            Rectangle newBoundsX = new Rectangle((int)newPositionX.X, (int)newPositionX.Y, 20, 20);

                            bool collisionX = false;
                            foreach (var obs in obstacles) if (newBoundsX.Intersects(obs)) collisionX = true;

                            if (!collisionX) WorldPosition = newPositionX;
                        }
                    }
                }
            }
        }

        public void LoadFrames(Texture2D[] frames)
        {
            Texture = frames;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer >= animationSpeed)
            {
                currentFrame++;

                if (currentFrame >= Texture.Length)
                    currentFrame = 0;

                animationTimer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerScreenPosition, Vector2 playerWorldPosition)
        {
            // Calculate screen position relative to player
            Vector2 screenPosition = WorldPosition - playerWorldPosition + playerScreenPosition;

            if (Texture != null)
            {
                int frameWidth = Texture.Width;
                int frameHeight = Texture.Height / _totalFrames;

                Rectangle sourceRectangle = new Rectangle(0, _currentFrame * frameHeight, frameWidth, frameHeight);

                SpriteEffects spriteEffect = SpriteEffects.None;

                // Inverte horizontalmente se o inimigo estiver posicionado à esquerda do jogador 
                // e precisar virar na direcão deste
                if (WorldPosition.X < playerWorldPosition.X)
                {
                    spriteEffect = SpriteEffects.FlipHorizontally;
                }

                spriteBatch.Draw(
                    Texture,
                    screenPosition,
                    sourceRectangle,
                    Color.White,
                    0f,
                    Vector2.Zero, // Ponto de origem
                    1f,           // Escala
                    spriteEffect,
                    0f            // Profundidade (Layer)
                );
            }
            else
            {
                // Draw a simple shape if texture is missing (for debugging/placeholder)
                Texture2D debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 20, 20);
                Color[] data = new Color[20 * 20];
                for (int i = 0; i < data.Length; ++i) data[i] = Color.Red;
                debugTexture.SetData(data);
                spriteBatch.Draw(debugTexture, screenPosition, Color.White);
            }

            // Draw Health Bar se estiver vivo
            if (Health > 0)
            {
                Texture2D healthBarTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                healthBarTexture.SetData(new[] { Color.White });

                int barWidth = Texture != null ? Texture.Width : 20; // Largura equivalente
                int barHeight = 4;
                int yOffset = -8; // Distância acima do inimigo

                // Barra de fundo (vermelha)
                Rectangle backgroundBar = new Rectangle((int)screenPosition.X, (int)screenPosition.Y + yOffset, barWidth, barHeight);
                spriteBatch.Draw(healthBarTexture, backgroundBar, Color.Red);

                // Barra de frente (verde)
                float healthPercent = (float)Health / MaxHealth;
                int currentBarWidth = (int)(barWidth * healthPercent);
                Rectangle currentHealthBar = new Rectangle((int)screenPosition.X, (int)screenPosition.Y + yOffset, currentBarWidth, barHeight);
                spriteBatch.Draw(healthBarTexture, currentHealthBar, Color.Green);
            }
        }
    }
}