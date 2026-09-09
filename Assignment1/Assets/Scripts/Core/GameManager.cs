// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlidingPuzzle.Generation;
using SlidingPuzzle.Solver;
using SlidingPuzzle.UI;

namespace SlidingPuzzle.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int boardSize = 3;
        [SerializeField] private BoardView boardView;
        [SerializeField] private UIManager uiManager;

        [SerializeField] private float solveStepDelay = 0.25f;

        private Board board;
        private readonly IPuzzleSolver solver = new PuzzleSolver();
        private bool isSolving;
        private int moveCount;
        private int? parMoveCount;
        private int score;
        private int solvesCompleted;

        private int PointsPerSolve => (boardSize - 2) * 10;

        // Per-size caps, not a formula: BFS has no heuristic, so solve time explodes past a
        // size-specific depth (benchmarked: ~16 moves for 3x3, ~10-12 for 4x4/5x5). These are
        // safety margins around that measured cliff, not a difficulty ceiling.
        private (int Base, int Increment, int Max) GetDifficultyConfig()
        {
            switch (boardSize)
            {
                case 4:
                    return (5, 1, 10);
                case 5:
                    return (6, 1, 10);
                default:
                    return (6, 2, 16);
            }
        }

        private void Start()
        {
            uiManager.HideWinPanel();
            uiManager.HideControlsPanel();
            uiManager.SetBoardVisible(false);
            uiManager.ShowTitlePanel();
            uiManager.Size3Button.onClick.AddListener(() => OnSizeSelected(3));
            uiManager.Size4Button.onClick.AddListener(() => OnSizeSelected(4));
            uiManager.Size5Button.onClick.AddListener(() => OnSizeSelected(5));
            uiManager.ShuffleButton.onClick.AddListener(OnShuffleButtonClicked);
            uiManager.SolveButton.onClick.AddListener(OnSolveButtonClicked);
            uiManager.RestartButton.onClick.AddListener(OnRestartButtonClicked);
            uiManager.ContinueButton.onClick.AddListener(OnShuffleButtonClicked);
            uiManager.HomeButton.onClick.AddListener(OnHomeButtonClicked);
        }

        private void OnSizeSelected(int size)
        {
            boardSize = size;
            uiManager.HideTitlePanel();
            uiManager.ShowControlsPanel();
            uiManager.SetBoardVisible(true);
            board = new Board(boardSize);
            boardView.BuildGrid(boardSize, OnTileSelected);
            boardView.Refresh(board, false);
            ResetMoveTracking();
            score = 0;
            solvesCompleted = 0;
            uiManager.UpdateScore(score);
        }

        private void OnHomeButtonClicked()
        {
            if (isSolving)
            {
                return;
            }

            uiManager.HideWinPanel();
            uiManager.HideControlsPanel();
            uiManager.SetBoardVisible(false);
            uiManager.ShowTitlePanel();
            score = 0;
            solvesCompleted = 0;
            uiManager.UpdateScore(score);
        }

        private void OnShuffleButtonClicked()
        {
            if (isSolving)
            {
                return;
            }

            uiManager.HideWinPanel();
            board = new PuzzleGenerator().Generate(boardSize, ComputeShuffleMoveCount());
            boardView.Refresh(board);
            ResetMoveTracking();
        }

        private int ComputeShuffleMoveCount()
        {
            var (baseMoves, increment, max) = GetDifficultyConfig();
            return Mathf.Min(max, baseMoves + solvesCompleted * increment);
        }

        private void OnRestartButtonClicked()
        {
            if (isSolving)
            {
                return;
            }

            uiManager.HideWinPanel();
            score = 0;
            solvesCompleted = 0;
            uiManager.UpdateScore(score);
            board = new Board(boardSize);
            boardView.Refresh(board, false);
            ResetMoveTracking();
        }

        private void ResetMoveTracking()
        {
            moveCount = 0;
            parMoveCount = board.IsSolved() ? 0 : solver.Solve(board).Count;
            uiManager.UpdateMoveCounter(moveCount, parMoveCount);
        }

        private void OnSolveButtonClicked()
        {
            if (isSolving || board.IsSolved())
            {
                return;
            }

            score = 0;
            uiManager.UpdateScore(score);
            StartCoroutine(SolveRoutine());
        }

        private IEnumerator SolveRoutine()
        {
            isSolving = true;
            List<Vector2Int> moves = solver.Solve(board);
            int autoMoveCount = 0;

            foreach (Vector2Int move in moves)
            {
                board.TryMove(move);
                autoMoveCount++;
                boardView.Refresh(board);
                uiManager.UpdateMoveCounter(autoMoveCount, parMoveCount);
                yield return new WaitForSeconds(solveStepDelay);
            }

            isSolving = false;

            if (board.IsSolved())
            {
                uiManager.ShowWinPanel("<color=#6D28D9><size=140%>Solved!</size></color>\nSolved automatically.", canContinue: false);
            }
        }

        public void OnTileSelected(Vector2Int gridPosition)
        {
            if (isSolving)
            {
                return;
            }

            if (!board.TryMove(gridPosition))
            {
                return;
            }

            moveCount++;
            uiManager.UpdateMoveCounter(moveCount, parMoveCount);
            boardView.Refresh(board);

            if (board.IsSolved())
            {
                score += PointsPerSolve;
                solvesCompleted++;
                uiManager.UpdateScore(score);

                string headline = "<color=#6D28D9><size=140%>You Win!</size></color>";
                string message = parMoveCount.HasValue
                    ? $"{headline}\nMoves: {moveCount}  (Par: {parMoveCount.Value})\n<color=#F97316>+{PointsPerSolve} points!</color>"
                    : $"{headline}\n<color=#F97316>+{PointsPerSolve} points!</color>";
                uiManager.ShowWinPanel(message, canContinue: true);
            }
        }
    }
}
