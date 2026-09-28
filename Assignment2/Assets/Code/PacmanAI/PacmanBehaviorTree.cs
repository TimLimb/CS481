// TODO: your name <thand556@gmail.com>
// Behavior Tree: given the FSM's current mode, decide WHAT Pacman should aim
// for right now (a "goal": an intent + a target position). This layer composes
// conditions and fallback behaviors; it does not pick a movement direction —
// that is the Utility function's job (UtilityDirectionScorer.cs).
using UnityEngine;

public enum PacGoalIntent {
    Idle,
    SeekPellet,
    SeekPowerPellet,
    Flee,
    HuntGhost,
}

public struct PacGoal {
    public PacGoalIntent intent;
    public Vector3 target;
}

public class PacmanBehaviorTree {
    // How close a threat has to be, even while just Exploring, to make grabbing
    // a power pellet worth prioritizing over the nearest plain pellet.
    public float powerPelletThreatRadius = 10f;

    private readonly BTNode root;

    // Ticked closures read/write these two fields; Evaluate() sets them before
    // ticking the tree and reads back whatever goal the tree produced.
    private PacAIState currentState;
    private PacContext currentCtx;
    private PacGoal goal;

    public PacmanBehaviorTree() {
        root = new BTSelector(
            // Danger nearby -> run from it.
            new BTSequence(
                new BTCondition(() => currentState == PacAIState.Evade && currentCtx.nearestDangerGhost != null),
                new BTAction(() => {
                    goal = new PacGoal { intent = PacGoalIntent.Flee, target = currentCtx.nearestDangerGhost.transform.position };
                    return BTStatus.Success;
                })
            ),
            // A frightened ghost is worth eating -> go get it.
            new BTSequence(
                new BTCondition(() => currentState == PacAIState.Hunt && currentCtx.nearestFrightenedGhost != null),
                new BTAction(() => {
                    goal = new PacGoal { intent = PacGoalIntent.HuntGhost, target = currentCtx.nearestFrightenedGhost.transform.position };
                    return BTStatus.Success;
                })
            ),
            // Exploring but a ghost is close enough that a power pellet would help soon.
            new BTSequence(
                new BTCondition(() => currentCtx.nearestPowerPellet != null
                    && currentCtx.nearestDangerGhost != null
                    && currentCtx.nearestDangerDist <= powerPelletThreatRadius),
                new BTAction(() => {
                    goal = new PacGoal { intent = PacGoalIntent.SeekPowerPellet, target = currentCtx.nearestPowerPellet.transform.position };
                    return BTStatus.Success;
                })
            ),
            // Default: go eat the nearest pellet.
            new BTSequence(
                new BTCondition(() => currentCtx.nearestPellet != null),
                new BTAction(() => {
                    goal = new PacGoal { intent = PacGoalIntent.SeekPellet, target = currentCtx.nearestPellet.transform.position };
                    return BTStatus.Success;
                })
            ),
            // Nothing left to do (e.g. no pellets found this tick) -> stay put.
            new BTAction(() => {
                goal = new PacGoal { intent = PacGoalIntent.Idle, target = currentCtx.position };
                return BTStatus.Success;
            })
        );
    }

    public PacGoal Evaluate(PacAIState state, PacContext ctx) {
        currentState = state;
        currentCtx = ctx;
        root.Tick();
        return goal;
    }
}
