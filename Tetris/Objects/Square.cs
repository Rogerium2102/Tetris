using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Objects
{
    public class Square
    {
        public int Id { get; set; }
        private Vector2 _position;
        private Texture2D _texture;
        private Color _color;
        public static int length;

        public Square(Texture2D squareTexture, Color color)
        {
            _texture = squareTexture;
            length = squareTexture.Bounds.Width;
            _color = color;
        }

        public void setPosition(Vector2 position)
        {
            _position = position;
        }

        public void resetColor(Color colour)
        {
            _color = colour;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, _color);
        }

        public Vector2 getPosition() { return _position; }
    }
}
