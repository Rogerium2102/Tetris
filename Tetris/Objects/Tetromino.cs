using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Objects
{
    internal class Tetromino
    {

        private Square[] squares = new Square[4];
        private Color _colour;
        private Random _random = new Random();
        private Vector2 _position;
        private int _tetID;
        private Rectangle _board;
        private bool stopped = false;
        private int rotation = 0;
        private Vector2 debugPivot;
        private Rectangle[] _bounding;
        private int Width;
        private int Height;
        public Tetromino(Texture2D texture, int tetromino, Rectangle board, Vector2 position) {
            _tetID = tetromino;
            _colour = pickRandomColor();
            _board = board;
            _position = position;
            for (int i = 0; i < squares.Length; i++)
            {
                squares[i] = new Square(texture, _colour);
            }
            _bounding = calcBoundingRectangles();
        }

        private Color pickRandomColor()
        {
            return new Color(new Vector3(_random.Next(0, 256), _random.Next(0, 256), _random.Next(0, 256)));
        }

        private bool areSquaresDefault()
        {
            foreach (Square square in squares)
            {
                if (square.getPosition() != new Vector2(0, 0))
                {
                    return false;
                }
            }
            return true;
        }

        public void setPosition(Vector2 position)
        {
            if (areSquaresDefault())
            {
                assemble(position);
            }
            else
            {
                Vector2 difference = position - _position;
                foreach (Square square in squares)
                {
                    square.setPosition(square.getPosition() + difference);
                }
                _position = position;
                _bounding = calcBoundingRectangles();
            }
        }

        private void assemble(Vector2 position)
        {
            switch (_tetID)
            {
                case 0: //Square
                    createLine(0, 2, false, position);
                    createLine(2, 2, false, new Vector2(position.X, position.Y + Square.length));
                    Width = 2;
                    Height = 2;
                    break;
                case 1: //Bump
                    createLine(0, 3, false, position);
                    squares[3].setPosition(new Vector2(position.X + Square.length, position.Y - Square.length));
                    Width = 3;
                    Height = 2;
                    break;
                case 2: //L
                    createLine(0, 3, true, position);
                    squares[3].setPosition(new Vector2(position.X + Square.length, position.Y + 2 * Square.length));
                    Width = 2;
                    Height = 3;
                    break;
                case 3: //Backwards L
                    createLine(0, 3, true, new Vector2(position.X + Square.length, position.Y));
                    squares[3].setPosition(new Vector2(position.X, position.Y + 2 * Square.length));
                    Width = 2;
                    Height = 3;
                    break;
                case 4: //¬|_
                    createLine(0, 2, false, new Vector2(position.X, position.Y));
                    createLine(2, 2, false, new Vector2(position.X + Square.length, position.Y + Square.length));
                    Width = 3;
                    Height = 2;
                    break;
                case 5: //_|¬
                    createLine(0, 2, false, new Vector2(position.X + Square.length, position.Y));
                    createLine(2, 2, false, new Vector2(position.X, position.Y + Square.length));
                    Width = 3;
                    Height = 2;
                    break;
                case 6: //Line
                    createLine(0, 4, true, position);
                    Width = 1;
                    Height = 4;
                    break;
                default:
                    _tetID = 0;
                    assemble(position);
                    break;
            }
        }

        public void move(int direction)
        {
            Vector2 newPos = _position;
            if (stopped) return;
            switch (direction)
            {
                case 0:
                    if (newPos.X < _board.X + _board.Width - Width * Square.length)
                    {
                        newPos.X += Square.length;
                    }
                    break;
                case 1:
                    if (newPos.X > _board.X)
                    {
                        newPos.X -= Square.length;
                    }
                    break;
                case 2:
                    if (newPos.Y >= _board.Y + _board.Height - Height * Square.length)
                    {
                        stopped = true;
                    }
                    else
                    {
                        newPos.Y += Square.length;
                    }
                    
                    break;
                default:
                    break;
            }
            setPosition(newPos);
        }

        public bool isSquareAboveOthers(Rectangle current)
        {
            foreach (Rectangle c in _bounding)
            {
                if (c.Contains(current.Center + new Point(0,Square.length))) {
                    return true;
                }
            }
            return false;
        }

        public void rotate()
        {
            if (_tetID == 0)
            {
                return;
            }
            Vector2 pivot = getPivot();
            pivot.X = (float)Math.Round(pivot.X / 50) * 50;
            pivot.Y = (float)Math.Round(pivot.Y / 50) * 50;
            Vector2 relativePos = new Vector2();
            Vector2 newPos = new Vector2();
            float storeX = 0; 
            for (int i = 0; i < squares.Length; i++)
            {
                relativePos = (squares[i].getPosition() - pivot) / Square.length;
                storeX = relativePos.X;
                relativePos.X = -1 * relativePos.Y;
                relativePos.Y = storeX;
                newPos = relativePos * Square.length + pivot;
                squares[i].setPosition(newPos);
            }
            Vector2[] positions = new Vector2[squares.Length];
            for (int j = 0; j < positions.Length; j++) {
                positions[j] = squares[j].getPosition();
            }
            _position = getSquareClosestTo(new Vector2(_board.X, 0)).getPosition();
            rotation = (rotation + 1) % 4;
            int widthStore = Width;
            Width = Height;
            Height = widthStore;
        }

        public Vector2 getPivot()
        {
            Vector2[] positions = new Vector2[squares.Length];
            Rectangle[] rectangles = new Rectangle[squares.Length];
            for (int i = 0; i < squares.Length; i++)
            {
                positions[i] = squares[i].getPosition();
                rectangles[i] = new Rectangle((int)positions[i].X, (int)positions[i].Y, Square.length, Square.length);
            }
            float X = 0;
            float Y = 0;
            foreach (Vector2 position in positions)
            {
                X += position.X;
                Y += position.Y;
            }
            return new Vector2(X / squares.Length,Y / squares.Length);
        }

        private int findClosest(Vector2 center, Vector2[] positions)
        {
            Vector2[] distances = new Vector2[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                distances[i] = positions[i] - center;
            }
            Vector2 min = distances[0];
            int index = 0;
            for (int i = 1; i < distances.Length; i++)
            {
                if (distances[i].Length() < min.Length())
                {
                    min = distances[i];
                    index = i;
                }
            }
            return index;
        }

        private void createLine(int start, int length, bool vertical, Vector2 position)
        {
            int count = 0;
            int i = start;
            while (count < length)
            {
                if (i > squares.Length - 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else if (vertical)
                {
                    squares[i].setPosition(new Vector2(position.X, position.Y + count * Square.length));
                }
                else
                {
                    squares[i].setPosition(new Vector2((position.X + count * Square.length), position.Y));
                }
                count++;
                i++;
            }
        }

        public Vector2 getDebugVector()
        {
            return debugPivot;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            foreach (Square square in squares)
            {
                square.draw(spriteBatch);
            }
        }

        public bool hasStopped()
        {
            return stopped;
        }

        public void stopBlock()
        {
            stopped = true;
        }

        public void restartBlock()
        {
            stopped = false;
        }

        public Rectangle[] calcBoundingRectangles()
        {
            Dictionary<Square, bool> visited = new Dictionary<Square, bool>();
            Dictionary<Square, int> map = new Dictionary<Square, int>();
            Square start = getSquareClosestTo(new Vector2(_board.X,0));
            start.resetColor(Color.Red);
            foreach (Square square in squares)
            {
                if (square != start)
                {
                    square.resetColor(_colour);
                }
            }
            List<Rectangle> result = new List<Rectangle>();
            result.Add(new Rectangle((int)start.getPosition().X, (int)start.getPosition().Y, 50, 50));
            map.Add(start, 0);
            return calcBoundingRecursive(start, visited, result, map, 1).ToArray();
        }

        private Square getSquareClosestTo(Vector2 Position)
        {
            float lengthDist;
            float min = 9999999;
            int index = -1;
            for (int i = 0; i < squares.Length; i++)
            {
                lengthDist = (squares[i].getPosition() - Position).Length();
                if (lengthDist < min)
                {
                    min = lengthDist;
                    index = i;
                }
            }
            return squares[index];

        }
        private List<Rectangle> calcBoundingRecursive(Square current, Dictionary<Square,bool> visited, List<Rectangle> result, Dictionary<Square,int> mapSquareToRect, int currentDir)
        {
            Square[] neighbours = getNeighbours(current, visited);
            visited[current] = true;
            int[] direction = new int[neighbours.Length];
            if (neighbours.Length == 0)
            {
                return result;
            }
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i].getPosition().X - 50 == current.getPosition().X)
                {
                    direction[i] = 1;
                }
                else if (neighbours[i].getPosition().X + 50 == current.getPosition().X)
                {
                    direction[i] = 2;
                }
                else if (neighbours[i].getPosition().Y - 50 == current.getPosition().Y)
                {
                    direction[i] = 3;
                }
                else if (neighbours[i].getPosition().Y + 50 == current.getPosition().Y)
                {
                    direction[i] = 4;
                }
            }
            for (int j = 0; j < neighbours.Length; j++)
            {
                if (visited.ContainsKey(neighbours[j])) {
                    continue;
                }
                result.Add(new Rectangle((int)neighbours[j].getPosition().X, (int)neighbours[j].getPosition().Y, 50, 50));
                mapSquareToRect.Add(neighbours[j], result.Count - 1);
                result = calcBoundingRecursive(neighbours[j], visited, result, mapSquareToRect, direction[j]);
            }
            return result;
        }

        public Square[] getNeighbours(Square current, Dictionary<Square, bool> visited)
        {
            List<Square> neighbours = new List<Square>();
            foreach (Square square in squares)
            {
                if (square == current || visited.ContainsKey(square)) { continue; }
                if (square.getPosition().X + 50 == current.getPosition().X || square.getPosition().X -50 == current.getPosition().X || square.getPosition().Y - 50 == current.getPosition().Y)
                {
                    neighbours.Add(square);
                }
            }
            return neighbours.ToArray();
        }

        public Rectangle[] getBoundingRectangles()
        {
            return _bounding;
        }
    }
}
