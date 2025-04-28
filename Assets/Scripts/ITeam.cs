using UnityEngine;

public interface ITeam
{
    byte team { get; set; }
    bool IsEnemy(ITeam target);
    int score { get; set; }
    int death { get; }
    void AddScore();
}
