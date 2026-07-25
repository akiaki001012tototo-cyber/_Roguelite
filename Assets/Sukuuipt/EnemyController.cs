using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const string PLYER_TAG_NAME = "Player";

        // ノックバックの強さ
        private const float KNOCKBACK_FORCE = 2.0f;

        // ノックバックの長さ
        private const float KNOCKBACK_DURATION = 0.15f;

        //敵の本体
        [SerializeField] private EnemyState enemyState = null;

        [SerializeField] NavMeshAgent navMeshAgent = null;

        Transform targetPlayer = null;

        // ノックバック動作のキャンセルトークン
        private CancellationTokenSource hitCts;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            //シーンからPlayerタグを探す
            GameObject Player = GameObject.FindGameObjectWithTag("Player");

            if (Player!=null)
            {
                targetPlayer=Player.transform;

            }
            else
            {
                Debug.LogError($"{PLYER_TAG_NAME}というタグが見つかりませんでした");
            }
            if (enemyState != null && enemyState.EnemyDataAsset != null)
            {
                navMeshAgent.speed = enemyState.EnemyDataAsset.Movespeed;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //ターゲットとナビメッシュが存在しいているか
            if (targetPlayer!=null&navMeshAgent!!=null)
            {
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }

        private void OnEnable()
        {
            if (enemyState!=null)
            {
                enemyState.OnDamageAction -= HandleDamage;
                enemyState.OnDamageAction += HandleDamage;
            }
        }


        private void OnDisable()
        {
            if (enemyState != null)
            {
                enemyState.OnDamageAction -= HandleDamage;
            }

            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = false;
            }
        }
    

        private async UniTaskVoid KnockbackAsync(CancellationToken token)
        {
            if (navMeshAgent == null)
            {
                return;
            }

            bool wasStopped = navMeshAgent.isStopped;
            navMeshAgent.isStopped=true;

            if (targetPlayer != null)
            {
                Vector3 dir = (transform.position - targetPlayer.position).normalized;

                dir.y = 0;
                transform.position += dir * KNOCKBACK_FORCE;
            }

            bool isCanceled = await UniTask.Delay(System.TimeSpan.FromSeconds(KNOCKBACK_DURATION)).SuppressCancellationThrow();

            if (isCanceled&&navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped=wasStopped;


            }
        }

        private void HandleDamage()
        {

            hitCts?.Cancel();
            hitCts?.Dispose();
            hitCts=null;

            hitCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(hitCts.Token, this.GetCancellationTokenOnDestroy());
            KnockbackAsync(linkedCts.Token).Forget();

        }
    }
}