using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;

namespace MonoGameLibrary
{
    public class Core : Game
    {
        // Usa variables de instancia, jamás estáticas
        protected GraphicsDeviceManager Graphics;
        protected ContentManager Content;
        protected SpriteBatch SpriteBatch;
        protected GraphicsDevice GraphicsDevice;
        protected InputManager Input;
        protected bool ExitOnEscape;

        protected Core(string title, int width, int height, bool fullScreen)
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.IsFullScreen = fullScreen;
            Graphics.ApplyChanges();

            Window.Title = title;
            Content = base.Content;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            ExitOnEscape = true;
        }

        protected override void Initialize()
        {
            GraphicsDevice = base.GraphicsDevice;
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Input = new InputManager();
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);

            if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }
    }
}
