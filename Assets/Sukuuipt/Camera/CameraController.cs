using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour


    {//マウス感度
        private float LOOK_SENSITIVITY = 0.2f;

        //プレイヤーからの距離
        private float DISTANCE = 6f;

        //プレイヤーからの高さ
        private float HEIGHT_OFFSET = 1.5f;

        //縦の最小角度
        private const float MIN_PITCH = -10f;

        // 縦の最大角度
        private const float MAX_PITCH = 60f;

        // 追従するターゲット
        [SerializeField] private Transform target;

        // 自動生成されたクラス
        private PleyrInputActions inputActions;

        //マウスの移動量
        private Vector2 lookInput = Vector2.zero;

        //横の回転角度（Y軸回転）
        private float currentYaw = 0f;

        //縦の回転角度（Y軸回転）
        private float currentPitch = 20f;

        private void Awake()
        {
            inputActions = new PleyrInputActions();
            //マウスカーソルを画面中央にロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            
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
            //マウスの移動量を取得
            lookInput=inputActions.Player.Look.ReadValue<Vector2>();

            //感度をかけて現在の角度に足し引きする
            currentYaw += lookInput.x*LOOK_SENSITIVITY;
            currentPitch -= lookInput.y*LOOK_SENSITIVITY;

            currentPitch=Mathf.Clamp(currentPitch,MIN_PITCH, MAX_PITCH);


        }

        private void LateUpdate()
        {
            //カメラの移動は、プレイヤーの移動が終わった後に行う

            // ターゲットが設定されていない場合はエラーを回避
            if (target==null)
            {
                return;

            }

            // 注視点を計算（プレイヤーのこしあたり）
            Vector3 targetPosition = target.position + Vector3.up * HEIGHT_OFFSET;

            //角度をQuaternionに変換
            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw,0f);

            // 注視点から、計算した角度の後ろ方向へ距離分だけ離した位置を計算
            Vector3 cameraPosition = targetPosition-(rotate*Vector3.forward*DISTANCE);

            // カメラの位置と回転を確定
            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}

