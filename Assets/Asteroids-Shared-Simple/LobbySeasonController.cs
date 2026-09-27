using Asteroids.SharedSimple;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbySeasonController : NetworkBehaviour, IPlayerLeft, IStateAuthorityChanged
{
    private const int MaxPlayerCount = 8;

    [SerializeField] private Transform _contentTransform;
    [SerializeField] private UILobbyPlayer _uiLobbyPlayerPrefab;
    [SerializeField] private Button _toggleReadyButton;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveRoomButton;
    [SerializeField] private string _gameSceneName;

    [Networked, OnChangedRender(nameof(OnPlayerDataChanged))]
    [Capacity(MaxPlayerCount)]
    private NetworkDictionary<PlayerRef, LobbyPlayerData> _playerData => default;
    private UILobbyPlayer[] _uiPlayerElements;

    private readonly List<KeyValuePair<PlayerRef, LobbyPlayerData>> _reusablePlayerSortList = new(capacity: MaxPlayerCount);

    private void Awake()
    {
        _toggleReadyButton.onClick.AddListener(OnToggleReadyClick);
        _startGameButton.onClick.AddListener(OnClickStartGame);
        _leaveRoomButton.onClick.AddListener(OnClickLeaveRoom);

        _startGameButton.interactable = false;
        CreateUILobbyEntries();
    }

    private void CreateUILobbyEntries()
    {
        _uiPlayerElements = new UILobbyPlayer[MaxPlayerCount];

        for (int i = 0; i < MaxPlayerCount; i++)
        {
            var instance = Instantiate(_uiLobbyPlayerPrefab, _contentTransform);
            _uiPlayerElements[i] = instance;
        }
    }

    public override void Spawned()
    {
        var data = new LobbyPlayerData()
        {
            PlayerName = LocalPlayerData.NickName,
            isReady = false
        };
        RPC_SetPlayerData(data);
        UpdateStartGameButton();
        OnPlayerDataChanged();
    }

    private void OnPlayerDataChanged()
    {
        _reusablePlayerSortList.Clear();

        foreach (var player in _playerData)
        {
            _reusablePlayerSortList.Add(player);
        }

        _reusablePlayerSortList.Sort((pair, pair2) => pair.Key.PlayerId.CompareTo(pair2.Key.PlayerId));

        for (int i = 0; i < _uiPlayerElements.Length; i++)
        {
            if (i < _reusablePlayerSortList.Count)
            {
                var value = _reusablePlayerSortList[i].Value;
                var IsLocalPlayer = _reusablePlayerSortList[i].Key == Runner.LocalPlayer;
                _uiPlayerElements[i].SetPlayerState(value.PlayerName.Value, value.isReady, IsLocalPlayer);

                _uiPlayerElements[i].gameObject.SetActive(true);
            }
            else
            {
                _uiPlayerElements[i].gameObject.SetActive(false);
            }
        }

        UpdateStartGameButton();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerData(LobbyPlayerData data, RpcInfo info = default)
    {
        if (_playerData.ContainsKey(info.Source))
        {
            _playerData.Set(info.Source, data);
        }
        else
        {
            _playerData.Add(info.Source, data);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ToggleReadyState(RpcInfo info = default)
    {
        if (_playerData.TryGet(info.Source, out LobbyPlayerData data))
        {
            data.isReady = !data.isReady;
            _playerData.Set(info.Source, data);
        }
    }

    private void UpdateStartGameButton()
    {
        _startGameButton.interactable = CanStartGame();
    }

    private bool CanStartGame()
    {
        if (HasStateAuthority == false) return false;

        foreach(var entry in _playerData)
        {
            if(entry.Value.isReady == false)
                return false;
        }

        return _playerData.Count > 0;
    }

    private void OnClickLeaveRoom()
    {
        Runner.Shutdown();
    }

    private void OnClickStartGame()
    {
        if (!CanStartGame()) return;

        Runner.SessionInfo.IsVisible = false;
        Runner.SessionInfo.IsOpen = false;
        Runner.LoadScene(_gameSceneName);
    }

    private void OnToggleReadyClick()
    {
        RPC_ToggleReadyState();
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            _playerData.Remove(player);
            UpdateStartGameButton();
        }
    }

    public void StateAuthorityChanged()
    {
        if (HasStateAuthority)
        {
            _reusablePlayerSortList.Clear();
            bool isActivePlayer = false;
            foreach (var playerData in _playerData)
            {
                foreach (var activePlayerRef in Runner.ActivePlayers)
                {
                    if (playerData.Key == activePlayerRef)
                    {
                        isActivePlayer = true;
                        break;
                    }
                }

                if (isActivePlayer == false)
                {
                    _reusablePlayerSortList.Add(playerData);
                }
            }
        }

        foreach(var playerToRemove in _reusablePlayerSortList)
        {
            _playerData.Remove(playerToRemove.Key);
        }
    }
}
