// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections.Generic;
using UnityEngine;

namespace SlidingPuzzle.Core
{
    public class Board
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        public int Size { get; private set; }
        public Vector2Int BlankPosition => blankPosition;
        private Tile[,] grid;
        private Vector2Int blankPosition;

        public Board(int size)
        {
            Size = size;
            grid = new Tile[size, size];

            int value = 1;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isLastCell = x == size - 1 && y == size - 1;
                    var position = new Vector2Int(x, y);
                    grid[x, y] = new Tile(isLastCell ? 0 : value, position);
                    if (isLastCell)
                    {
                        blankPosition = position;
                    }
                    value++;
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return grid[x, y];
        }

        public bool TryMove(Vector2Int from)
        {
            if (!IsInBounds(from))
            {
                return false;
            }

            if (!IsAdjacentToBlank(from))
            {
                return false;
            }

            Tile moving = grid[from.x, from.y];
            Tile blank = grid[blankPosition.x, blankPosition.y];

            grid[blankPosition.x, blankPosition.y] = moving;
            grid[from.x, from.y] = blank;

            moving.GridPosition = blankPosition;
            blank.GridPosition = from;
            blankPosition = from;

            return true;
        }

        public bool IsSolved()
        {
            int expectedValue = 1;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool isLastCell = x == Size - 1 && y == Size - 1;
                    int expected = isLastCell ? 0 : expectedValue;
                    if (grid[x, y].Value != expected)
                    {
                        return false;
                    }
                    expectedValue++;
                }
            }
            return true;
        }

        public List<Vector2Int> GetValidMoves()
        {
            var moves = new List<Vector2Int>();
            foreach (var direction in Directions)
            {
                var candidate = blankPosition + direction;
                if (IsInBounds(candidate))
                {
                    moves.Add(candidate);
                }
            }
            return moves;
        }

        private bool IsAdjacentToBlank(Vector2Int position)
        {
            int distance = Mathf.Abs(position.x - blankPosition.x) + Mathf.Abs(position.y - blankPosition.y);
            return distance == 1;
        }

        private bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < Size && position.y >= 0 && position.y < Size;
        }
    }
}
