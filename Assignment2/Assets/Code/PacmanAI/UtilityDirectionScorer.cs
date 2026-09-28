// TODO: your name <thand556@gmail.com>
// Utility function: given the legal directions at the current nav node and the
// goal chosen by the Behavior Tree, score every candidate direction and return
// the best one. This is the only layer that actually decides which way to move.
using System.Collections.Generic;
using UnityEngine;

public static class UtilityDirectionScorer {
    // How much reaching/avoiding the BT's target matters.
    private const float GoalWeight = 1f;
    // How strongly nearby non-frightened ghosts push Pacman away from a tile.
    private const float ThreatWeight = 4f;
    private const float ThreatRadius = 5f;
    // Small bonus for continuing straight so Pacman doesn't wobble pointlessly
    // between two equally-good directions.
    private const float MomentumWeight = 0.5f;

    public static Vector2Int ChooseDirection(Vector3 position, List<Vector2Int> directions, PacGoal goal, GhostNew[] ghosts, Vector2Int currentDirection) {
        // Same convention as GhostPhysics.RemoveReverseUnlessDeadEnd: never
        // immediately reverse unless it's the only option. Without this, a
        // single-step-lookahead utility function can get stuck oscillating
        // forever between two tiles once the pellets on both ends are eaten
        // (the "nearest pellet" goal keeps flipping back and forth with no
        // memory of the direction just taken).
        List<Vector2Int> candidates = RemoveReverseUnlessDeadEnd(directions, currentDirection);

        Vector2Int best = candidates[0];
        float bestScore = float.MinValue;

        foreach(Vector2Int dir in candidates) {
            Vector3 newPos = position + new Vector3(dir.x, dir.y, 0);
            float score = Score(newPos, goal, ghosts, dir, currentDirection);
            if(score > bestScore) {
                bestScore = score;
                best = dir;
            }
        }
        return best;
    }

    private static List<Vector2Int> RemoveReverseUnlessDeadEnd(List<Vector2Int> directions, Vector2Int currentDirection) {
        if(currentDirection == Vector2Int.zero) {
            return directions;
        }
        Vector2Int reverse = -currentDirection;
        List<Vector2Int> filtered = new List<Vector2Int>();
        foreach(Vector2Int dir in directions) {
            if(dir != reverse) {
                filtered.Add(dir);
            }
        }
        return filtered.Count > 0 ? filtered : directions;
    }

    private static float Score(Vector3 newPos, PacGoal goal, GhostNew[] ghosts, Vector2Int dir, Vector2Int currentDirection) {
        float distToGoal = Vector3.Distance(newPos, goal.target);
        // Fleeing wants distance to the goal (the danger) to be as large as possible;
        // every other intent wants it as small as possible.
        float goalTerm = goal.intent == PacGoalIntent.Flee ? distToGoal : -distToGoal;

        float threatPenalty = 0f;
        if(ghosts != null) {
            foreach(GhostNew ghost in ghosts) {
                if(ghost == null || !ghost.gameObject.activeInHierarchy) {
                    continue;
                }
                if(ghost.State == GhostState.Frightened || ghost.State == GhostState.Home) {
                    continue;
                }
                float d = Vector3.Distance(newPos, ghost.transform.position);
                if(d < ThreatRadius) {
                    threatPenalty += ThreatRadius - d;
                }
            }
        }

        float momentumTerm = dir == currentDirection ? 1f : 0f;

        return GoalWeight * goalTerm - ThreatWeight * threatPenalty + MomentumWeight * momentumTerm;
    }
}
