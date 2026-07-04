using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    //–¼‘O
    [field: SerializeField] public string EnemyNeme { get; private set; }

    //‘Ì—Í
    [field: SerializeField] public int MaxHP { get; private set; }

    //ˆÚ“®‘¬“x
    [field: SerializeField] public float Movespeed { get; private set; }
}
