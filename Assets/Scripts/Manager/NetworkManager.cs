using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon;

public class NetworkManager : Manager.SingletonPhoton<NetworkManager>
{
    private string _gameVersion = "Temp";
    private string _mapName = "Map2";

    private int _networkIndex = 1;

    public int AssignNetworkIndex()
    {
        return _networkIndex++;
    }

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();

        TryConnect();

        LobbyManager.Instance.UpdateConnectionWidgets();
    }


    public override void OnConnectedToMaster()
    {
        LobbyManager.Instance.UpdateConnectionWidgets();

        JoinLobby();

        PhotonNetwork.player.CustomProperties["Color"] = PlayerColor.Black;

        base.OnConnectedToMaster();
    }

    public override void OnDisconnectedFromPhoton()
    {
        LobbyManager.Instance.UpdateConnectionWidgets();

        base.OnDisconnectedFromPhoton();
    }

    private bool TryConnect()
    {
        if( !PhotonNetwork.connected)
        {
            PhotonNetwork.gameVersion = _gameVersion;
            
            return PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

        return false;
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void SetupNickName( string nickName)
    {
        PhotonNetwork.playerName = nickName;
    }

    public void JoinRoom(string _room_name)
    {
        if (PhotonNetwork.connected)
        {
            if (PhotonNetwork.GetRoomList().Length > 0)
            {
                PhotonNetwork.JoinRoom(PhotonNetwork.GetRoomList()[0].Name);
            }
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.isMessageQueueRunning = false;

        if( PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(_mapName);
        }
        else
        {
            SceneManager.LoadScene(_mapName);
        }

        //      SceneManager.LoadScene("Map1");
    }

    private void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.isMessageQueueRunning = true;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        // Manager.GameManager.Instance.CreateNewPlayer();
    }

    public void CreateRoom( string _room_name)
    {
        RoomOptions room_option = new RoomOptions();
        room_option.MaxPlayers = 10;

        TypedLobby type_lobby = new TypedLobby();
        type_lobby.Type = LobbyType.Default;

        PhotonNetwork.CreateRoom(_room_name, room_option, type_lobby); 
    }

    public void ExitRoom()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
        PhotonNetwork.LeaveRoom();
    }


    // Update is called once per frame
    void Update()
    {
    }
}
