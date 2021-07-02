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
    
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private List<GameObject> players;
        [SerializeField] private Text timeText;
        public GameObject player;
        
        
        #region Timer Variables
        private IEnumerator _gameTimer;
        public int timeCount = 0;
        private int _hour=0, _minute=0, _second=0;
        #endregion
        
        // Comment : Ÿ�̸��� ���õ� ������ ���Ŀ� networkManager���� ������ �ʿ��մϴ�.
        //           ���ӽ����� ������ �ƴ�.
        private void Start()
        {
            player = Resources.Load("Prefabs/PlayerChara") as GameObject;

            InitializeComponents();
            _gameTimer = TimeCoroutine();
            StartCoroutine(_gameTimer);
            //SpawnPlayer(); // Todo: ���� ��Ʈ��ũ ��� �߰� ���Ŀ��� �������ּ���.
        }

        private void InitializeComponents()
        {
        }


        // PlayerController.cs�� void start()���� �߰� �˴ϴ�.
        public void AddPlayer(GameObject player)
        {
            var photonView = player.GetPhotonView();

            if ( photonView != null)
            {
                players.Add(player);
            }
        }
        
        public void ReachGoalEvent(GameObject player)
        {
            StopCoroutine(_gameTimer);
            foreach (GameObject obj in players)
            {
                obj.GetComponent<PlayerController>().enabled = false;
            }
        }
        
        //  Todo: ��Ʈ��ũ ������ �ɶ� �÷��̾� ���� �Լ��� ȣ���ϸ� �ɰ� �����ϴ�.
        public void SpawnPlayer()
        {
            StartCoroutine(waitThenCallback(1f,() =>
            {
                Instantiate(player);
            }));
        }

        public void CreateNewPlayer()
        {
            const string player_prefab_name = "Prefabs/PlayerChara";
            Vector3 start_location = new Vector3(0, 0, 0);

            PhotonNetwork.Instantiate(player_prefab_name, start_location, Quaternion.identity, 0);
        }

        private IEnumerator TimeCoroutine()
        {
            while (true)
            {
                timeCount++;
                _hour = (timeCount%(60*60*24))/(60*60); 
                _minute = (timeCount%(60*60))/(60);
                _second = timeCount%(60);
                timeText.text = _hour + ":" + _minute + ":" + _second;
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
