using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace jogo
{
    public class Game1 : Game 
    {
        // boss com animação de 4 frames
        private Texture2D[] bossFrames;
        private Texture2D[] sheepFrames;
        private Texture2D[] rangedFramesUp;
        private Texture2D[] rangedFramesDown;
        private Texture2D[] rangedFramesLeft;
        private Texture2D[] rangedFramesRight;

        // sound effects
        private SoundEffect attackSound;
        private SoundEffect damagedSound;

        Vector2 _playerWorldPosition = Vector2.Zero;

        // mapas por area
        Texture2D[] _areaMaps = new Texture2D[4]; // indices 1..3 usados para niveis

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

        // O mapa ficará agora estático no fundo, tamanho do mapa será os limites:
        int _mapWidth = 800; // Será atualizado para o tamanho da textura ou viewport
        int _mapHeight = 600;


        //inimigo
        Enemy _enemy;
        Texture2D _enemyTexture;
        System.Collections.Generic.List<Enemy> _enemies = new System.Collections.Generic.List<Enemy>();
        System.Collections.Generic.List<RangedEnemyBullet> _enemyBullets = new System.Collections.Generic.List<RangedEnemyBullet>();
        Texture2D _bulletTexture; // Textura estática da bala

        // Portal e Zonas
        int _currentArea = 1;
        bool _portalActive = false;
        Rectangle _portalBounds;


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
        float _currentXp = 0;
        int _xpToNextLevel = 2; // Será modificado para requerer 50 x 2 = 100 de exp (sendo que cada gema de lvl 1 passará a dar apenas 2 XP de forma que 50 pedaços deem 100). Usaremos 100 de referencial

        // Lista de pedras de XP no cenário
        System.Collections.Generic.List<ExperienceGem> _experienceGems = new System.Collections.Generic.List<ExperienceGem>();

        // obstacles / walls
        Wall _wall;
        BrownWall _brownWall;

        //imagem do menu
        Texture2D _menuImage;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        enum GameState
        {
            Menu,
            Playing,
            Victory
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
            _enemies.Add(new Enemy(new Vector2(335, 100)));
            _enemies.Add(new Enemy(new Vector2(375, 100)));
            _enemies.Add(new Enemy(new Vector2(430, 100)));
            _enemies.Add(new Enemy(new Vector2(50, 300)));
            _enemies.Add(new Enemy(new Vector2(680, 300)));

            _portalBounds = new Rectangle(355, 210, 90, 127);

            // inicializar parede perto do player (exemplo posição e tamanho)
            if (_currentArea == 3) _wall = new Wall(new Vector2(100, 180), 590, 135);
            else _wall = new Wall(new Vector2(357, 207), 85, 120);

            _playerWorldPosition = new Vector2(400, 400); // Começar no meio visível
            _brownWall = new BrownWall(new Vector2(450, 300), 100, 20); // Perto do player

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

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // boss com animação de 4 frames
            bossFrames = new Texture2D[4];

            bossFrames[0] = Content.Load<Texture2D>("boss1");
            bossFrames[1] = Content.Load<Texture2D>("boss2");
            bossFrames[2] = Content.Load<Texture2D>("boss3");
            bossFrames[3] = Content.Load<Texture2D>("boss4");

            //melee enemy
            sheepFrames = new Texture2D[2];

            sheepFrames[0] = Content.Load<Texture2D>("OvelhaAzul_1");
            sheepFrames[1] = Content.Load<Texture2D>("OvelhaAzul_2");

            //ranged enemy
            rangedFramesDown = new Texture2D[2];
            rangedFramesDown[0] = Content.Load<Texture2D>("rangedEnemyDown_0");
            rangedFramesDown[1] = Content.Load<Texture2D>("rangedEnemyDown_1");

            rangedFramesLeft = new Texture2D[2];
            rangedFramesLeft[0] = Content.Load<Texture2D>("rangedEnemyLeft_0");
            rangedFramesLeft[1] = Content.Load<Texture2D>("rangedEnemyLeft_1");

            rangedFramesRight = new Texture2D[2];
            rangedFramesRight[0] = Content.Load<Texture2D>("rangedEnemyRight_0");
            rangedFramesRight[1] = Content.Load<Texture2D>("rangedEnemyRight_1");

            rangedFramesUp = new Texture2D[2];
            rangedFramesUp[0] = Content.Load<Texture2D>("rangedEnemyUp_0");
            rangedFramesUp[1] = Content.Load<Texture2D>("rangedEnemyUp_1");

            //sound effects
            attackSound = Content.Load<SoundEffect>("Hurtsound");
            damagedSound = Content.Load<SoundEffect>("takingDamage");

            foreach (var enemy in _enemies)
            {
                enemy.LoadFrames(sheepFrames);
            }

            // carregar mapas por niveis
            // nivel 1: imagem nivel1ovelhas (vai ser esticada para ocupar o ecrã)
            _areaMaps[1] = Content.Load<Texture2D>("nivel1ovelhas");
            // nivel 2: mapa que será usado em mosaico (tile)
            _areaMaps[2] = Content.Load<Texture2D>("mapadosputos");
            // nivel 3: mapa do boss (vai ser esticado para ocupar o ecrã)
            _areaMaps[3] = Content.Load<Texture2D>("mapaboss");

            // definir tamanho do mapa inicial (area 1,2,3) - esticado ou tiled para ocupar o ecrã
            if (_currentArea == 1 || _currentArea == 2 || _currentArea == 3)
            {
                _mapWidth = GraphicsDevice.Viewport.Width;
                _mapHeight = GraphicsDevice.Viewport.Height;
            }
            else
            {
                _mapWidth = _areaMaps[_currentArea].Width;
                _mapHeight = _areaMaps[_currentArea].Height;
            }

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
            _font = Content.Load<SpriteFont>("DefaultFont");
            _brownWall.LoadContent(GraphicsDevice);

            // Criamos a textura da bala uma só vez
            _bulletTexture = new Texture2D(GraphicsDevice, 10, 10);
            Color[] bData = new Color[10 * 10];
            for (int i = 0; i < bData.Length; ++i) bData[i] = Color.Purple;
            _bulletTexture.SetData(bData);
        }


        // lógica (teclas, movimento, menu)
        protected override void Update(GameTime gameTime)
        {



            // Atualizar animação do boss
            foreach (var enemy in _enemies)
            {
                if (enemy is BossEnemy boss)
                {
                    boss.UpdateAnimation(gameTime);
                }
                else enemy.UpdateAnimation(gameTime);
            }


            // coisas do menu
            _keyboard = Keyboard.GetState();

            if (_currentState == GameState.Menu)
            {
                _prevMouse = Mouse.GetState();

                if (_keyboard.IsKeyDown(Keys.Down) && _prevKeyboard.IsKeyUp(Keys.Down))
                    _selectedIndex++;

                if (_keyboard.IsKeyDown(Keys.Up) && _prevKeyboard.IsKeyUp(Keys.Up))
                    _selectedIndex--;

                _selectedIndex = Math.Clamp(_selectedIndex, 0, _menuOptions.Length - 1);

                if (_keyboard.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
                    HandleMenuSelection();
            }

            if (_currentState == GameState.Victory)
            {
                if (_keyboard.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
                {
                    _currentState = GameState.Menu;
                    _playerWorldPosition = new Vector2(400, 400);
                    _playerLevel = 1;
                    _currentXp = 0;
                    _playerAttack.AttackLevel = 1;
                    _currentArea = 1;
                    _portalActive = false;
                    _enemies.Clear();
                    Enemy enemy = new Enemy(new Vector2(200, 200));
                    enemy.LoadFrames(sheepFrames);
                    _enemies.Add(enemy);
                    _enemyBullets.Clear();
                    _experienceGems.Clear();
                    _playerHealth = 100;
                    _playerMaxHealth = 100;
                }
            }

            //coisas dos playereszinhos e com o mundo a mover-se
            if (_currentState == GameState.Playing)
            {
                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                if (keyboard.IsKeyDown(Keys.W))
                {
                    Vector2 nextPos = _playerWorldPosition;
                    nextPos.Y -= 3;
                    Rectangle nextBounds = new Rectangle((int)nextPos.X, (int)nextPos.Y, 32, 32);
                    if (!_wall.Bounds.Intersects(nextBounds) && !_brownWall.Bounds.Intersects(nextBounds))
                        _playerWorldPosition.Y -= 3;
                    _currentPlayerTexture = _playerUp;
                    _playerFaceDirection = new Vector2(0, -1);
                }

                if (keyboard.IsKeyDown(Keys.S))
                {
                    Vector2 nextPos = _playerWorldPosition;
                    nextPos.Y += 3;
                    Rectangle nextBounds = new Rectangle((int)nextPos.X, (int)nextPos.Y, 32, 32);
                    if (!_wall.Bounds.Intersects(nextBounds) && !_brownWall.Bounds.Intersects(nextBounds))
                        _playerWorldPosition.Y += 3;
                    _currentPlayerTexture = _playerDown;
                    _playerFaceDirection = new Vector2(0, 1);
                }

                if (keyboard.IsKeyDown(Keys.A))
                {
                    Vector2 nextPos = _playerWorldPosition;
                    nextPos.X -= 3;
                    Rectangle nextBounds = new Rectangle((int)nextPos.X, (int)nextPos.Y, 32, 32);
                    if (!_wall.Bounds.Intersects(nextBounds) && !_brownWall.Bounds.Intersects(nextBounds))
                        _playerWorldPosition.X -= 3;
                    _currentPlayerTexture = _playerLeft;
                    _playerFaceDirection = new Vector2(-1, 0);
                }

                if (keyboard.IsKeyDown(Keys.D))
                {
                    Vector2 nextPos = _playerWorldPosition;
                    nextPos.X += 3;
                    Rectangle nextBounds = new Rectangle((int)nextPos.X, (int)nextPos.Y, 32, 32);
                    if (!_wall.Bounds.Intersects(nextBounds) && !_brownWall.Bounds.Intersects(nextBounds))
                        _playerWorldPosition.X += 3;
                    _currentPlayerTexture = _playerRight;
                    _playerFaceDirection = new Vector2(1, 0);
                }

                // Usar dimensões efetivas: para as areas esticadas/tiled (1,2,3) limitar pelo tamanho da viewport
                int effectiveWidth = (_currentArea == 1 || _currentArea == 2 || _currentArea == 3)
                    ? GraphicsDevice.Viewport.Width
                    : _mapWidth;
                int effectiveHeight = (_currentArea == 1 || _currentArea == 2 || _currentArea == 3)
                    ? GraphicsDevice.Viewport.Height
                    : _mapHeight;

                _playerWorldPosition.X = Math.Clamp(_playerWorldPosition.X, 0, effectiveWidth - 32);
                _playerWorldPosition.Y = Math.Clamp(_playerWorldPosition.Y, 0, effectiveHeight - 32);

                // Disparar o ataque ao pressionar espaço ou clique esquerdo do mouse
                if ((keyboard.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space)) ||
                    (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released && !_menuButtonRect.Contains(mouse.Position)))
                {
                    Vector2 attackDirection = _playerFaceDirection;

                    if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
                    {
                        // Quando não há mais tracking the câmera, mouse.X e Y já apontam diretamente para o world position correspondente
                        Vector2 mouseWorldPos = new Vector2(mouse.X, mouse.Y);
                        Vector2 direction = mouseWorldPos - _playerWorldPosition;
                        if (direction.LengthSquared() > 0)
                        {
                            direction.Normalize();
                            attackDirection = direction;
                        }
                    }

                    _playerAttack.StartAttack(_playerWorldPosition, attackDirection);
                    attackSound.Play();
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

                var obstacles = new System.Collections.Generic.List<Rectangle>();

                Rectangle playerBounds = new Rectangle(
                    (int)_playerWorldPosition.X,
                    (int)_playerWorldPosition.Y,
                    32, 32 // assumindo tamanho 32x32 para o player
                );

                _timeSinceLastEnemyCollision += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Processar balas
                for (int i = _enemyBullets.Count - 1; i >= 0; i--)
                {
                    _enemyBullets[i].Update();

                    if (_enemyBullets[i].Bounds.Intersects(playerBounds) && _enemyBullets[i].IsActive)
                    {
                        if (_timeSinceLastEnemyCollision >= 1f) // Usa mesmo i-frame
                        {
                            _playerHealth -= _enemyBullets[i].Damage;
                            damagedSound.Play();
                            _timeSinceLastDamage = 0f;
                            _timeSinceLastEnemyCollision = 0f;
                        }
                        _enemyBullets[i].IsActive = false;
                    }

                    if (!_enemyBullets[i].IsActive || _enemyBullets[i].Position.X < 0 || _enemyBullets[i].Position.X > _mapWidth || _enemyBullets[i].Position.Y < 0 || _enemyBullets[i].Position.Y > _mapHeight)
                    {
                        _enemyBullets.RemoveAt(i);
                    }
                }

                // Processar Inimigos
                for (int i = _enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = _enemies[i];

                    // Criar uma cópia isolada de obstáculos para colocar os outros inimigos lá para o teste de colisão
                    var currentEnemyObstacles = new System.Collections.Generic.List<Rectangle>(obstacles);
                    for (int j = 0; j < _enemies.Count; j++)
                    {
                        if (i != j && _enemies[j].Health > 0)
                        {
                            currentEnemyObstacles.Add(_enemies[j].Bounds);
                        }
                    }

                    if (enemy is RangedEnemy rangedEnemy)
                    {
                        //calcular direçao do ranged enemy para adaptar as textures
                        if (Math.Abs(rangedEnemy.direction.X) > Math.Abs(rangedEnemy.direction.Y))
                        {
                            if(rangedEnemy.direction.X > 0)
                            {
                                enemy.LoadFrames(rangedFramesRight);
                            }
                            else if (rangedEnemy.direction.X < 0)
                            {
                                enemy.LoadFrames(rangedFramesLeft);
                            }
                        }
                        if (Math.Abs(rangedEnemy.direction.Y) > Math.Abs(rangedEnemy.direction.X))
                        {
                            if (rangedEnemy.direction.Y > 0)
                            {
                                enemy.LoadFrames(rangedFramesDown);
                            }
                            else if (rangedEnemy.direction.Y < 0)
                            {
                                enemy.LoadFrames(rangedFramesUp);
                            }
                        }
                        var bullet = rangedEnemy.UpdateRanged(gameTime, _playerWorldPosition, currentEnemyObstacles);
                        if (bullet != null) _enemyBullets.Add(bullet);
                    }
                    else if(enemy is BossEnemy bossEnemy)
                    {
                        bossEnemy.UpdateBossAttacks(gameTime, _playerWorldPosition);
                        var bossBullets = bossEnemy.BossBullets;
                        // Handle collision logic with boss bullets inside Game1
                        for (int b = bossBullets.Count - 1; b >= 0; b--)
                        {
                            if (bossBullets[b].Bounds.Intersects(playerBounds))
                            {
                                if (_timeSinceLastEnemyCollision >= 1f)
                                {
                                    _playerHealth -= bossBullets[b].Damage;
                                    damagedSound.Play();
                                    _timeSinceLastDamage = 0f;
                                    _timeSinceLastEnemyCollision = 0f;
                                    bossBullets.RemoveAt(b);
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        enemy.Update(_playerWorldPosition, currentEnemyObstacles);
                    }

                    if (enemy.Health > 0 && enemy.Bounds.Intersects(playerBounds))
                    {
                        if (_timeSinceLastEnemyCollision >= 1f)
                        {
                            _playerHealth -= 5;
                            damagedSound.Play();
                            _timeSinceLastDamage = 0f;
                            _timeSinceLastEnemyCollision = 0f;

                            if (_playerHealth <= 0)
                            {
                                _playerHealth = _playerMaxHealth;
                                _currentState = GameState.Menu;
                            }
                        }
                    }

                    if (enemy.Health > 0 && _playerAttack.IsAttacking && !_enemyHitByCurrentAttack && _playerAttack.CheckCollision(enemy.Bounds))
                    {
                        enemy.TakeDamage(_playerAttack.Damage);
                        _enemyHitByCurrentAttack = true;

                        if (enemy.Health <= 0)
                        {
                            if (_currentArea == 1)
                            {
                                _experienceGems.Add(new ExperienceGem(
                                new Vector2(enemy.WorldPosition.X + 5, enemy.WorldPosition.Y + 5), 1, 0.4f));
                            }
                            if (_currentArea == 2)
                            {
                                _experienceGems.Add(new ExperienceGem(
                                new Vector2(enemy.WorldPosition.X + 5, enemy.WorldPosition.Y + 5), 1, 0.31f));
                            }
                        }
                    }

                    if (enemy.Health <= 0)
                    {
                        _enemies.RemoveAt(i);

                        // Victory Check (se estiver na area 3 e ja nao existirem inimigos (o boss for o unico))
                        if (_currentArea == 3 && _enemies.Count == 0)
                        {
                            _currentState = GameState.Victory;
                        }
                    }
                }

                // Portal logica
                if (_portalActive && _portalBounds.Intersects(playerBounds))
                {
                    _currentArea++;
                    _portalActive = false;
                    _enemies.Clear();
                    _enemyBullets.Clear();

                    // atualizar dimensões do mapa para a nova area
                    if (_currentArea == 1)
                    {
                        _mapWidth = GraphicsDevice.Viewport.Width;
                        _mapHeight = GraphicsDevice.Viewport.Height;
                    }
                    else if (_currentArea == 2)
                    {
                        // nivel 2 usa mosaico para cobrir o ecrã: permitir movimento por todo o ecrã
                        _mapWidth = GraphicsDevice.Viewport.Width;
                        _mapHeight = GraphicsDevice.Viewport.Height;
                    }
                    else if (_areaMaps[_currentArea] != null)
                    {
                        _mapWidth = _areaMaps[_currentArea].Width;
                        _mapHeight = _areaMaps[_currentArea].Height;
                    }

                    if (_currentArea == 2)
                    {
                        _playerWorldPosition = new Vector2(400, 400); // Reset position meio screen

                        RangedEnemy enemy1 = new RangedEnemy(new Vector2(335, 100));
                        enemy1.LoadFrames(sheepFrames);

                        RangedEnemy enemy2 = new RangedEnemy(new Vector2(430, 100));
                        enemy2.LoadFrames(sheepFrames);

                        RangedEnemy enemy3 = new RangedEnemy(new Vector2(50, 300));
                        enemy3.LoadFrames(sheepFrames);

                        RangedEnemy enemy4 = new RangedEnemy(new Vector2(680, 300));
                        enemy4.LoadFrames(sheepFrames);

                        RangedEnemy enemy5 = new RangedEnemy(new Vector2(375, 100));
                        enemy5.LoadFrames(sheepFrames);

                        RangedEnemy enemy6 = new RangedEnemy(new Vector2(300, 100));
                        enemy6.LoadFrames(sheepFrames);

                        RangedEnemy enemy7 = new RangedEnemy(new Vector2(470, 100));
                        enemy7.LoadFrames(sheepFrames);

                        RangedEnemy enemy8 = new RangedEnemy(new Vector2(50, 400));
                        enemy8.LoadFrames(sheepFrames);

                        RangedEnemy enemy9 = new RangedEnemy(new Vector2(680, 400));
                        enemy9.LoadFrames(sheepFrames);

                        RangedEnemy enemy10 = new RangedEnemy(new Vector2(50, 100));
                        enemy10.LoadFrames(sheepFrames);

                        _enemies.Add(enemy1); // Mudou de meelee para ranged
                        _enemies.Add(enemy2);
                        _enemies.Add(enemy3);
                        _enemies.Add(enemy4);
                        _enemies.Add(enemy5);
                        _enemies.Add(enemy6);
                        _enemies.Add(enemy7);
                        _enemies.Add(enemy8);
                        _enemies.Add(enemy9);
                        _enemies.Add(enemy10);
                    }
                    if (_currentArea == 3)
                    {
                        // garantir dimensões do mapa correspondem ao ecrã para o nível 3
                        _mapWidth = GraphicsDevice.Viewport.Width;
                        _mapHeight = GraphicsDevice.Viewport.Height;

                        // posicionar o jogador no centro do ecrã ao entrar no nível 3
                        _playerWorldPosition = new Vector2(400, 400);


                        // posicionar o boss no centro do ecrã (ajustando pela metade da sua dimensão estimada)
                        float bossHalfW = 30f; // estimativa baseada no comentário original (60x60)
                        float bossHalfH = 30f;
                        BossEnemy boss = new BossEnemy(
                            new Vector2(GraphicsDevice.Viewport.Width / 2f - bossHalfW, GraphicsDevice.Viewport.Height / 2f - bossHalfH)
                        );

                        boss.LoadBossFrames(bossFrames);

                        _enemies.Add(boss); // Spawn no meio (60x60, logo tiramos metade para centralizar)
                    }
                }


                // Range de atração do player (exemplo 80x80 centrado no player)
                Rectangle playerMagnetBounds = new Rectangle(
                    (int)_playerWorldPosition.X - 24,
                    (int)_playerWorldPosition.Y - 24,
                    80, 80
                );

                // Coletar pedras de XP
                for (int i = _experienceGems.Count - 1; i >= 0; i--)
                {
                    // Lógica de Atracção Magnética (Pick Up Range)
                    if (playerMagnetBounds.Intersects(_experienceGems[i].Bounds))
                    {
                        Vector2 direction = _playerWorldPosition - _experienceGems[i].WorldPosition;
                        if (direction != Vector2.Zero)
                        {
                            direction.Normalize();
                            _experienceGems[i].WorldPosition += direction * 4f; // Velocidade em que é sugada
                        }
                    }

                    if (playerBounds.Intersects(_experienceGems[i].Bounds))
                    {
                        // Coleciona a pedra se nao max
                        if (_playerLevel < 3)
                        {
                            _currentXp += _experienceGems[i].XpAmount;
                        }

                        _experienceGems.RemoveAt(i);

                        // Checar level up
                        if (_currentXp >= _xpToNextLevel)
                        {
                            _currentXp -= _xpToNextLevel;
                            _playerLevel++;
                            _xpToNextLevel = (int)(_xpToNextLevel * 1.5f); // Aumenta a exp necessária para o próximo

                            // Aumentar nível do ataque e vida caso passe do level 2 ou 3 
                            if (_playerLevel >= 2 && _currentArea == 1)
                            {
                                _playerAttack.AttackLevel = 2;
                                _portalActive = true;
                            }
                            if (_playerLevel >= 3 && _currentArea == 2)
                            {
                                _playerAttack.AttackLevel = 3;
                                _portalActive = true;
                            }

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
                // Como não tem mais camera offset, todos os objetos são desenhados diretamente na sua posição de mundo

                // mapa: escolher por area
                Texture2D currentMap = _areaMaps[_currentArea];

                if (currentMap == null)
                {
                    // fallback: desenhar fundo vazio se não houver mapa
                    Texture2D empty = new Texture2D(GraphicsDevice, 1, 1);
                    empty.SetData(new[] { Color.CornflowerBlue });
                    _spriteBatch.Draw(empty, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                }
                else if (_currentArea == 1 || _currentArea == 3)
                {
                    // nível 1 e 3: esticar para ocupar o ecrã inteiro
                    Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _spriteBatch.Draw(currentMap, screenRect, Color.White);
                }
                else if (_currentArea == 2)
                {
                    // nível 2: desenhar como mosaico (tile) para cobrir o ecrã
                    int texW = currentMap.Width;
                    int texH = currentMap.Height;
                    int screenW = GraphicsDevice.Viewport.Width;
                    int screenH = GraphicsDevice.Viewport.Height;

                    for (int x = 0; x < screenW; x += texW)
                    {
                        for (int y = 0; y < screenH; y += texH)
                        {
                            _spriteBatch.Draw(currentMap, new Vector2(x, y), Color.White);
                        }
                    }
                }
                else
                {
                    // desenhar mapa nas suas dimensões normais
                    _spriteBatch.Draw(currentMap, Vector2.Zero, Color.White);
                }

                // mini menu
                _spriteBatch.Draw(_menuButton, _menuButtonRect, Color.White);

            Vector2 cameraOffset = Vector2.Zero; // sem offset para o caso de ainda o calcular
            _brownWall.Draw(_spriteBatch, cameraOffset);

            if (_portalActive)
                {
                    Texture2D portalTex = new Texture2D(GraphicsDevice, 50, 50);
                    Color[] data = new Color[50 * 50];
                    for (int i = 0; i < data.Length; ++i) data[i] = Color.Cyan;
                    portalTex.SetData(data);
                    _spriteBatch.Draw(portalTex, _portalBounds, Color.White);
                }

                // desenhar inimigos
                foreach (var enemy in _enemies)
                {
                    if (enemy is BossEnemy boss)
                    {
                        boss.Draw(_spriteBatch, boss.WorldPosition, boss.WorldPosition);
                    }
                    else
                    {
                        enemy.Draw(_spriteBatch);
                    }
                }

            // playerzinho no mundo (desenhar depois dos inimigos para garantir visibilidade)
            _spriteBatch.Draw(_currentPlayerTexture, _playerWorldPosition, Color.White);

                // desenhar balas
                foreach (var bullet in _enemyBullets)
                {
                    bullet.Draw(_spriteBatch, _bulletTexture);
                }

                // desenhar gemas de XP
                foreach (var gem in _experienceGems)
                {
                    gem.Draw(_spriteBatch, gem.WorldPosition, gem.WorldPosition);
                }

                // desenhar o ataque visual (hitbox do ataque)
                _playerAttack.Draw(_spriteBatch, _playerWorldPosition, _playerWorldPosition);

                // Desenhar a vida do jogador por cima
                _spriteBatch.DrawString(_font, $"Vida: {_playerHealth}/{_playerMaxHealth}", new Vector2(10, 10), Color.Red);

                // Desenhar nível e XP
                string lvlText = _playerLevel >= 3 ? "Nvl: MAX" : $"Nvl: {_playerLevel}";
                Vector2 lvlTextSize = _font.MeasureString(lvlText);
                _spriteBatch.DrawString(_font, lvlText, new Vector2(10, 35), Color.Gold);

                if (_playerLevel < 3)
                {
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
            }
            else if (_currentState == GameState.Victory)
            {
                // Placeholder para imagem comemorativa futura
                Texture2D backgroundVic = new Texture2D(GraphicsDevice, 1, 1);
                backgroundVic.SetData(new[] { Color.DarkOrange });
                _spriteBatch.Draw(backgroundVic, new Rectangle(0, 0, _mapWidth, _mapHeight), Color.White);

                string vicText = "VICTORIA! BOSS DERROTADO";
                Vector2 vicTextSize = _font.MeasureString(vicText);
                _spriteBatch.DrawString(_font, vicText, new Vector2(_mapWidth / 2 - vicTextSize.X / 2, _mapHeight / 2 - 50), Color.White);

                string retText = "Pressiona [ENTER] para regressar ao Menu";
                Vector2 retTextSize = _font.MeasureString(retText);
                _spriteBatch.DrawString(_font, retText, new Vector2(_mapWidth / 2 - retTextSize.X / 2, _mapHeight / 2 + 50), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
