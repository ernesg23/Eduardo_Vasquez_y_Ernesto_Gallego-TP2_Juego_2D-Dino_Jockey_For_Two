using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace dino_jockey_for_two;

public class Game1 : Core
{
    private const int WIDTH = 720;
    private const int HEIGTH = 576;
    private GameSession _game1;
    private InputManager _inputManager = new InputManager();

    public Game1() : base("Dino Jockey", WIDTH, HEIGTH, false) { }

    protected override void LoadContent()
    {
        var floor = TextureAtlas.FromFile(Content, "images/floor-definition.xml");
        var dinoAtlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
        int HALF_HEIGTH = Window.ClientBounds.Height / 2;

        _game1 = new GameSession(
            viewport: new Rectangle(0, 0, Window.ClientBounds.Width, HALF_HEIGTH),
            floorSprite: floor.CreateSprite("floor"),
            dinoAtlas: dinoAtlas,
            jumpKey: Keys.Up,
            name: "Player 1"
        );
    }


    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.F4)) { }
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _inputManager.Update(gameTime);
        _game1?.Update(gameTime, _inputManager);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        _game1.Draw(SpriteBatch);

        SpriteBatch.End();
        base.Draw(gameTime);
    }
}
