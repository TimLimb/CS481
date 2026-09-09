// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System;
using System.Collections.Generic;
using UnityEngine;
using SlidingPuzzle.Core;

namespace SlidingPuzzle.Solver
{
    // BFS: every move costs the same, so the first time the goal is dequeued is via the shortest path.
    public class PuzzleSolver : IPuzzleSolver
    {
        private class SearchNode
        {
            public int[] Values;
            public Vector2Int BlankPosition;
            public SearchNode Parent;
            public Vector2Int MoveFromParent;
        }

        public List<Vector2Int> Solve(Board board)
        {
            int size = board.Size;
            int[] startValues = ExtractValues(board, size, out Vector2Int startBlank);
            string startKey = Encode(startValues);
            string goalKey = Encode(BuildGoalValues(size));

            if (startKey == goalKey)
            {
                return new List<Vector2Int>();
            }

            var startNode = new SearchNode
            {
                Values = startValues,
                BlankPosition = startBlank,
                Parent = null
            };

            var queue = new Queue<SearchNode>();
            var visited = new HashSet<string> { startKey };
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                SearchNode current = queue.Dequeue();

                foreach (Vector2Int candidate in GetAdjacentPositions(current.BlankPosition, size))
                {
                    int[] nextValues = (int[])current.Values.Clone();
                    Swap(nextValues, size, current.BlankPosition, candidate);
                    string nextKey = Encode(nextValues);

                    if (visited.Contains(nextKey))
                    {
                        continue;
                    }

                    var nextNode = new SearchNode
                    {
                        Values = nextValues,
                        BlankPosition = candidate,
                        Parent = current,
                        MoveFromParent = candidate
                    };

                    if (nextKey == goalKey)
                    {
                        return ReconstructPath(nextNode);
                    }

                    visited.Add(nextKey);
                    queue.Enqueue(nextNode);
                }
            }

            throw new InvalidOperationException("No solution exists for the given board state.");
        }

        private int[] ExtractValues(Board board, int size, out Vector2Int blankPosition)
        {
            var values = new int[size * size];
            blankPosition = default;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int value = board.GetTile(x, y).Value;
                    values[y * size + x] = value;
                    if (value == 0)
                    {
                        blankPosition = new Vector2Int(x, y);
                    }
                }
            }

            return values;
        }

        private int[] BuildGoalValues(int size)
        {
            var goal = new int[size * size];
            for (int i = 0; i < goal.Length; i++)
            {
                goal[i] = i == goal.Length - 1 ? 0 : i + 1;
            }
            return goal;
        }

        private List<Vector2Int> ReconstructPath(SearchNode goalNode)
        {
            var moves = new List<Vector2Int>();
            for (SearchNode node = goalNode; node.Parent != null; node = node.Parent)
            {
                moves.Add(node.MoveFromParent);
            }
            moves.Reverse();
            return moves;
        }

        private IEnumerable<Vector2Int> GetAdjacentPositions(Vector2Int position, int size)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int direction in directions)
            {
                Vector2Int candidate = position + direction;
                if (candidate.x >= 0 && candidate.x < size && candidate.y >= 0 && candidate.y < size)
                {
                    yield return candidate;
                }
            }
        }

        private void Swap(int[] values, int size, Vector2Int a, Vector2Int b)
        {
            int indexA = a.y * size + a.x;
            int indexB = b.y * size + b.x;
            (values[indexA], values[indexB]) = (values[indexB], values[indexA]);
        }

        private string Encode(int[] values)
        {
            return string.Join(",", values);
        }
    }
}
