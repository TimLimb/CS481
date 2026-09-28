// TODO: your name <thand556@gmail.com>
using UnityEngine;

public enum PacmanState {
    Idle,
    Moving,
    Dead,
    Chasing,
}

public class Pacman : MonoBehaviour {
    public SpriteAnimator deathSequence;
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D circleCollider;
    public Movement movement;
    public PacmanAI ai;

    private Vector3 rotationEulerAngle = Vector3.zero;
    public Vector3 startPosition = Vector3.zero;

    public PacmanState state = PacmanState.Idle;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        movement = GetComponent<Movement>();
        ai = GetComponent<PacmanAI>();
        state = PacmanState.Idle;
        startPosition = transform.position;
    }

    private void Update() {
        HandlePacmanInput();
    }

    void HandlePacmanInput() {
        // AI drives direction changes via PacmanAI.HandleNavigation when aiControlled is on;
        // manual keyboard control is only used as a fallback (toggle with P) for testing/demo.
        if(ai != null && ai.aiControlled) {
            state = movement.currentDirection == Vector2Int.zero ? PacmanState.Idle : PacmanState.Moving;
        } else {
            HandleKeyboardInput();
        }
        float angle = Mathf.Atan2(movement.currentDirection.y, movement.currentDirection.x) * Mathf.Rad2Deg;
        rotationEulerAngle.z = angle;
        transform.localEulerAngles = rotationEulerAngle; // Rotate the object to face the direction of movement
    }

    void HandleKeyboardInput() {
        // Handle input for direction change
        state = PacmanState.Moving;
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            movement.SetDirection(new Vector2Int(0, 1)); // Up
        } else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            movement.SetDirection(new Vector2Int(0, -1)); // Down
        } else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            movement.SetDirection(new Vector2Int(-1, 0)); // Left
        } else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            movement.SetDirection(new Vector2Int(1, 0)); // Right
        } else if(Input.GetKeyDown(KeyCode.Space)) {
            // Handle space key for some action
            movement.currentDirection = Vector2Int.zero; // Stop moving
            state = PacmanState.Idle;
        }
    }

    public void ResetState() {
        transform.position = FindValidSpawnPosition(startPosition);
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        //deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
        state = PacmanState.Idle;
    }

    // startPosition is a fixed world coordinate captured once at Awake(), but
    // different mazes (e.g. level 2) aren't guaranteed to have an open tile
    // there - it can land inside a wall. Pellets only exist on walkable tiles
    // in whichever maze is currently loaded, so snapping to the nearest one
    // guarantees a valid spawn regardless of maze layout.
    private Vector3 FindValidSpawnPosition(Vector3 nominal) {
        if(GridMgr.instance == null || GridMgr.instance.pellets == null || GridMgr.instance.pellets.Count == 0) {
            return nominal;
        }
        Vector3 best = nominal;
        float bestDist = float.MaxValue;
        foreach(Pellet pellet in GridMgr.instance.pellets) {
            if(pellet == null) {
                continue;
            }
            float dist = Vector3.Distance(pellet.transform.position, nominal);
            if(dist < bestDist) {
                bestDist = dist;
                best = pellet.transform.position;
            }
        }
        return best;
    }

    public void DeathSequence() {
        spriteRenderer.enabled = false;
        circleCollider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
        state = PacmanState.Dead;
    }

}