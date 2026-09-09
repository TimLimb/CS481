// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using UnityEngine;

namespace SlidingPuzzle.Core
{
    public class Tile
    {
        public int Value { get; private set; }
        public Vector2Int GridPosition { get; set; }
        public bool IsBlank => Value == 0;

        public Tile(int value, Vector2Int gridPosition)
        {
            Value = value;
            GridPosition = gridPosition;
        }
    }
}
