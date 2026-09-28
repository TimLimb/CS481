// TODO: your name <thand556@gmail.com>
using System.Collections.Generic;
using UnityEngine;

public class CmdFrightened : GhostCommand
{

    public CmdFrightened(GhostNew ghost) : base(ghost) {

    }

    public CmdFrightened(GhostNew ghost, float duration) : base(ghost, duration) {
        //this.duration = duration;
    }

    public override void Init() {
        base.Init();
    }

    public override void Tick(float dt) {
        base.Tick(dt);
        if(ghost.isEaten) {
            ghost.isEaten = false; //reset eaten state
            ghost.GoHome(); //teleport home
            ghost.SetFrightenedEatenAppearance();//just eyes showing
        }
    }

    public override Vector2Int GetNewDirection(Vector3 pos, List<Vector2Int> directions) {

        if(!IsDone()) {
            return GetDirectionAway(ghost.target.position, directions, pos);
        } else {
            return Vector2Int.zero;
        }
    }

}
