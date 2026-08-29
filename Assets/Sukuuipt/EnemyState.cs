using UnityEngine;
using Core.Interface;
using UnityEngine.Events;
using Core.MasterData;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace TPSRoguelite
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        //点滅時間
        private const float FLASH_DURATION = 0.1f;

        private const float ORB_DROP_HEIGHT_OFFSET = 0.5f;

        //キャラクターのレンダラー
        [SerializeField] private Renderer[] modelRenderers;

        [SerializeField] private GameObject experienceOrbPrefab;


        //キャラクターの元々の色
        private Color[] defaulfColors;
        //点滅するアニメーションのキャンセルトークン

        private CancellationTokenSource flashCts;

        //敵のデータ
        public EnemyDataRecord EnemyDataAsset { get; private set; }

        //現在の体力
        public int CurrentHp { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        public event UnityAction OnDamageAction;

        public void Initalize(ulong id)
        {
            EnemyDataAsset=MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (modelRenderers!=null)
            {
                defaulfColors=new Color[modelRenderers.Length];
                for (int i = 0; i<modelRenderers.Length; i++)
                {
                    if (modelRenderers[i]!=null)
                    {
                        defaulfColors[i]=modelRenderers[i].material.color;
                    }
                }

            }
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
            ResetColor();

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

            if (CurrentHp>0)
            {
                OnDamageAction?.Invoke();

                flashCts?.Cancel();
                flashCts?.Dispose();
                flashCts=null;

                flashCts=new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamgeFlashAsync(linkedCts.Token).Forget();
            }

            else
            {
                Die();
            }

            void Die()
            {
                if (experienceOrbPrefab != null)
                {
                    Vector3 spawnPosition = transform.position + Vector3.up * ORB_DROP_HEIGHT_OFFSET;
                    Instantiate(experienceOrbPrefab, spawnPosition, Quaternion.identity);
                }

                Debug.Log($"{EnemyDataAsset.EnemyNeme}を倒しました");
                // Destroy(gameObject);

                gameObject.SetActive(false);

                OnReturnToPoolAction?.Invoke(this);
            }


        }


        private void ResetColor()
        {

            {
                if (modelRenderers == null || defaulfColors == null)
                {
                    return;
                }

                for (int i = 0; i<modelRenderers.Length; i++)
                {
                    if (modelRenderers[i]!=null)
                    {
                        modelRenderers[i].material.color = defaulfColors[i];
                    }
                }
            }

        }

        private async UniTaskVoid DamgeFlashAsync(CancellationToken token)
        {
            foreach (var renderer in modelRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken: token).SuppressCancellationThrow();

            if (!isCanceled)
            {
                ResetColor();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            var player = collision.gameObject.GetComponent<IDamageable>();
            if (player != null && collision.gameObject.CompareTag("Player"))
            {
                player.TakeDamage(10);
            }
        }
    }

}
    
