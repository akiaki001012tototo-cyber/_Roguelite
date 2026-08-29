using Core.MasterData;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Core.MasterData
{
    [Serializable]

    public class SkillDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string SkillName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int SkillType { get; private set; }
        [field: SerializeField] public float Value { get; private set; }
     }


  [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData")]
public class SkillData : ScriptableObject, IMasterDataContainer<SkillDataRecord>
{
    [field: SerializeField] public List<SkillDataRecord> Records { get; private set; } = new List<SkillDataRecord>();
}

}

