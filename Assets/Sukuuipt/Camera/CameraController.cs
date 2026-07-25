using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour

    {
        // 追従するターゲット
        [SerializeField] private Transform target;

        [Header("カメラの基本設定")]

        //カメラの感度
        [SerializeField] private float lookSensitivity = 0.2f;

        //縦の最小角度
        [SerializeField] private float minPitch = 10f;

        //縦の最大角度
        [SerializeField] private float maxPitch = 60f;
        //ズーム速度
        [SerializeField] private float zoomSpeed = 0.5f;

        [Header("カメラの視点")]

        //後ろに下がる距離
        [SerializeField] private float targetDistance = 3.0f;

        //高さ
        [SerializeField] private float targetHeightOffset = 1.2f;

        //右にずらす距離
        [SerializeField] private float targetShouiderOffset = 0.8f;
        
        
        // 自動生成されたクラス
        private PleyrInputActions inputActions;

        //マウスの移動量
        private Vector2 lookInput = Vector2.zero;

        //横の回転角度（Y軸回転）
        private float currentYaw = 0f;

        //縦の回転角度（Y軸回転）
        private float currentPitch = 20f;

        private float currentDistance = 0f;
        private float currentHeightOffset = 0f;
        private float currentShouiderOffset = 0f;


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
            currentYaw += lookInput.x*lookSensitivity;
            currentPitch -= lookInput.y*lookSensitivity;

            currentPitch=Mathf.Clamp(currentPitch,minPitch, maxPitch);


        }

        private void LateUpdate()
        {
            //カメラの移動は、プレイヤーの移動が終わった後に行う

            // ターゲットが設定されていない場合はエラーを回避
            if (target==null)
            {
                return;

            }

            //現在の数値を、目標の数値に向かったら滑らかに変化させる(変化させる機能が「Mathf.Lerp」)
            currentDistance=Mathf.Lerp(currentDistance, targetDistance, zoomSpeed*Time.deltaTime);
            
            currentHeightOffset=Mathf.Lerp(currentShouiderOffset, targetHeightOffset, zoomSpeed*Time.deltaTime);
            
            currentShouiderOffset=Mathf.Lerp(currentShouiderOffset,targetShouiderOffset, zoomSpeed*Time.deltaTime);
            //カメラの回転を計算
            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw,0f);
            //注視点の計算(カメラのが見るとこ)
            Vector3 basPosition=target.position+Vector3.up*currentHeightOffset;
            // 肩越し視点にするため、カメラにとっての「右方向」へずらす
            Vector3 ShouiderPosition = basPosition+(rotate*Vector3.right*currentShouiderOffset);
            //そこから、カメラにとっての「後ろ方向」へ距離分だけ離す
            Vector3 cameraPosition = ShouiderPosition+(rotate*Vector3.forward*currentDistance);


            // カメラの位置と回転を確定
            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}

