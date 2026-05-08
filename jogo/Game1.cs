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

                if (keyboard.IsKeyDown(Keys.W))
                {
                    _playerWorldPosition.Y -= 3;
                    _currentPlayerTexture = _playerUp;
                }

                if (keyboard.IsKeyDown(Keys.S))
                {
                    _playerWorldPosition.Y += 3;
                    _currentPlayerTexture = _playerDown;
                }

                if (keyboard.IsKeyDown(Keys.A))
                {
                    _playerWorldPosition.X -= 3;
                    _currentPlayerTexture = _playerLeft;
                }

                if (keyboard.IsKeyDown(Keys.D))
                {
                    _playerWorldPosition.X += 3;
                    _currentPlayerTexture = _playerRight;
                }
                ;

                //playerzinho acede ao mini menu
                MouseState mouse = Mouse.GetState();

                if (mouse.LeftButton == ButtonState.Pressed &&
                    _menuButtonRect.Contains(mouse.Position))
                {
                    _currentState = GameState.Menu;
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
                }
            


            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
