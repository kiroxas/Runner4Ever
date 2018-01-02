using Photon;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : PunBehaviour {

    public GameObject PlayerPrefab;

    int userCount = 2;

    private void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        if (PhotonNetwork.connected)
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.ConnectUsingSettings("1");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        List<Vector2> spawns = UnityUtils.getSpawningLocations();
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawns[PhotonNetwork.room.PlayerCount - 1], Quaternion.identity, 0);

        player.GetComponent<CharacterController2D>().sendSpawn();

        if(PhotonNetwork.room.PlayerCount == userCount)
            EventManager.TriggerEvent(EventManager.get().unPauseAllPlayerEvent, new GameConstants.UnPauseAllPlayerArgument());

    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.room.PlayerCount == userCount)
            EventManager.TriggerEvent(EventManager.get().unPauseAllPlayerEvent, new GameConstants.UnPauseAllPlayerArgument());
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = (byte)userCount }, null);
    }
}
