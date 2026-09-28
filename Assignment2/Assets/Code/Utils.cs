using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{


    public static List<Vector2Int> FindAvailableDirections(Vector3 position, LayerMask layerMask) {
        List<Vector2Int> directions = new List<Vector2Int>();
        Vector2 size = new Vector2(0.4f, 0.4f);
        // Grid spacing is 1 unit. The cast distance adds to the box's own
        // half-size (0.2) to give the true reach, so a naive 1.1 actually
        // reaches ~1.3 total - close enough to a wall 2 tiles away to
        // occasionally clip it and wrongly report the immediate (1 tile
        // away) neighbor as blocked. This caused Pacman/ghost navigation to
        // misjudge short dead-end alcoves and oscillate forever between two
        // tiles. 0.9 gives a true reach of ~1.1: past the adjacent tile's
        // center, short of the next one. Matches Movement.CanMove's
        // reasoning for the same distance-vs-box-size effect.
        const float castDistance = 0.9f;

        Vector2Int pos2D = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

        RaycastHit2D hit = Physics2D.BoxCast(pos2D, size, 0, Vector2.up, castDistance, layerMask);
        if(hit.collider == null) {
            directions.Add(Vector2Int.up);
        }
        hit = Physics2D.BoxCast(pos2D, size, 0, Vector2.down, castDistance, layerMask);
        if(hit.collider == null) {
            directions.Add(Vector2Int.down);
        }
        hit = Physics2D.BoxCast(pos2D, size, 0, Vector2.left, castDistance, layerMask);
        if(hit.collider == null) {
            directions.Add(Vector2Int.left);
        }
        hit = Physics2D.BoxCast(pos2D, size, 0, Vector2.right, castDistance, layerMask);
        if(hit.collider == null) {
            directions.Add(Vector2Int.right);
        }
        return directions;
    }


}
