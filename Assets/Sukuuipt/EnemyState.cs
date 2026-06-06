using UnityEngine;
using Core.Interface;
namespace TPSRoguelite
{
    public class EnemyState : MonoBehaviour,IDamageable
    {
        // 体力の最大値
        private const int MAX_HP = 100;
        //現在の体力
        public int CurrentHp { get; private set; }

        private void Awake()
        {
            CurrentHp = MAX_HP;
        }

        public void TakeDamage(int damageAmount)
        {
            //マイナスのダメージ(回復)を防ぐ
            if (damageAmount<=0)
            {
                return;
            }

            CurrentHp -= damageAmount;
            Debug.Log($"敵に {damageAmount} のダメージ！ 残りHP: {CurrentHp}");

            if (CurrentHp <= 0)
            {
                Die();
            }

            void Die()
                {
                Debug.Log("敵を倒しました");
                Destroy(gameObject);
            }
        }
    }
}