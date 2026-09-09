// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System;
using System.Collections.Generic;
using UnityEngine;
using SlidingPuzzle.Core;

namespace SlidingPuzzle.Generation
{
    public class PuzzleGenerator
    {
        private const int ShufflesPerTile = 50;

        // Omitting shuffleMoves shuffles far past the puzzle's diameter for full randomness;
        // a smaller value tracks the optimal solve length closely, producing an easier puzzle.
        public Board Generate(int size, int? shuffleMoves = null)
        {
            var board = new Board(size);
            var rng = new System.Random();
            int iterations = shuffleMoves ?? size * size * ShufflesPerTile;

            // Every move is its own inverse, so a random walk of legal moves from a
            // solved board can never leave the solvable half of the permutation space.
            Vector2Int? forbidden = null;
            for (int i = 0; i < iterations; i++)
            {
                List<Vector2Int> moves = board.GetValidMoves();
                if (forbidden.HasValue && moves.Count > 1)
                {
                    moves.Remove(forbidden.Value);
                }

                Vector2Int blankBeforeMove = board.BlankPosition;
                Vector2Int chosen = moves[rng.Next(moves.Count)];
                board.TryMove(chosen);
                forbidden = blankBeforeMove;
            }

            Debug.Assert(IsSolvable(board), "Random-walk shuffle produced an unsolvable board.");
            return board;
        }

        private bool IsSolvable(Board board)
        {
            int size = board.Size;
            var values = new List<int>(size * size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    values.Add(board.GetTile(x, y).Value);
                }
            }

            int inversions = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == 0)
                {
                    continue;
                }

                for (int j = i + 1; j < values.Count; j++)
                {
                    if (values[j] != 0 && values[i] > values[j])
                    {
                        inversions++;
                    }
                }
            }

            if (size % 2 == 1)
            {
                return inversions % 2 == 0;
            }

            int blankRowFromBottom = size - board.BlankPosition.y;
            return (inversions + blankRowFromBottom) % 2 == 1;
        }
    }
}
