using UnityEngine;

namespace Core.MasterData
{
    //1行のデータIDを持つことを保証する
    public interface IMasterData
    {

        public ulong Id { get; }
    }

}
