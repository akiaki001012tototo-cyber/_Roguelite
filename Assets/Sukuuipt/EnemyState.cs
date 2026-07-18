using UnityEngine;
using Core.Interface;
using UnityEngine.Events;
using Core.MasterData;
namespace TPSRoguelite
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        //敵のデータ
       public EnemyDataRecord EnemyDataAsset { get; private set; }

        //現在の体力
        public int CurrentHp { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;



        public void Initalize(ulong id)
        {
            EnemyDataAsset=MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);
        }

        public void Setup()
        {
            if (EnemyDataAsset==null)
            {
                Debug.Log("EnemyDataがセットされていません");
                return;
            }

            // オブジェクトプールで再利用される時、表示された瞬間にHPを元に戻す
            CurrentHp = EnemyDataAsset.MaxHP;
            gameObject.SetActive(true);

        }
        public void TakeDamage(int damageAmount)
        {
            //マイナスのダメージ(回復)を防ぐ
            if (damageAmount<=0)
            {
                return;
            }

            CurrentHp -= damageAmount;
            Debug.Log($"{EnemyDataAsset} {damageAmount} のダメージ！ 残りHP: {CurrentHp}");

            if (CurrentHp <= 0)
            {
                Die();
            }

            void Die()
            {
                Debug.Log($"{EnemyDataAsset.EnemyNeme}を倒しました");
                // Destroy(gameObject);

                gameObject.SetActive(false);

                OnReturnToPoolAction?.Invoke(this);
            }
        }
    }
}