using UnityEngine;
using System;
using System.Collections.Generic;

namespace Core.MasterData
{
    [Serializable]
    public class EnemyDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }

        //–¼‘O
        [field: SerializeField] public string EnemyNeme { get; private set; }

        //‘Ì—Í
        [field: SerializeField] public int MaxHP { get; private set; }

        //ˆÚ“®‘¬“x
        [field: SerializeField] public float Movespeed { get; private set; }
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject, IMasterDataContainer<EnemyDataRecord>
    {
        [field: SerializeField]public List<EnemyDataRecord> Records { get; private set; }
    }
}
