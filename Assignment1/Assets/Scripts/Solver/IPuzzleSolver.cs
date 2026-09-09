// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections.Generic;
using UnityEngine;
using SlidingPuzzle.Core;

namespace SlidingPuzzle.Solver
{
    public interface IPuzzleSolver
    {
        List<Vector2Int> Solve(Board board);
    }
}
