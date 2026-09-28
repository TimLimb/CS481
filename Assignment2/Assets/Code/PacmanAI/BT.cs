// TODO: your name <thand556@gmail.com>
// Minimal generic Behavior Tree framework used by PacmanBehaviorTree.cs.
using System;
using System.Collections.Generic;

public enum BTStatus {
    Success,
    Failure,
    Running,
}

public abstract class BTNode {
    public abstract BTStatus Tick();
}

// Runs children in order, stops at the first that does not fail (Success or Running).
public class BTSelector : BTNode {
    private readonly List<BTNode> children;

    public BTSelector(params BTNode[] children) {
        this.children = new List<BTNode>(children);
    }

    public override BTStatus Tick() {
        foreach(BTNode child in children) {
            BTStatus status = child.Tick();
            if(status != BTStatus.Failure) {
                return status;
            }
        }
        return BTStatus.Failure;
    }
}

// Runs children in order, stops at the first that does not succeed (Failure or Running).
public class BTSequence : BTNode {
    private readonly List<BTNode> children;

    public BTSequence(params BTNode[] children) {
        this.children = new List<BTNode>(children);
    }

    public override BTStatus Tick() {
        foreach(BTNode child in children) {
            BTStatus status = child.Tick();
            if(status != BTStatus.Success) {
                return status;
            }
        }
        return BTStatus.Success;
    }
}

// Wraps a boolean check as a leaf: Success if true, Failure if false.
public class BTCondition : BTNode {
    private readonly Func<bool> condition;

    public BTCondition(Func<bool> condition) {
        this.condition = condition;
    }

    public override BTStatus Tick() {
        return condition() ? BTStatus.Success : BTStatus.Failure;
    }
}

// Wraps an action as a leaf. The action returns the status directly
// (Pacman's actions are instantaneous goal-setters, so they always return Success).
public class BTAction : BTNode {
    private readonly Func<BTStatus> action;

    public BTAction(Func<BTStatus> action) {
        this.action = action;
    }

    public override BTStatus Tick() {
        return action();
    }
}
