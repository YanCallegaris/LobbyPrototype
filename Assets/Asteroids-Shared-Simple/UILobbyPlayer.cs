using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UILobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText, _readyStateText;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerState(string playerName, bool isReady, bool isLocalPlayer)
    {
        _playerNameText.text = playerName;
        _playerNameText.color = isLocalPlayer ? Color.green : Color.white;

        _readyStateText.text = isReady ? "Ready" : "Not Ready";
        _readyStateText.color = isReady ? Color.white : Color.red;
    }
}
