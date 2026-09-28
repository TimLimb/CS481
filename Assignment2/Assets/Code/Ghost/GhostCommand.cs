// TODO: your name <thand556@gmail.com>
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostCommand
{
    [SerializeField]
    protected float duration = 2;
    [SerializeField]
    private float timeRemaining = 0;
    public GhostNew ghost;
    public bool stop = false;

    public GhostCommand(GhostNew ghost) {
        this.ghost = ghost;
    }

    public GhostCommand(GhostNew ghost, float duration) {
        this.ghost = ghost;
        this.duration = duration;
    }

    public virtual void Init() {
        this.timeRemaining = this.duration;
        stop = false;
    }

    public virtual void Tick(float dt) {
        this.timeRemaining -= dt;
    }

    public virtual void Stop() {
        stop = true;
    }

    public virtual bool IsDone() {
        return this.timeRemaining <= 0 || stop;
    }

    public virtual Vector2Int GetNewDirection(Vector3 newDir, List<Vector2Int> directions) {
        return Vector2Int.zero;
    }

    // Shared by CmdChaser/CmdScatter: pick the legal direction that ends up closest to target.
    protected Vector2Int GetDirectionTowards(Vector3 target, List<Vector2Int> directions, Vector3 position) {
        Vector2Int best = Vector2Int.zero;
        float bestDistanceSq = float.MaxValue;
        Vector3 direction = Vector3.zero;
        foreach(Vector2Int dir in directions) {
            direction.x = dir.x;
            direction.y = dir.y;
            Vector3 newPos = position + direction;
            float distanceSq = (newPos - target).sqrMagnitude;
            if(distanceSq < bestDistanceSq) {
                bestDistanceSq = distanceSq;
                best = dir;
            }
        }
        return best;
    }

    // Used by CmdFrightened: pick the legal direction that ends up farthest from target.
    protected Vector2Int GetDirectionAway(Vector3 target, List<Vector2Int> directions, Vector3 position) {
        Vector2Int best = Vector2Int.zero;
        float bestDistanceSq = float.MinValue;
        Vector3 direction = Vector3.zero;
        foreach(Vector2Int dir in directions) {
            direction.x = dir.x;
            direction.y = dir.y;
            Vector3 newPos = position + direction;
            float distanceSq = (newPos - target).sqrMagnitude;
            if(distanceSq > bestDistanceSq) {
                bestDistanceSq = distanceSq;
                best = dir;
            }
        }
        return best;
    }

}
