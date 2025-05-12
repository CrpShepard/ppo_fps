using UnityEngine;

public interface ITarget
{
    byte team { get; set; }
    bool IsEnemy(ITarget target);
    int score { get; set; }
    int death { get; }
    void AddScore();
    void TakeDamage(float damage, ITarget source);
    bool isDead { get; }
}
