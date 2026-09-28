// TODO: your name <thand556@gmail.com>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CmdChaser : GhostCommand {

    // How close Clyde needs to be to Pacman before he gets shy and retreats to his corner.
    private const float ClydeShyDistance = 8f;

    public CmdChaser(GhostNew ghost) : base(ghost) {

    }

    public CmdChaser(GhostNew ghost, float duration) : base(ghost, duration) {
        //this.duration = duration;
    }

    public override void Init() {
        base.Init();
        SfxMgr.instance.Play(SfxType.GhostChase);
    }

    public override void Tick(float dt) {
        base.Tick(dt);



    }

    public override Vector2Int GetNewDirection(Vector3 pos, List<Vector2Int> directions) {

        if(!IsDone()) {
            Vector3 target = GetChaseTarget(pos);
            return GetDirectionTowards(target, directions, pos);
        } else {
            return Vector2Int.zero;
        }
    }

    // Classic Pac-Man ghost personalities: each ghost aims at a different point
    // relative to Pacman instead of all four dog-piling on his exact tile.
    private Vector3 GetChaseTarget(Vector3 pos) {
        Transform pacman = ghost.target;
        string name = ghost.gameObject.name;

        if(name.Contains("Pinky")) {
            // Ambush 4 tiles ahead of Pacman's current heading.
            Vector2Int dir = GameMgr.instance.pacman.movement.currentDirection;
            return pacman.position + new Vector3(dir.x, dir.y, 0) * 4f;
        }

        if(name.Contains("Inky")) {
            // Reflect a point 2 tiles ahead of Pacman through Blinky's position.
            GhostNew blinky = FindGhostByName("Blinky");
            Vector2Int dir = GameMgr.instance.pacman.movement.currentDirection;
            Vector3 pivot = pacman.position + new Vector3(dir.x, dir.y, 0) * 2f;
            if(blinky != null) {
                return pivot + (pivot - blinky.transform.position);
            }
            return pivot;
        }

        if(name.Contains("Clyde")) {
            // Shy: chase directly while far, retreat to his own corner once close.
            float distSq = (pos - pacman.position).sqrMagnitude;
            if(distSq > ClydeShyDistance * ClydeShyDistance) {
                return pacman.position;
            }
            return GhostScatterCorners.GetCornerFor(ghost.gameObject);
        }

        // Blinky (and any unnamed ghost) chases Pacman directly.
        return pacman.position;
    }

    private GhostNew FindGhostByName(string namePart) {
        if(GameMgr.instance == null || GameMgr.instance.ghosts == null) {
            return null;
        }
        foreach(GhostNew g in GameMgr.instance.ghosts) {
            if(g != null && g.gameObject.name.Contains(namePart)) {
                return g;
            }
        }
        return null;
    }

}
