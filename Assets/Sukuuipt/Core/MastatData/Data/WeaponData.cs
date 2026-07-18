using System;
using System.Collections.Generic;
using UnityEngine;
namespace Core.MasterData
{
    [Serializable]
    public class WeaponDataRecord : IMasterData
    {

        [field: SerializeField] public ulong Id { get; private set; }

       
        // 武器の名前
        [field: SerializeField] public string WeaponName { get; private set; }

        // 連射タイプ
        [field: SerializeField] public int  WeaponFireType { get; private set; }

       
        // 攻撃力
        [field: SerializeField] public int AttackPower { get; private set; }

       // フルオートやバースト時の連射間隔
        [field: SerializeField] public float FireInterval { get; private set; }

        // 次の球が撃てるまでの待機時間
        /// </summary>
        [field: SerializeField] public float FireRate { get; private set; }

        // マガジンの最大弾数
        [field: SerializeField] public int MaxAmmo { get; private set; }

        // リロードにかかる時間
        [field: SerializeField] public float ReloadTime { get; private set; }
    }

    

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/WeaponData")]
    public class WeaponData : ScriptableObject, IMasterDataContainer<WeaponDataRecord>
    {
        [field: SerializeField] public List<WeaponDataRecord> Records { get; private set; }
    }
}
    


