using UnityEngine;
using TPSRoguelite.InGame.Player;
using Unity.VisualScripting;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        private const float MAGNET_RANGE = 5f;

        private const float MOVE_SPEED = 15f;

        private const string PLAYER_TAG = "Player";

        private Transform playerTarget;

        private bool isFollowing = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Playerが見つかりませんでした。");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playerTarget == null)
            {
                return;
            }

            if (isFollowing)
            {
                transform.position=Vector3.MoveTowards(transform.position, playerTarget.position, MOVE_SPEED * Time.deltaTime);
            }
            else
            {
                // プレイヤーとの距離を計算し、引き寄せ範囲内であれば引き寄せを開始する
                float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                if (distanceToPlayer <= MAGNET_RANGE)
                {
                    // プレイヤーに引き寄せられるようになる
                    isFollowing = true;
                }


            }
        }

        // プレイヤーに触れたときの処理（コライダーの Is Trigger がオンになっていいないと動かない）
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlieaControra  player = other.GetComponent<PlieaControra>();
                if (player != null)
                {
                    player.AddExp(1);
                }
                else
                {
                    Debug.LogWarning("PlieaControraが見つかりませんでした");
                }

                Destroy(gameObject);
            }
        }
    }
}

