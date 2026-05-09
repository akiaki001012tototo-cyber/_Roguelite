using UnityEngine;

public class PlieaControra : MonoBehaviour
{

    private const float MOVE_SPEED = 5.0f;  //移動速度

    [SerializeField] private Rigidbody rigidbody;

    private Vector3 movDirection = Vector3.zero;//移動方向のベクトル
   //外部(アニメなど)に現在の速度をを教えるため保持するVelocity
    public Vector3 CurrenVelocity { get; private set; }


    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //入力自値から移動方向のベクトル
        movDirection=new Vector3(x,0,z).normalized;
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

            rigidbody.linearVelocity=new Vector3(0f,rigidbody.linearVelocity.y,0f);
            return;
        }

        //移動速度計算
        Vector3 targetVelocity=movDirection*MOVE_SPEED;

        rigidbody.linearVelocity=new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);
        CurrenVelocity=rigidbody.linearVelocity;

    }

}
