using UnityEngine;
using Core.Interface;
using UnityEngine.Events;
namespace TPSRoguelite
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        //敵のデータ
        [field: SerializeField]public EnemyData EnemyDataAsset { get; private set; }

        //現在の体力
        public int CurrentHp { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;




        


        private void OnEnable()
        {
            if (EnemyDataAsset==null)
            {
                return;
            }

            // オブジェクトプールで再利用される時、表示された瞬間にHPを元に戻す
            CurrentHp = EnemyDataAsset.MaxHP;

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