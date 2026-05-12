using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace jogo
{
    public class PlayerAttack
    {
        public bool IsAttacking { get; private set; }

        // Removido AttackArea estático, nós checaremos colisão com um método agora

        public int Damage { get; private set; }

        private float _attackDuration = 0.2f; // dura x segundos
        private float _attackTimer = 0f;
        private Vector2 _currentDirection;

        // Propriedade construtora de arco
        private float _arcAngle; // Angulo da abertura do cone
        private float _arcRadius; // Raio máximo

        private Vector2 _playerCenterAtAttack;

        // Para desenhar precisaremos de uma mini textura
        private Texture2D _pixelTexture;

        public int AttackLevel { get; set; } = 1;

        public void StartAttack(Vector2 playerPos, Vector2 faceDirection)
        {
            if (IsAttacking) return;

            IsAttacking = true;
            _attackTimer = _attackDuration;
            _currentDirection = faceDirection;

            UpdateAttackArea(playerPos);
        }

        public void Update(GameTime gameTime, Vector2 playerPos)
        {
            if (IsAttacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_attackTimer <= 0)
                {
                    IsAttacking = false;
                }
                else
                {
                    UpdateAttackArea(playerPos);
                }
            }
        }

        private void UpdateAttackArea(Vector2 playerPos)
        {
            // O centro consideraremos +16 (pois o player tem ~32x32)
            _playerCenterAtAttack = playerPos + new Vector2(16, 16);

            if (AttackLevel == 2)
            {
                _arcRadius = 70f;
                _arcAngle = MathHelper.PiOver2; // 90 Graus
                Damage = 20;
            }
            else if (AttackLevel >= 3)
            {
                _arcRadius = 90f;
                _arcAngle = MathHelper.Pi * 0.75f; // 135 graus
                Damage = 30;
            }
            else // Nível 1
            {
                _arcRadius = 50f;
                _arcAngle = MathHelper.PiOver4; // 45 Graus
                Damage = 5;
            }
        }

        // Novo método para checar se algo colide com o Arco
        public bool CheckCollision(Rectangle target)
        {
            if (!IsAttacking) return false;

            // Retângulo da bounding box do target
            Vector2 targetCenter = new Vector2(target.X + target.Width / 2f, target.Y + target.Height / 2f);

            // 1. O alvo está dentro do raio?
            float distanceToTarget = Vector2.Distance(_playerCenterAtAttack, targetCenter);

            // Adicional raio usando o tamanho do target (aproximado)
            if (distanceToTarget > _arcRadius + (target.Width / 2f))
                return false;

            // 2. O alvo está dentro do ângulo do cone?
            // Angulo da nossa direção
            float dirAngle = (float)Math.Atan2(_currentDirection.Y, _currentDirection.X);

            // Angulo para o alvo
            // Diferença de posição
            Vector2 diff = targetCenter - _playerCenterAtAttack;
            float targetAngle = (float)Math.Atan2(diff.Y, diff.X);

            // Diferença angular
            float angleDiff = targetAngle - dirAngle;
            // Normalizar entre -Pi e Pi
            while (angleDiff <= -MathHelper.Pi) angleDiff += MathHelper.TwoPi;
            while (angleDiff > MathHelper.Pi) angleDiff -= MathHelper.TwoPi;

            // Checar se a diferença pro centro do arco tá dentro de metade da abertura do arco
            if (Math.Abs(angleDiff) <= (_arcAngle / 2f))
            {
                return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerScreenPosition, Vector2 playerWorldPosition)
        {
            if (IsAttacking)
            {
                if (_pixelTexture == null)
                {
                    _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                    _pixelTexture.SetData(new[] { Color.White });
                }

                Color attackColor = Color.White * 0.5f;
                if (AttackLevel == 1) attackColor = Color.Yellow * 0.5f;
                if (AttackLevel == 2) attackColor = Color.Orange * 0.5f;
                if (AttackLevel >= 3) attackColor = Color.Red * 0.5f;

                // Centro na Tela
                Vector2 centerScreenPos = _playerCenterAtAttack - playerWorldPosition + playerScreenPosition;

                float dirAngle = (float)Math.Atan2(_currentDirection.Y, _currentDirection.X);

                // Desenhar vários pontinhos pra simular o arco/cone
                int segments = 20; // Qtde de fatias que serão renderizadas para compor o visual do Cone
                float startAngle = dirAngle - (_arcAngle / 2f);
                float angleStep = _arcAngle / segments;

                for (int i = 0; i <= segments; i++)
                {
                    float currentAngle = startAngle + (i * angleStep);

                    // Vai desenhar desde o centro até o raio preenchendo as arestas da pizza num triângulo
                    // Uma maneira simples de desenhar linhas em monogame sem bibliotecas extras:

                    // Desenhemos a borda do arco
                    Vector2 edgePoint = centerScreenPos + new Vector2(
                        (float)Math.Cos(currentAngle) * _arcRadius,
                        (float)Math.Sin(currentAngle) * _arcRadius
                    );

                    DrawLine(spriteBatch, _pixelTexture, centerScreenPos, edgePoint, attackColor);
                }
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float distance = edge.Length();

            spriteBatch.Draw(texture,
                new Rectangle((int)start.X, (int)start.Y, (int)distance, 2), // 2 pixels thick
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }
    }
}