using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;
using Core.MasterData;
namespace TPSRoguelite.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        //出現時間
        const float SPAWN_INTERVAL = 3.0f;

        //最初に用意する敵の数
        const int POOL_SIZE = 20;


        //出現範囲
        const float MAX_SPAWN_DISTANCE = 2.0f;

        //敵のプレハブ
        [SerializeField]
        GameObject enemyPrefb = null;

        // 出現ポイント
        [SerializeField] private Transform[] spawnPoints;

        //敵を待機させておくプール
        private Queue<EnemyState> enemyPool;


        


        private async UniTaskVoid SpawnLoopAsync()
        {
            //発生装置が破壊された時にタイマーを安全に止めるためのトークンを取得
            var token = this.GetCancellationTokenOnDestroy();

            //無限ループ
            while (true)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPAWN_INTERVAL));

                SpawnEnemyFromPool();
            }



        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
       

        public void Setup()
        {
            if (enemyPrefb==null)
            {
                return;
            }

            enemyPool = new Queue<EnemyState>();

            //開始時にあらかじめ用意した数だけ生成しておく
            for (int i = 0; i<POOL_SIZE; i++)
            {

                GameObject enemyObj = Instantiate(enemyPrefb);
                EnemyState enemy = enemyObj.GetComponent<EnemyState>();
                if (enemy!=null)
                {
                    ulong randomId = (ulong)UnityEngine.Random.Range(1, MasterDataAccessor.Instance.Count<EnemyDataRecord>());
                    enemy.Initalize(randomId);
                    enemy.gameObject.SetActive(false);
                    enemyPool.Enqueue(enemy);
                }


            }
            SpawnLoopAsync().Forget();

        }

        // 敵の生成
        void SpawnEnemyFromPool()
        {
            if (enemyPrefb==null||spawnPoints.Length==0)
            {
                return;
            }

            //ランダムな出現場所を決める
            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            Vector3 safePosition = spawnPoint.position;

            if (NavMesh.SamplePosition(safePosition, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                //見つかったら安全な座標に上書きする
                safePosition=hit.position;

            }
            else
            {
                //見つからなかったら
                Debug.LogWarning("近くに安全なスポーン位置が見つかりませんでした");
                return;
            }


            EnemyState enemy = null;

            if (enemyPool.Count>0)
            {
                enemy=enemyPool.Dequeue();

            }
            else
            {
                Debug.LogWarning("プールに空きがないためInstantiateで生成します。プールのサイズを増やすか、生成に制限をかけてください");

                GameObject enmyobj = Instantiate(enemyPrefb);
                enemy=enmyobj.GetComponent<EnemyState>();
                if (enemy==null)
                {
                    Debug.LogError("Eneystateの取得に失敗しました");
                    return;
                }


            }

            enemy.OnReturnToPoolAction+=ReturnToPool;
            enemy.transform.position = safePosition;
            enemy.transform.rotation = spawnPoint.rotation;

            enemy.Setup();

            
        }

        //プールに戻す
        private void ReturnToPool(EnemyState enemyState)
        {
            enemyPool.Enqueue(enemyState);
            enemyState.OnReturnToPoolAction -= ReturnToPool;
        }
    }


}
