// TODO: your name <thand556@gmail.com>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdScatter : GhostCommand
{
    public CmdScatter(GhostNew ghost) : base(ghost) {
        this.duration = 4;
    }

    public CmdScatter(GhostNew ghost, float duration) : base(ghost, duration) {
        this.duration = duration;
    }

    public override void Init() {
        base.Init();
    }

    public override void Tick(float dt) {
        base.Tick(dt);

    }



    public override Vector2Int GetNewDirection(Vector3 pos, List<Vector2Int> directions) {

        if(!IsDone()) {
            if(directions.Count == 0) {
                Debug.Log(ghost.name + ": no directions available @ " + pos);
                return Vector2Int.zero;
            } else {
                // Head toward this ghost's own scatter corner instead of wandering randomly.
                Vector3 corner = GhostScatterCorners.GetCornerFor(ghost.gameObject);
                return GetDirectionTowards(corner, directions, pos);
            }
        } else {
            return Vector2Int.zero;
        }

    }

}
