// TODO: your name <thand556@gmail.com>
// Orchestrator that ties the three AI techniques together for Pacman:
// FSM (PacFsm) picks the mode -> Behavior Tree (PacmanBehaviorTree) picks a goal
// for that mode -> Utility function (UtilityDirectionScorer) picks the direction.
// HandleNavigation mirrors GhostPhysics.HandleNavigation: it is called by
// Pellet.OnTriggerEnter2D every time Pacman reaches a nav node (intersection).
using System.Collections.Generic;
using UnityEngine;

public class PacmanAI : MonoBehaviour {
    public Movement movement;

    [Tooltip("Toggle with P at runtime. When off, Pacman.HandlePacmanInput() reads the keyboard instead.")]
    public bool aiControlled = false;

    private readonly PacFsm fsm = new PacFsm();
    private readonly PacmanBehaviorTree behaviorTree = new PacmanBehaviorTree();

    public PacAIState CurrentState => fsm.state;

    private void Awake() {
        movement = GetComponent<Movement>();
    }

    // Tracks the tile the stuck-recovery below last acted on, so it decides
    // once per tile rather than every single frame it's blocked.
    private Vector3 lastRecoveryTile = new Vector3(float.NaN, float.NaN, float.NaN);

    private void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            aiControlled = !aiControlled;
            Debug.Log("Pacman AI control: " + (aiControlled ? "ON" : "OFF (manual)"));
        }

        // HandleNavigation only fires when Pacman's collider triggers a
        // scripted nav-node pellet. If a reset/respawn (new round, level
        // transition, death) lands Pacman with its default direction blocked
        // by a wall before it reaches any nav node, nothing else would ever
        // prompt a new decision and it would sit stuck indefinitely. Detect
        // that case here and force a fresh decision from the current tile.
        if(aiControlled && movement.currentDirection != Vector2Int.zero && !movement.CanMove(movement.currentDirection)) {
            Vector3 tileCenter = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
            // HandleNavigation snaps to tileCenter every call. CanMove can
            // flicker true/false near a tile boundary as Pacman edges
            // forward, so without this guard, re-deciding (and re-snapping)
            // every single frame at the SAME tile repeatedly resets that
            // forward progress right back to the tile center, even when the
            // decision itself doesn't change - it never actually escapes.
            if(tileCenter != lastRecoveryTile) {
                lastRecoveryTile = tileCenter;
                List<Vector2Int> directions = Utils.FindAvailableDirections(tileCenter, movement.obsLayerMask);
                HandleNavigation(tileCenter, directions);
            }
        }
    }

    public void HandleNavigation(Vector3 tileCenter, List<Vector2Int> directions) {
        if(!aiControlled || directions == null || directions.Count == 0) {
            return;
        }

        transform.position = tileCenter; // snap to the node, same as GhostPhysics.HandleNavigation

        PacContext ctx = BuildContext(tileCenter);
        PacAIState state = fsm.Tick(ctx);
        PacGoal goal = behaviorTree.Evaluate(state, ctx);
        Vector2Int direction = UtilityDirectionScorer.ChooseDirection(tileCenter, directions, goal, GameMgr.instance.ghosts, movement.currentDirection);

        movement.SetDirection(direction, forced: true);
    }

    private PacContext BuildContext(Vector3 pos) {
        PacContext ctx = new PacContext { position = pos };

        if(GameMgr.instance != null && GameMgr.instance.ghosts != null) {
            foreach(GhostNew ghost in GameMgr.instance.ghosts) {
                if(ghost == null || !ghost.gameObject.activeInHierarchy) {
                    continue;
                }
                float dist = Vector3.Distance(pos, ghost.transform.position);
                bool isDanger = ghost.State == GhostState.Chasing || ghost.State == GhostState.Scatter;
                bool isFrightened = ghost.State == GhostState.Frightened && !ghost.isEaten;

                if(isDanger && dist < ctx.nearestDangerDist) {
                    ctx.nearestDangerDist = dist;
                    ctx.nearestDangerGhost = ghost;
                }
                if(isFrightened && dist < ctx.nearestFrightenedDist) {
                    ctx.nearestFrightenedDist = dist;
                    ctx.nearestFrightenedGhost = ghost;
                }
            }
        }

        if(GridMgr.instance != null && GridMgr.instance.pellets != null) {
            foreach(Pellet pellet in GridMgr.instance.pellets) {
                if(pellet == null || pellet.isEaten) {
                    continue;
                }
                float dist = Vector3.Distance(pos, pellet.transform.position);
                if(pellet is PowerPellet powerPellet) {
                    if(dist < ctx.nearestPowerPelletDist) {
                        ctx.nearestPowerPelletDist = dist;
                        ctx.nearestPowerPellet = powerPellet;
                    }
                } else if(dist < ctx.nearestPelletDist) {
                    ctx.nearestPelletDist = dist;
                    ctx.nearestPellet = pellet;
                }
            }
        }

        return ctx;
    }
}
