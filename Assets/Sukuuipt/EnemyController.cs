using UnityEngine;
using Unity.AI;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const string PLYER_TAG_NAME = "Player";

        //敵の本体
        [SerializeField] private EnemyState enemyState=null;

        [SerializeField] NavMeshAgent navMeshAgent =null;
        
        Transform targetPlayer=null;
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
    }
}