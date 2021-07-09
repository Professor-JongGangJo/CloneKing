using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class SingletonPhoton<T> : Photon.PunBehaviour where T : Photon.PunBehaviour
    {
        private static T _instance = null;

        protected void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        var go = new GameObject();
                        var component = go.AddComponent<T>();

                        _instance = component;
                    }

                    _instance.gameObject.name = typeof(T).ToString();

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
    }
    public class DestoryableSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        var go = new GameObject();
                        var component = go.AddComponent<T>();

                        go.name = typeof(T).ToString();

                        _instance = component;
                    }
                }

                return _instance;
            }
        }
    }
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;
        
        public static T Instance
        {
            get
            {
                if( _instance == null)
                {
                    _instance = (T)FindObjectOfType<T>();

                    if( _instance == null)
                    {
                        var go = new GameObject();
                        var component = go.AddComponent<T>();

                        go.name = typeof(T).ToString();

                        DontDestroyOnLoad(go);

                        _instance = component;
                    }
                }

                return _instance;
            }
        }
    }

    public class GameManager : DestoryableSingleton<GameManager>
    {
        [SerializeField] private List<PhotonView> players;
        [SerializeField] private Text timeText;
        private float playTime = 1800.0f;

        public GameObject player;

        #region Timer Variables
        private IEnumerator _gameTimer;
        public int timeCount = 0;
        private int _hour = 0, _minute = 0, _second = 0;
        #endregion

        public List<PhotonView> GetPlayers() { return players; }

        // Comment : 타이머의 관련된 변수는 추후에 networkManager에서 관리가 필요합니다.
        //           접속시점이 기준이 아님.
        private void Start()
        {
            player = Resources.Load("Prefabs/PlayerChara") as GameObject;

            InitializeComponents();
            _gameTimer = TimeCoroutine();
            StartCoroutine(_gameTimer);

            // if (PhotonNetwork.isMasterClient)
            {
                StartCoroutine(DriveRanking());
            }
            //SpawnPlayer(); // Todo: 차후 네트워크 기능 추가 이후에는 변경해주세요.
        }

        private void InitializeComponents()
        {
            timeText = GameObject.Find("timer text").GetComponent<Text>();
            timeText.gameObject.SetActive(true);

        }

        private void Update()
        {
        }


        // PlayerController.cs의 void start()에서 추가 됩니다.
        public void AddPlayer(GameObject player)
        {
            var photonView = player.GetPhotonView();

            if (photonView != null)
            {
                players.Add(photonView);

                if (photonView.owner.IsMasterClient)
                {
                    PhotonNetwork.room.CustomProperties["Time"] = PhotonNetwork.time + playTime;
                }
            }
        }

        public void UpdateRank()
        {
            const float tolerance = 0.3f;

            players.Sort((lhs, rhs) => { return -lhs.gameObject.transform.position.y.CompareTo(rhs.gameObject.transform.position.y); });

            int rank = 1;

            PhotonView prevPlayer = null;
            int prevPlayerRank = 1;

            foreach (var player in players)
            {
                int currentRank = rank++;
                var photonView = player;

                if (photonView != null && photonView.isMine)
                {
                    int prevRank = -1;

                    photonView.TryGetValueToInt("Rank", out prevRank);

                    if (prevPlayer != null)
                    {
                        if (prevPlayer.gameObject.transform.position.y.NearlyEquals(player.gameObject.transform.position.y, tolerance))
                        {
                            currentRank = prevPlayerRank;
                        }
                    }

                    prevPlayer = player;
                    prevPlayerRank = currentRank;

                    if (currentRank != prevRank)
                    {
                        photonView.owner.CustomProperties["Rank"] = currentRank;
                        photonView.owner.SetCustomProperties(photonView.owner.CustomProperties);
                    }
                }
            }
        }

        public void ReachGoalEvent(GameObject player)
        {
            StopCoroutine(_gameTimer);
            StartCoroutine(DriveVictoryParticle());
            StartCoroutine(waitThenCallback(5.0f, () => { CreateRankingPopup(); }));

            foreach (PhotonView photonView in players)
            {
                photonView.gameObject.GetComponent<PlayerController>().enabled = false;
            }
        }

        private void CreateRankingPopup()
        {
            const string rankingPopupPath = "Prefabs/UI/RankingPopup";

            var canvas = FindObjectOfType<Canvas>();

            GameObject go = Instantiate(Resources.Load(rankingPopupPath) as GameObject);

            Debug.Log(go);
            if (canvas != null)
            {
                go.transform.SetParent(canvas.transform, false);
            }

        }

        //  Todo: 네트워크 접속이 될때 플레이어 생성 함수를 호출하면 될거 같습니다.
        public void SpawnPlayer()
        {
            StartCoroutine(waitThenCallback(1f, () =>
             {
                 Instantiate(player);
             }));
        }

        public void CreateNewPlayer()
        {
            CharaType charaType = CharaType.VirtualGuy;

            object obj;
            if (PhotonNetwork.player.CustomProperties.TryGetValue("CharaType", out obj))
            {
                charaType = (CharaType)obj;
            }

            string player_prefab_name = "Prefabs/PlayerChara";
            switch ( charaType)
            {
                case CharaType.VirtualGuy:
                    player_prefab_name = "Prefabs/PlayerChara Prototype";
                    break;
            }

            Vector3 start_location = new Vector3(0, 0, 0);

            PhotonNetwork.Instantiate(player_prefab_name, start_location, Quaternion.identity, 0);
        }

        private IEnumerator DriveRanking()
        {
            const float updateTime = 0.5f;

            while (true)
            {
                UpdateRank();

                yield return new WaitForSeconds(updateTime);
            }
        }


        private IEnumerator DriveVictoryParticle()
        {
            const float delayTime= 2.0f;
            const string resourcesName ="Prefabs/Effects/Fireworks";

            while(true)
            {
                Vector3 viewportRandomPosition = new Vector3(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 0.0f);
                Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(viewportRandomPosition);

                Instantiate(Resources.Load(resourcesName), spawnPosition, new Quaternion());

                yield return new WaitForSeconds(delayTime);
            }
        }

        private IEnumerator TimeCoroutine()
        {
            while (true)
            {
                object obj;
                if (PhotonNetwork.room.CustomProperties.TryGetValue("Time", out obj))
                {
                    double time = (double)obj;
                    double remainTime = time - PhotonNetwork.time;
                    
                    timeCount = Mathf.Max(0, ((int)remainTime));

                    _hour = (timeCount % (60 * 60 * 24)) / (60 * 60);
                    _minute = (timeCount % (60 * 60)) / (60);
                    _second = timeCount % (60);
                    timeText.text = _hour + ":" + _minute + ":" + _second;

                    if( remainTime <= 0.0d && players.Count > 0)
                    {
                        players[0].RPC("RPC_FinishGame", PhotonTargets.All, true);
                        break;
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }
        private IEnumerator waitThenCallback(float time, Action callback)
        {
            yield return new WaitForSeconds(time);
            callback();
        }
    }
    
    
}
