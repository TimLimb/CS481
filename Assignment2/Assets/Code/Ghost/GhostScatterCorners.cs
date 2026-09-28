// TODO: your name <thand556@gmail.com>
// Computes the 4 classic "scatter corner" targets from the maze currently loaded
// in GridMgr (a bit outside the pellet field, in each corner direction), so
// ghosts can retreat to a real corner during Scatter instead of wandering randomly.
// Works for whatever maze is currently loaded (level 1 or level 2) since it reads
// live pellet positions instead of hardcoded coordinates.
using System.Collections.Generic;
using UnityEngine;

public static class GhostScatterCorners {
    public enum Corner {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    private const float Margin = 2f;

    // Blinky/Pinky patrol the top corners, Inky/Clyde the bottom ones, matching arcade Pac-Man.
    public static Vector3 GetCornerFor(GameObject ghost) {
        if(ghost.name.Contains("Blinky")) {
            return Get(Corner.TopRight);
        }
        if(ghost.name.Contains("Pinky")) {
            return Get(Corner.TopLeft);
        }
        if(ghost.name.Contains("Inky")) {
            return Get(Corner.BottomRight);
        }
        if(ghost.name.Contains("Clyde")) {
            return Get(Corner.BottomLeft);
        }
        return Get(Corner.TopRight);
    }

    public static Vector3 Get(Corner corner) {
        Bounds bounds = ComputeMazeBounds();
        switch(corner) {
            case Corner.TopLeft:
                return new Vector3(bounds.min.x - Margin, bounds.max.y + Margin, 0);
            case Corner.TopRight:
                return new Vector3(bounds.max.x + Margin, bounds.max.y + Margin, 0);
            case Corner.BottomLeft:
                return new Vector3(bounds.min.x - Margin, bounds.min.y - Margin, 0);
            default:
                return new Vector3(bounds.max.x + Margin, bounds.min.y - Margin, 0);
        }
    }

    private static Bounds ComputeMazeBounds() {
        List<Pellet> pellets = GridMgr.instance != null ? GridMgr.instance.pellets : null;
        if(pellets == null || pellets.Count == 0) {
            return new Bounds(Vector3.zero, Vector3.zero);
        }
        Bounds bounds = new Bounds(pellets[0].transform.position, Vector3.zero);
        foreach(Pellet pellet in pellets) {
            if(pellet != null) {
                bounds.Encapsulate(pellet.transform.position);
            }
        }
        return bounds;
    }
}
