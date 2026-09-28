// TODO: your name <thand556@gmail.com>
// Finite State Machine: Pacman's high-level "mode". This is the macro layer —
// it only decides Explore/Evade/Hunt, it never picks a direction itself.
// The Behavior Tree (PacmanBehaviorTree.cs) reads this state to decide what to
// aim for, and the Utility function (UtilityDirectionScorer.cs) decides how to
// actually move towards that aim.
using UnityEngine;

public enum PacAIState {
    Explore, // default: eat pellets, stay away from danger
    Evade,   // a non-frightened ghost is dangerously close
    Hunt,    // a frightened ghost is close enough to be worth chasing for points
}

// Snapshot of the world built fresh each decision tick (see PacmanAI.BuildContext).
public class PacContext {
    public Vector3 position;
    public GhostNew nearestDangerGhost;
    public float nearestDangerDist = float.MaxValue;
    public GhostNew nearestFrightenedGhost;
    public float nearestFrightenedDist = float.MaxValue;
    public Pellet nearestPellet;
    public float nearestPelletDist = float.MaxValue;
    public PowerPellet nearestPowerPellet;
    public float nearestPowerPelletDist = float.MaxValue;
}

public class PacFsm {
    // Enter thresholds are tighter than exit thresholds (simple hysteresis)
    // so Pacman doesn't flicker between modes when a ghost sits right at the edge.
    public float dangerEnterDist = 5f;
    public float dangerExitDist = 7f;
    public float huntEnterDist = 6f;
    public float huntExitDist = 8f;

    public PacAIState state = PacAIState.Explore;

    public PacAIState Tick(PacContext ctx) {
        bool dangerNear = ctx.nearestDangerGhost != null &&
            ctx.nearestDangerDist <= (state == PacAIState.Evade ? dangerExitDist : dangerEnterDist);

        bool frightenedNear = ctx.nearestFrightenedGhost != null &&
            ctx.nearestFrightenedDist <= (state == PacAIState.Hunt ? huntExitDist : huntEnterDist);

        // Safety always wins over opportunism.
        if(dangerNear) {
            state = PacAIState.Evade;
        } else if(frightenedNear) {
            state = PacAIState.Hunt;
        } else {
            state = PacAIState.Explore;
        }
        return state;
    }
}
