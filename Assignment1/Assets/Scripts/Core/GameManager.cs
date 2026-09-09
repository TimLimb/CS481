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

        private const int HardModeMinShuffle = 16;
        private const int HardModeMaxShuffle = 30;

        private Board board;
        private readonly IPuzzleSolver solver = new PuzzleSolver();
        private bool isSolving;
        private bool isHardMode;
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
            uiManager.HardModeButton.onClick.AddListener(() => OnSizeSelected(3, hardMode: true));
            uiManager.ShuffleButton.onClick.AddListener(OnShuffleButtonClicked);
            uiManager.SolveButton.onClick.AddListener(OnSolveButtonClicked);
            uiManager.RestartButton.onClick.AddListener(OnRestartButtonClicked);
            uiManager.ContinueButton.onClick.AddListener(OnShuffleButtonClicked);
            uiManager.HomeButton.onClick.AddListener(OnHomeButtonClicked);
        }

        private void OnSizeSelected(int size, bool hardMode = false)
        {
            boardSize = size;
            isHardMode = hardMode;
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
            board = GenerateShuffledBoard();
            boardView.Refresh(board);
            ResetMoveTracking();
        }

        private Board GenerateShuffledBoard()
        {
            if (!isHardMode)
            {
                return new PuzzleGenerator().Generate(boardSize, ComputeShuffleMoveCount());
            }

            // The random-walk shuffle count only approximates the resulting optimal solve
            // length (shortcuts can appear), so retry until Par actually lands in range.
            const int maxAttempts = 25;
            Board candidate = null;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                candidate = new PuzzleGenerator().Generate(boardSize, ComputeShuffleMoveCount());
                int optimalLength = solver.Solve(candidate).Count;
                if (optimalLength >= HardModeMinShuffle && optimalLength <= HardModeMaxShuffle)
                {
                    break;
                }
            }

            return candidate;
        }

        private int ComputeShuffleMoveCount()
        {
            if (isHardMode)
            {
                return Random.Range(HardModeMinShuffle, HardModeMaxShuffle + 1);
            }

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
