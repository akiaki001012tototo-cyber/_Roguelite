using Core.Interface;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


namespace TPSRoguelite.InGame.Player
{
    public class PlieaControra : MonoBehaviour
    {

        private const float MOVE_SPEED = 5.0f;  //移動速度

        [SerializeField] private Rigidbody rigidbody;

        private Vector3 movDirection = Vector3.zero;//移動方向のベクトル
                                                    //外部(アニメなど)に現在の速度をを教えるため保持するVelocity
        public Vector3 CurrenVelocity { get; private set; }

        private PleyrInputActions inputActions;

        private Vector2 movInput = Vector2.zero;//入力方向

        // 回転速度
        private const float ROTATION_SPEED = 10.0f;

        /// カメラのトランスフォーム
        private Transform mainCameraTransform;

        // レーザーポインターの描画距離
        private const float LASER_MAX_DISTANCE = 50.0f;

        //相手に与えるダメージ量
        private const int ATTACK_DAMAGE = 20;

        // 攻撃距離（射撃範囲）
        private const float ATTACK_RANGE = 50f;

        //最大弾数
        private const int MAX_AMMO = 30;
       
        //リロード時間
        private const float RELOAD_TIME = 1.5f;

        // (既存のメンバ変数は省略)
        private bool isReloading;

        //現在の弾の数
        public int CurrenAmmo { get; private set; }

        // 銃口の位置
        [SerializeField] private Transform weaponOrigin;


        // レーザーポインターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;



        private void Awake()
        {
            CurrenAmmo=MAX_AMMO;

            inputActions=new PleyrInputActions();
            inputActions.Player.Fire.performed+=Fire;

            inputActions.Player.Reload.performed+=OnReload;


            if (UnityEngine.Camera.main!=null)
            {
                mainCameraTransform  = UnityEngine.Camera.main.transform;


            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
        // Update is called once per frame
        void Update()
        {
            //float x = Input.GetAxisRaw("Horizontal");
            //float z = Input.GetAxisRaw("Vertical");

            ////入力自値から移動方向のベクトル
            //movDirection=new Vector3(x,0,z).normalized;

            movInput=inputActions.Player.Move.ReadValue<Vector2>();

            DrawLaserPointer();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            if (rigidbody==null)
            {
                Debug.LogError("Rigidbodyが設置されていいません");


                return;
            }
            if (movInput==Vector2.zero)
            {
                rigidbody.linearVelocity=new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrenVelocity=Vector3.zero;
                return;
            }
            ////移動速度計算
            //Vector3 targetVelocity = new Vector3(movInput.x, rigidbody.linearVelocity.y, movInput.y);
            //targetVelocity.Normalize();
            //rigidbody.linearVelocity=targetVelocity*MOVE_SPEED;

            // カメラ基準の計算に変更
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            // 空や地面に向かって移動しないよう、Y軸を水平に補正
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward*movInput.y+cameraRight*movInput.x);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation=Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATION_SPEED*Time.fixedDeltaTime);

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity= new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);


            //外部(アニメーションやUIなど)に現在の速度を教えるためにプロパティを更新

        }

        void Fire(InputAction.CallbackContext context)

        {
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線が何かに当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中");

                // 当たった相手が IDamageable (ダメージを受けられる性質) を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                if (target != null)
                {
                    //ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                    target.TakeDamage(ATTACK_DAMAGE);
                }


            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading&&CurrenAmmo==MAX_AMMO)
            {
                return;
            }

            ReloadAsync().Forget();             
        }


        async UniTask ReloadAsync()
        { 
          isReloading = true;
            Debug.Log("リロード中");
             await UniTask.Delay (System.TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrenAmmo=MAX_AMMO;
            isReloading=false;
            Debug.Log("リロード完了");
        }


        private void DrawLaserPointer()
        {
            if (laserLineRenderer==null|| weaponOrigin==null||mainCameraTransform==null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weaponOrigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }

            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
