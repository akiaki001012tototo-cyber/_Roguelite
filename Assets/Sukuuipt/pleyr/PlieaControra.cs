using Core.Interface;
using Core.MasterData;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using InGame.Enums;
using System;
using System.Threading;
using TMPro;
using TPSRoguelite.InGame.Enum;
using TPSRoguelite.InGame.Manager;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

        //武器のID
        [SerializeField] private ulong weponId = 1;

        //スマッシュのエフェクト
        [SerializeField] private ParticleSystem muzzleFlash;

        //リロード中のテキストと画像をまとめたオブジェクト
        [SerializeField] private GameObject rieioadUI;

        //リロード中の時間がかかるサークル画像
        [SerializeField] private Image rieioadImage;


        [SerializeField] private Slider expBar;

        [SerializeField] private TextMeshProUGUI levelUpText;

        [SerializeField] private ParticleSystem levelUpEffct;

        // 攻撃距離（射撃範囲）
        private const float ATTACK_RANGE = 50f;

        private const float LEVEL_UP_EFFECT_DURATION = 2f;

        // (既存のメンバ変数は省略)
        private bool isReloading;

        //現在の弾の数
        public int CurrenAmmo { get; private set; }

        public int CurrenExp { get; private set; }

        public int CurrentLevel { get; private set; }

        private int RequirdExp => CurrentLevel*5;

        private int FinalAttackPower => currentWeapon != null ? Mathf.RoundToInt(currentWeapon.AttackPower * (1f + attackPowerBuf)) : 0;

        private int FinalMaxAmmo => currentWeapon != null ? currentWeapon.MaxAmmo + maxAmmoBuff : 0;

        private float FinalReloadTime => currentWeapon != null ? currentWeapon.ReloadTime * Mathf.Max(0.1f, 1f - reloadSpeedBuff) : 0f;

        private float FinalFireRate => currentWeapon != null ? currentWeapon.FireRate * Mathf.Max(0.1f, 1f - fireRateBuff) : 0f;

        // 銃口の位置
        [SerializeField] private Transform weaponOrigin;


        // レーザーポインターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;
        //武器の名前
        [SerializeField] private TextMeshProUGUI weaponName;
        //弾のテキスト
        [SerializeField] private TextMeshProUGUI ammoText;


        // 武器のデータ 
        private WeaponDataRecord currentWeapon;

        // 射撃可能か
        private bool canShoot = true;


        //射撃のキャンセルトークン
        private CancellationTokenSource firCts;


        private float moveSpeedBuf = 0;
        private float attackPowerBuf = 0;
        private float fireRateBuff = 0f;
        private float reloadSpeedBuff = 0f;
        private int maxAmmoBuff = 0;


        private void Awake()
        {

            gameObject.SetActive(false);


        }

        public void Setup()
        {
            currentWeapon=MasterDataAccessor.Instance.GetById<WeaponDataRecord>(weponId);



            // ゲーム開始時に、マガジンに弾をフル装填する
            if (currentWeapon != null)
            {
                CurrenAmmo = currentWeapon.MaxAmmo;
                UpdateWeaponUI();
            }
            else
            {
                Debug.LogError("currentWeaponが見つかりませんでした");
            }


            moveSpeedBuf = 0;
            attackPowerBuf = 0;
            fireRateBuff = 0f;
            reloadSpeedBuff = 0f;
            maxAmmoBuff = 0;

            inputActions = new PleyrInputActions();
            inputActions.Player.Fire.started += OnFire;
            inputActions.Player.Fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;


            if (UnityEngine.Camera.main!=null)
            {
                mainCameraTransform  = UnityEngine.Camera.main.transform;


            }


            if (rieioadUI!=null)
            {
                rieioadUI.SetActive(false);
            }

            gameObject.SetActive(true);

            CurrenExp = 0;

            CurrentLevel = 1;

            if (levelUpText != null)
            {
                // レベルアップ時のテキストを非表示にする
                levelUpText.enabled = false;
            }


            UpdateExpUI();

        }


        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                // クールダウン中やリロード中（撃てない状態）なら、連打されても完全に無視する！
                if (!canShoot || isReloading || currentWeapon == null)
                {
                    return;
                }

                firCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(firCts.Token, this.GetCancellationTokenOnDestroy());

                switch ((FireType)currentWeapon.WeaponFireType)
                {


                    case FireType.SemiAuto:
                        ShootSemiAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        SootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case FireType.Burst:
                        SootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case FireType.FullAuto:
                        SootFullAsync(linkedCts.Token).Forget();
                        break;

                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります。{currentWeapon.WeaponFireType}");
                        break;

                }

                if (context.canceled)
                {
                    firCts?.Cancel();
                    firCts?.Dispose();
                    firCts=null;
                }
            }
        }

        // セミオートの射撃処理

        private async UniTaskVoid ShootSemiAutoAsync(CancellationToken token)
        {
            canShoot = false;

            if (CurrenAmmo <= 0)
            {
                Reload();
                return;
            }

            CurrenAmmo--;
            UpdateCurrentAmmoUI();
            Debug.Log($"バン！ 残弾: {CurrenAmmo}");
            Shoot();

            await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token);

            canShoot = true;
        }


        //バーストの射撃処理
        private async UniTaskVoid SootBurstAsync(CancellationToken token)
        {
            canShoot= false;

            for (int i = 0; i<3; i++)
            {
                if (CurrenAmmo<=0)
                {
                    Reload();
                    break;
                }

                CurrenAmmo--;
                UpdateCurrentAmmoUI();
                Shoot();
                Debug.Log($"バースト！ 残弾: {CurrenAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token);
                canShoot=true;
            }
        }


        private async UniTaskVoid SootFullAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (CurrenAmmo<=0)
                {
                    Reload();
                    break;
                }
                CurrenAmmo--;
                UpdateCurrentAmmoUI();
                Debug.Log($"フルオート！ 残弾: {CurrenAmmo}");
                Shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    break;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: this.GetCancellationTokenOnDestroy());
                canShoot=true;

            }

        }

        // 共通の射撃処理
        private void Shoot()
        {

            if (muzzleFlash!=null)
            {
                Debug.Log($"play");
                muzzleFlash.Play();
            }

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null)
                {
                    target.TakeDamage(FinalAttackPower);
                }
            }
        }







        private void OnEnable()
        {
            inputActions?.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
        }


        // Update is called once per frame
        void Update()
        {
            //float x = Input.GetAxisRaw("Horizontal");
            //float z = Input.GetAxisRaw("Vertical");

            //入力自値から移動方向のベクトル
            //movDirection=new Vector3(x,0,z).normalized;

            if (Time.timeScale==0f)
            {
                return;
            }

            if (inputActions == null||mainCameraTransform==null)
            {
                return;
            }



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
            // カメラの水平方向の前方を計算 (入力の有無に関わらず常に計算する)
            Vector3 cameraForward = mainCameraTransform.forward;
            cameraForward.y= 0f;
            cameraForward.Normalize();

            if (cameraForward!=Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATION_SPEED*Time.fixedDeltaTime);
            }

            movInput=inputActions.Player.Move.ReadValue<Vector2>();
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
            Vector3 cameraRight = mainCameraTransform.right;

            // 空や地面に向かって移動しないよう、Y軸を水平に補正
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward*movInput.y+cameraRight*movInput.x);

            float finalMovespeed = MOVE_SPEED*(1f+moveSpeedBuf);

            Vector3 targetVelocity = moveDirection * finalMovespeed;
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
                    target.TakeDamage(currentWeapon.AttackPower);
                }


            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading||CurrenAmmo==FinalMaxAmmo)
            {
                return;
            }

            Reload();
        }


        void Reload()
        {

            isReloading = true;

            if (rieioadUI!=null)
            {
                rieioadUI.SetActive(true);
            }
            if (rieioadImage)
            {
                rieioadImage.fillAmount=0f;
            }

            float finalReloadTime = currentWeapon!=null ? currentWeapon.ReloadTime*Mathf.Max(0.1f-reloadSpeedBuff) : 0f;

            DOVirtual.Float(0f, 1f, finalReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);


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
        void UpdateWeaponUI()
        {
            if (weaponName!=null)
            {
                weaponName.SetText(currentWeapon.WeaponName);

                //色で武器のタイプがわかる
                switch ((FireType)currentWeapon.WeaponFireType)
                {
                    case FireType.SemiAuto:
                        weaponName.color = Color.white;
                        break;

                    case FireType.Burst:
                        weaponName.color = Color.yellow;
                        break;

                    case FireType.FullAuto:
                        weaponName.color = Color.red;
                        break;
                }
            }

            UpdateCurrentAmmoUI();
        }

        void UpdateCurrentAmmoUI()
        {
            ammoText.SetText($"{CurrenAmmo}/{FinalMaxAmmo}");
        }


        private void UpdateReloadUI(float value)
        {
            if (rieioadImage != null)
            {
                rieioadImage.fillAmount = value;
            }
        }

        // リロード終了処理
        private void FinishReload()
        {
            if (rieioadUI != null)
            {
                rieioadUI.SetActive(false);
            }

            CurrenAmmo = FinalMaxAmmo;
            UpdateCurrentAmmoUI();
            isReloading = false;
        }

        public void AddExp(int amount)
        {
            CurrenExp+=amount;

            if (CurrenExp>=RequirdExp)
            {
                LeveUp();
            }

            UpdateExpUI();

        }

        void UpdateExpUI()
        {
            if (expBar!=null)
            {
                expBar.value=(float)CurrenExp/RequirdExp;
            }
        }


        private void LeveUp()
        {

            CurrenExp-=RequirdExp;

            CurrentLevel++;

            if (levelUpEffct!=null)
            {

                levelUpEffct.Play();
            }

            ShowLevelUpTextAsync().Forget();

        }

        private async UniTaskVoid ShowLevelUpTextAsync()
        {
            if (levelUpText==null)
            {
                return;
            }

            levelUpText.enabled=true;
            levelUpText.SetText($"Level Up\n<size=50%>Lv.{CurrentLevel}</size>");

            await UniTask.Delay(TimeSpan.FromSeconds(LEVEL_UP_EFFECT_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy());

            levelUpText.enabled = false;

            LevelUpManager.Instance.OnLeveUp(inputActions, this);
        }


        public void ApplySkill(SkillDataRecord skill)
        {

            switch ((SkillType)skill.SkillType)
            {

                case SkillType.MoveSpeedUp:moveSpeedBuf+=skill.Value;
                    break;

                case SkillType.AttackPowerUp: attackPowerBuf+=skill.Value;
                    break;

                case SkillType.FireRateUp:fireRateBuff+=skill.Value;
                    break;

                case SkillType.ReloadSpeedUp:reloadSpeedBuff+=skill.Value;
                    break;

                case SkillType.MaxAmmoUp: maxAmmoBuff+=(int)skill.Value;
                    UpdateCurrentAmmoUI();
                    break;

            }
        }


        } } 