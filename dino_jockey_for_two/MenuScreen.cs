using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace dino_jockey_for_two
{
    public class MenuScreen : IDisposable
    {
        public event Action StartRequested;

        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;

        private MouseState _prevMouse;
        private MouseState _currMouse;
        private KeyboardState _prevKeyboard;
        private KeyboardState _currKeyboard;

        private Rectangle _buttonRect;
        private string _title = "Dino Jockey";
        private string _subtitle = "For Two";
        private string _buttonText = "Iniciar";

        private Color _bgTop = new Color(230, 246, 255);
        private Color _bgBottom = new Color(200, 232, 255);
        private Color _cloud = new Color(255, 255, 255, 140);
        private Color _buttonIdle = new Color(255, 170, 200);
        private Color _buttonHover = new Color(255, 140, 185);
        private Color _buttonPress = new Color(245, 110, 165);
        private Color _buttonTextColor = Color.White;
        private Color _titleColor = new Color(70, 70, 90);
        private Color _subtitleColor = new Color(120, 120, 150);

        private bool _isHover;
        private bool _isPress;

        public MenuScreen(SpriteFont font, Texture2D pixel)
        {
            _font = font;
            _pixel = pixel;
            _prevMouse = _currMouse = Mouse.GetState();
            _prevKeyboard = _currKeyboard = Keyboard.GetState();
        }

        public void Resize(Point windowSize)
        {
            int btnWidth = (int)(windowSize.X * 0.28f);
            int btnHeight = (int)(windowSize.Y * 0.11f);
            if (btnWidth < 240) btnWidth = 240;
            if (btnHeight < 70) btnHeight = 70;

            _buttonRect = new Rectangle(
                x: windowSize.X / 2 - btnWidth / 2,
                y: (int)(windowSize.Y * 0.60f),
                width: btnWidth,
                height: btnHeight
            );
        }

        public void Update(GameTime gameTime)
        {
            // Actualiza estados
            _prevMouse = _currMouse;
            _currMouse = Mouse.GetState();
            _prevKeyboard = _currKeyboard;
            _currKeyboard = Keyboard.GetState();

            var mousePoint = new Point(_currMouse.X, _currMouse.Y);
            _isHover = _buttonRect.Contains(mousePoint);

            bool mouseClicked = false;
            if (_isHover)
            {
                // Detecta click real, solo si el botón acaba de ser soltado dentro del botón
                if (_prevMouse.LeftButton == ButtonState.Pressed && _currMouse.LeftButton == ButtonState.Released)
                {
                    mouseClicked = true;
                }
            }

            bool keyPressed =
                (_currKeyboard.IsKeyDown(Keys.Enter) && !_prevKeyboard.IsKeyDown(Keys.Enter)) ||
                (_currKeyboard.IsKeyDown(Keys.Space) && !_prevKeyboard.IsKeyDown(Keys.Space)) ||
                (_currKeyboard.IsKeyDown(Keys.Up) && !_prevKeyboard.IsKeyDown(Keys.Up));

            if (mouseClicked || keyPressed)
            {
                StartRequested?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewport)
        {
            DrawBackground(spriteBatch, viewport);
            DrawTitle(spriteBatch, viewport);
            DrawStartButton(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch, Rectangle viewport)
        {
            int bands = 20;
            for (int i = 0; i < bands; i++)
            {
                float t = i / (float)(bands - 1);
                var color = Color.Lerp(_bgTop, _bgBottom, t);
                var rect = new Rectangle(
                    viewport.X,
                    viewport.Y + (int)(viewport.Height * i / (float)bands),
                    viewport.Width,
                    viewport.Height / bands + 2
                );
                spriteBatch.Draw(_pixel, rect, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
            }

            DrawCloud(spriteBatch, new Vector2(viewport.Width * 0.18f, viewport.Height * 0.22f), 80);
            DrawCloud(spriteBatch, new Vector2(viewport.Width * 0.72f, viewport.Height * 0.28f), 110);
            DrawCloud(spriteBatch, new Vector2(viewport.Width * 0.50f, viewport.Height * 0.18f), 70);
        }

        private void DrawCloud(SpriteBatch spriteBatch, Vector2 center, float size)
        {
            float z = 0.2f;
            var c = _cloud;
            spriteBatch.Draw(_pixel, new Rectangle((int)(center.X - size), (int)(center.Y - size * 0.25f), (int)(size * 1.1f), (int)(size * 0.5f)), null, c, 0f, Vector2.Zero, SpriteEffects.None, z);
            spriteBatch.Draw(_pixel, new Rectangle((int)(center.X - size * 0.3f), (int)(center.Y - size * 0.35f), (int)(size * 1.2f), (int)(size * 0.6f)), null, c, 0f, Vector2.Zero, SpriteEffects.None, z);
            spriteBatch.Draw(_pixel, new Rectangle((int)(center.X + size * 0.1f), (int)(center.Y - size * 0.25f), (int)(size * 1.0f), (int)(size * 0.5f)), null, c, 0f, Vector2.Zero, SpriteEffects.None, z);
        }

        private void DrawTitle(SpriteBatch spriteBatch, Rectangle viewport)
        {
            var titleScale = Math.Max(1f, viewport.Width / 640f);
            var subtitleScale = Math.Max(1f, viewport.Width / 1000f);

            var titleSize = _font.MeasureString(_title) * titleScale;
            var subtitleSize = _font.MeasureString(_subtitle) * subtitleScale;

            var titlePos = new Vector2(viewport.Center.X - titleSize.X / 2, viewport.Height * 0.22f - titleSize.Y / 2);
            var subtitlePos = new Vector2(viewport.Center.X - subtitleSize.X / 2, viewport.Height * 0.32f - subtitleSize.Y / 2);

            var shadowOffset = new Vector2(3, 3);
            spriteBatch.DrawString(_font, _title, titlePos + shadowOffset, Color.White * 0.7f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(_font, _title, titlePos, _titleColor, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0.6f);

            spriteBatch.DrawString(_font, _subtitle, subtitlePos + shadowOffset * 0.7f, Color.White * 0.5f, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(_font, _subtitle, subtitlePos, _subtitleColor, 0f, Vector2.Zero, subtitleScale, SpriteEffects.None, 0.6f);
        }

        private void DrawStartButton(SpriteBatch spriteBatch)
        {
            var color = _buttonIdle;
            if (_isPress) color = _buttonPress;
            else if (_isHover) color = _buttonHover;

            int radius = Math.Min(_buttonRect.Height, _buttonRect.Width) / 6;
            DrawRoundedRect(spriteBatch, _buttonRect, radius, color, 0.8f);

            var textSize = _font.MeasureString(_buttonText);
            var textPos = new Vector2(
                _buttonRect.X + _buttonRect.Width / 2f - textSize.X / 2f,
                _buttonRect.Y + _buttonRect.Height / 2f - textSize.Y / 2f
            );
            spriteBatch.DrawString(_font, _buttonText, textPos + new Vector2(1, 2), Color.Black * 0.2f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.95f);
            spriteBatch.DrawString(_font, _buttonText, textPos, _buttonTextColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);
        }

        private void DrawRoundedRect(SpriteBatch spriteBatch, Rectangle rect, int radius, Color color, float depth)
        {
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - 2 * radius, rect.Height), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + radius, radius, rect.Height - 2 * radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.Right - radius, rect.Y + radius, radius, rect.Height - 2 * radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - 2 * radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + radius, rect.Bottom - radius, rect.Width - 2 * radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.Right - radius, rect.Y, radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - radius, radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
            spriteBatch.Draw(_pixel, new Rectangle(rect.Right - radius, rect.Bottom - radius, radius, radius), null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);
        }

        public void Dispose()
        {
            _pixel?.Dispose();
        }
    }
}
