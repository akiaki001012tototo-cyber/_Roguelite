using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private void Awake()
        {
            inputActions=new PleyrInputActions();
            inputActions.Player.Fire.performed+=Fire;

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
            //移動速度計算
            Vector3 targetVelocity = new Vector3(movInput.x, rigidbody.linearVelocity.y, movInput.y);
            targetVelocity.Normalize();
            rigidbody.linearVelocity=targetVelocity*MOVE_SPEED;

        }

        void Fire(InputAction.CallbackContext context)

        {
            Debug.Log("Fire");



        }
    }
}
