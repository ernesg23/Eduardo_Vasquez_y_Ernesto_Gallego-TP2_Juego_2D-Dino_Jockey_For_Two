using MonoGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DinoJockey.Scenes;

namespace DinoJockey
{
    public class Game1 : Core
    {
        public Game1(): base ("Dino Jockey", 1280, 720, true){}

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            IsMouseVisible = false;

            ChangeScene(new GameScene());
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }
    }
}
