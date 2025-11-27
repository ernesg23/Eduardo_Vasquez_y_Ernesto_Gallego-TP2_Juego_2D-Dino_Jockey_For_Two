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
            IsMouseVisible = true;

            // Iniciar en el menú de inicio
            ChangeScene(new StartMenuScene());
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }
    }
}
