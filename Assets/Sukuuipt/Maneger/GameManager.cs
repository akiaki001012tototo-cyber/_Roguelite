using UnityEngine;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.Spawner;
using Cysharp.Threading.Tasks;
using Core.MasterData;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;


namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {

        private const string RESULT_SCENE_NAME = "GemuSaukoa";

        public static GameManager Instance { get; private set; }

        [SerializeField] private PlieaControra player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;

        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private float gameClearTime = 180f;

        private float currentTime = 0f;
        private bool isGameActive=false;

        public bool IsGameClear { get; private set; }
        public float SurvivedTime { get; private set; }
        public int FinalLevel { get; private set; }

        private void Awake()
        {
            if (Instance==null)
            {
                Instance=this;
                DontDestroyOnLoad(gameObject);
            }
        }



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Setup().Forget();
        }

        async UniTaskVoid Setup()
        {
            //マスターデータの読み込み
            await MasterDataAccessor.Instance.InitializeAsync();

            //読み込みが完了したら、プレイヤーとスポナーの準備を始める
            if (player!=null)
            {
                player.Setup();
            }

            if (enemySpawner!=null)
            {
                enemySpawner.Setup();
            }


            IsGameClear = false;
            currentTime = gameClearTime;
            isGameActive = true;

        }


        private void Update()
        {
            if (!isGameActive)
            { 
            return;
            }
            if (Time.timeScale==0f)
            {
                return;
            }

            currentTime-=Time.deltaTime;
            SurvivedTime=gameClearTime-currentTime;

            if (timerText!=null)
            {
                int minutes =Mathf.FloorToInt(currentTime/60f);
                int seconds = Mathf.FloorToInt(currentTime - minutes * 60f);
                timerText.SetText($"{minutes:00}:{seconds:00}");

                if (currentTime<=0f)
                {
                    GameClear();
                }
            
                        }

        }


        void GameClear()
        {
            isGameActive=false;
            IsGameClear=true;
            FinalLevel=player!=null?player.CurrentLevel:0;

            GoToResultScene();
        }

        public void GameOver()
        {
            isGameActive=false;
            IsGameClear=false;
            FinalLevel=player!=null ? player.CurrentLevel : 0;

            Debug.Log("ゲームオーバー");

            GoToResultScene();
        }

        void GoToResultScene()
        { 
        Time.timeScale=1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(RESULT_SCENE_NAME);
        }

    }
}