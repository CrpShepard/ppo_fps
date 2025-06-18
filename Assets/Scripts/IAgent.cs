using UnityEngine;

public interface IAgent : ITarget
{
    void _AddReward(float reward);

    float distanceToTarget {  get; set; }
}
