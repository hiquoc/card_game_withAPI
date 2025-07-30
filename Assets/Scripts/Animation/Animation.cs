using System;
using UnityEngine;

public class Animation
{
    public string animationId;
    public Vector3 source;
    public Vector3 target;
    public Action callback;
    public bool isAOE=false;
    public Animation(string animationId, Vector3 source, Vector3 target, Action callback, bool isAOE=false)
    {
        this.animationId = animationId;
        this.source = source;
        this.target = target;
        this.callback = callback;
        this.isAOE = isAOE;
    }
}
