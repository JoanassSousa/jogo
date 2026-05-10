using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace jogo
{
    public class Game1 : Game
    {

        Vector2 _playerWorldPosition = Vector2.Zero;
        //porta do mapa
        Rectangle _storeDoor = new Rectangle(350, 205, 100, 130);

        //png do supermarket do lado de fora e do lado de dentro
        Texture2D _outsideMap;
        Texture2D _insideMap;

        bool _insideStore = false;

        //posição do supermercado do lado de fora
        Vector2 _worldPosition = new Vector2(0, 250);

        // variaveis do mini menu
        Texture2D _menuButton;

        Rectangle _menuButtonRect;

        //classes dos playerzinhos
        Texture2D _playerUp;
        Texture2D _playerDown;
        Texture2D _playerLeft;
        Texture2D _playerRight;

        Texture2D _currentPlayerTexture;
        //player 
        Vector2 _playerScreenPosition;

        //inimigo
        Enemy _enemy;

        //ataque do player
        PlayerAttack _playerAttack;
        //direção para a qual o player está a olhar
        Vector2 _playerFaceDirection = new Vector2(0, 1);

        // vida do jogador
        int _playerHealth = 100;
        int _playerMaxHealth = 100;
        float _timeSinceLastDamage = 0f;
        float _regenTimer = 0f;

        // collision debounce
        float _timeSinceLastEnemyCollision = 0f;

        // debounce attack (para o inimigo não tomar vários danos por tick no mesmo ataque)
        bool _enemyHitByCurrentAttack = false;

        // XP e Level do Player
        int _playerLevel = 1;
        int _currentXp = 0;
        int _xpToNextLevel = 100; // Será modificado para requerer 50 x 2 = 100 de exp (sendo que cada gema de lvl 1 passará a dar apenas 2 XP de forma que 50 pedaços deem 100). Usaremos 100 de referencial

        // Lista de pedras de XP no cenário
        System.Collections.Generic.List<ExperienceGem> _experienceGems = new System.Collections.Generic.List<ExperienceGem>();

        //imagem do menu
        Texture2D _menuImage;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        enum GameState
        {
            Menu,
            Playing
        }

        GameState _currentState = GameState.Menu;

        string[] _menuOptions = { "Iniciar", "Continuar", "Salvar", "Sair" };
        int _selectedIndex = 0;

        //detetar teclado
        KeyboardState _keyboard;
        KeyboardState _prevKeyboard;

        MouseState _prevMouse;

        SpriteFont _font;


        void SalvarJogo()
        {
            File.WriteAllText("save.txt", "exemplo");
        }

        void CarregarJogo()
        {
            if (File.Exists("save.txt"))
            {
                // carregar dados
            }
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {

            //definir centro do ecrã
            _playerScreenPosition = new Vector2(
    GraphicsDevice.Viewport.Width / 2,
    GraphicsDevice.Viewport.Height / 2
);

            // inicializar o inimigo
            _enemy = new Enemy(new Vector2(200, 200));

            // inicializar ataque
            _playerAttack = new PlayerAttack();

            // posicao e tamanho do mini menu
            _menuButtonRect = new Rectangle(740, 10, 50, 50);

            // TODO: Add your initialization logic here

            base.Initialize();
        }

        // carregar coisas (fontes, imagens)
        protected override void LoadContent()
        {
            //supermarket do lado de fora e do lado de dentro
            _outsideMap = Content.Load<Texture2D>("supermercado_fora");
            _insideMap = Content.Load<Texture2D>("supermercado_dentro");

            // imagem do mini menu
            _menuButton = Content.Load<Texture2D>("menu_button");

            //imagens dos playerzinhos
            _playerUp = Content.Load<Texture2D>("player_up");
            _playerDown = Content.Load<Texture2D>("player_down");
            _playerLeft = Content.Load<Texture2D>("player_left");
            _playerRight = Content.Load<Texture2D>("player_right");

            // direção inicial
            _currentPlayerTexture = _playerDown;
            // imagem do menu
            _menuImage = Content.Load<Texture2D>("capablue");

            //fonte do menu
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("DefaultFont");
        }


        // lógica (teclas, movimento, menu)
        protected override void Update(GameTime gameTime)
        {

            // coisas do menu
            _keyboard = Keyboard.GetState();

            if (_currentState == GameState.Menu)
            {
                _prevMouse = Mouse.GetState();
                //detetar porta do supermercado
                if (!_insideStore && _storeDoor.Contains(_worldPosition))
                {
                    _insideStore = true;

                    _worldPosition = new Vector2(200, 200);
                }

                if (_keyboard.IsKeyDown(Keys.Down) && _prevKeyboard.IsKeyUp(Keys.Down))
                    _selectedIndex++;

                if (_keyboard.IsKeyDown(Keys.Up) && _prevKeyboard.IsKeyUp(Keys.Up))
                    _selectedIndex--;

                _selectedIndex = Math.Clamp(_selectedIndex, 0, _menuOptions.Length - 1);

                if (_keyboard.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
                    HandleMenuSelection();
            }

            //coisas dos playereszinhos e com o mundo a mover-se
            if (_currentState == GameState.Playing)
            {
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                if (keyboard.IsKeyDown(Keys.W))
                {
                    _playerWorldPosition.Y -= 3;
                    _currentPlayerTexture = _playerUp;
                    _playerFaceDirection = new Vector2(0, -1);
                }

                if (keyboard.IsKeyDown(Keys.S))
                {
                    _playerWorldPosition.Y += 3;
                    _currentPlayerTexture = _playerDown;
                    _playerFaceDirection = new Vector2(0, 1);
                }

                if (keyboard.IsKeyDown(Keys.A))
                {
                    _playerWorldPosition.X -= 3;
                    _currentPlayerTexture = _playerLeft;
                    _playerFaceDirection = new Vector2(-1, 0);
                }

                if (keyboard.IsKeyDown(Keys.D))
                {
                    _playerWorldPosition.X += 3;
                    _currentPlayerTexture = _playerRight;
                    _playerFaceDirection = new Vector2(1, 0);
                }

                // Disparar o ataque ao pressionar espaço ou clique esquerdo do mouse
                if ((keyboard.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space)) || 
                    (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released && !_menuButtonRect.Contains(mouse.Position)))
                {
                    Vector2 attackDirection = _playerFaceDirection;

                    if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
                    {
                        // Calcula direção em relação ao ponteiro do mouse
                        Vector2 mouseWorldPos = new Vector2(mouse.X, mouse.Y) + _worldPosition;
                        Vector2 direction = mouseWorldPos - _playerWorldPosition;
                        if (direction.LengthSquared() > 0)
                        {
                            direction.Normalize();
                            attackDirection = direction;
                        }
                    }

                    _playerAttack.StartAttack(_playerWorldPosition, attackDirection);
                    _enemyHitByCurrentAttack = false; // reseta controle de hit no ataque novo
                }

                _playerAttack.Update(gameTime, _playerWorldPosition);

                // lógica de regeneração de vida do jogador
                _timeSinceLastDamage += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_timeSinceLastDamage >= 5f && _playerHealth < _playerMaxHealth)
                {
                    _regenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_regenTimer >= 1f)
                    {
                        _playerHealth += 1;
                        if (_playerHealth > _playerMaxHealth) _playerHealth = _playerMaxHealth;
                        _regenTimer = 0f;
                    }
                }
                else
                {
                    _regenTimer = 0f;
                }

                //playerzinho acede ao mini menu
                if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released &&
                    _menuButtonRect.Contains(mouse.Position))
                {
                    _currentState = GameState.Menu;
                }

                // criar lista de obstáculos (neste caso, a porta)
                var obstacles = new System.Collections.Generic.List<Rectangle>();
                if (!_insideStore)
                {
                    obstacles.Add(_storeDoor); // o inimigo contornará a porta colidível por fora
                }

                _enemy.Update(_playerWorldPosition, obstacles);

                // Colisão do inimigo com o jogador
                _timeSinceLastEnemyCollision += (float)gameTime.ElapsedGameTime.TotalSeconds;

                Rectangle playerBounds = new Rectangle(
                    (int)_playerWorldPosition.X, 
                    (int)_playerWorldPosition.Y, 
                    32, 32 // assumindo tamanho 32x32 para o player
                );

                if (_enemy.Health > 0 && _enemy.Bounds.Intersects(playerBounds))
                {
                    if (_timeSinceLastEnemyCollision >= 1f) // 1 seg de invulnerabilidade após bater
                    {
                        _playerHealth -= 5;
                        _timeSinceLastDamage = 0f;
                        _timeSinceLastEnemyCollision = 0f;

                        if (_playerHealth <= 0)
                        {
                            // Ação ao morrer (ex: voltar ao menu)
                            _playerHealth = _playerMaxHealth;
                            _currentState = GameState.Menu;
                        }
                    }
                }

                // Lógica de dano do ataque no inimigo 
                if (_enemy.Health > 0 && _playerAttack.IsAttacking && !_enemyHitByCurrentAttack && _playerAttack.CheckCollision(_enemy.Bounds))
                {
                    _enemy.TakeDamage(_playerAttack.Damage); // nivel 1 já está configurado na classe p/ 10 de dano por padrão ou vc pode alterar lá. Atualmente é 10, vou colocar pra forçar 5 lá.
                    _enemyHitByCurrentAttack = true;

                    if (_enemy.Health <= 0)
                    {
                        // Inimigo morreu, dropa a gema de XP (Nível 1 de visual)
                        // Para que sejam necessários 50 pedaços até o nível 2 (quando se precisa de 100XP), dada uma gema, ela dará 2 de XP.
                        _experienceGems.Add(new ExperienceGem(
                            new Vector2(_enemy.WorldPosition.X + 5, _enemy.WorldPosition.Y + 5), 1, 2));
                    }
                }

                // Coletar pedras de XP
                for (int i = _experienceGems.Count - 1; i >= 0; i--)
                {
                    if (playerBounds.Intersects(_experienceGems[i].Bounds))
                    {
                        // Coleciona a pedra
                        _currentXp += _experienceGems[i].XpAmount;
                        _experienceGems.RemoveAt(i);

                        // Checar level up
                        if (_currentXp >= _xpToNextLevel)
                        {
                            _currentXp -= _xpToNextLevel;
                            _playerLevel++;
                            _xpToNextLevel = (int)(_xpToNextLevel * 1.5f); // Aumenta a exp necessária para o próximo

                            // Aumentar nível do ataque e vida caso passe do level 2 ou 3 
                            if (_playerLevel >= 2) _playerAttack.AttackLevel = 2;
                            if (_playerLevel >= 3) _playerAttack.AttackLevel = 3;

                            _playerMaxHealth += 10;
                            _playerHealth = _playerMaxHealth;
                        }
                    }
                }

            }

            _prevKeyboard = _keyboard;


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }


        void HandleMenuSelection()
        {
            switch (_selectedIndex)
            {
                case 0:
                    _currentState = GameState.Playing;
                    break;

                case 1:
                    CarregarJogo();
                    _currentState = GameState.Playing;
                    break;

                case 2:
                    SalvarJogo();
                    break;

                case 3:
                    Exit();
                    break;
            }
        }


        //desenhar no ecrã
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);

            _spriteBatch.Begin();

            if (_currentState == GameState.Menu)
            {
                // MENU (texto)
                for (int i = 0; i < _menuOptions.Length; i++)
                {
                    Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;

                    _spriteBatch.DrawString(_font, _menuOptions[i], new Vector2(100, 100 + i * 40), color);
                }

                // x e y de onde começa a imagem
                _spriteBatch.Draw(_menuImage, new Vector2(400, 0), Color.White);
            }
                else if (_currentState == GameState.Playing)
                {
                    // mapa
                    Texture2D currentMap = _insideStore ? _insideMap : _outsideMap;
                    _spriteBatch.Draw(currentMap, -_worldPosition, Color.White);

                //porta do supermercado
                Texture2D debugTexture = new Texture2D(GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Black });

                Rectangle movedDoor = new Rectangle(
                    (int)(_storeDoor.X - _worldPosition.X),
                    (int)(_storeDoor.Y - _worldPosition.Y),
                    _storeDoor.Width,
                    _storeDoor.Height
                );

                _spriteBatch.Draw(debugTexture, movedDoor, Color.Red * 0.5f);


                // mini menu
                _spriteBatch.Draw(_menuButton, _menuButtonRect, Color.White);

                    // playerzinho no mundo
                    _spriteBatch.Draw(_currentPlayerTexture, _playerScreenPosition, Color.White);

                    // desenhar inimigo apenas se estiver vivo
                    if (_enemy.Health > 0)
                    {
                        _enemy.Draw(_spriteBatch, _playerScreenPosition, _playerWorldPosition);
                    }

                    // desenhar gemas de XP
                    foreach (var gem in _experienceGems)
                    {
                        gem.Draw(_spriteBatch, _playerScreenPosition, _playerWorldPosition);
                    }

                    // desenhar o ataque visual (hitbox do ataque)
                    _playerAttack.Draw(_spriteBatch, _playerScreenPosition, _playerWorldPosition);

                    // Desenhar a vida do jogador por cima
                    _spriteBatch.DrawString(_font, $"Vida: {_playerHealth}/{_playerMaxHealth}", new Vector2(10, 10), Color.Red);

                    // Desenhar nível e XP
                    string lvlText = $"Nvl: {_playerLevel}";
                    Vector2 lvlTextSize = _font.MeasureString(lvlText);
                    _spriteBatch.DrawString(_font, lvlText, new Vector2(10, 35), Color.Gold);

                    // Barra de XP
                    Texture2D barTex = new Texture2D(GraphicsDevice, 1, 1);
                    barTex.SetData(new[] { Color.White });

                    int xpBarWidth = 100;
                    int xpBarHeight = 10;
                    Vector2 xpBarPos = new Vector2(15 + lvlTextSize.X, 40);

                    // Fundo da barra
                    _spriteBatch.Draw(barTex, new Rectangle((int)xpBarPos.X, (int)xpBarPos.Y, xpBarWidth, xpBarHeight), Color.Gray * 0.5f);

                    // Atual barra de XP
                    float xpPercent = (float)_currentXp / _xpToNextLevel;
                    _spriteBatch.Draw(barTex, new Rectangle((int)xpBarPos.X, (int)xpBarPos.Y, (int)(xpBarWidth * xpPercent), xpBarHeight), new Color(35, 79, 215)); // Cor pedida
                }



            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
