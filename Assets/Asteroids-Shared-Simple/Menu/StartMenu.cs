using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;

namespace Asteroids.SharedSimple
{
    // A utility class which defines the behaviour of the various buttons and input fields found in the Menu scene
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;

        [SerializeField] private TMP_InputField _nickName = null;

        // The Placeholder Text is not accessible through the TMP_InputField component so need a direct reference
        [SerializeField] private TextMeshProUGUI _nickNamePlaceholder = null;

        [SerializeField] private TMP_InputField _roomName = null;
        [SerializeField] private string _lobbySceneName = null;

        private NetworkRunner _runnerInstance = null;

        // Attempts to start a new game session 
        public void StartShared()
        {
            SetPlayerData();
            StartGame(GameMode.Shared, _roomName.text, _lobbySceneName);
        }
        

        private void SetPlayerData()
        {
            if (string.IsNullOrWhiteSpace(_nickName.text))
            {
                LocalPlayerData.NickName = _nickNamePlaceholder.text;
            }
            else
            {
                LocalPlayerData.NickName = _nickName.text;
            }
        }

        private async void StartGame(GameMode mode, string roomName, string sceneName)
        {
            _runnerInstance = FindObjectOfType<NetworkRunner>();
            if (_runnerInstance == null)
            {
                _runnerInstance = Instantiate(_networkRunnerPrefab);
            }

            // Let the Fusion Runner know that we will be providing user input
            _runnerInstance.ProvideInput = true;

            NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
            var sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(_lobbySceneName));
            sceneInfo.AddSceneRef(sceneRef);

            var startGameArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = sceneInfo,
                ObjectProvider = _runnerInstance.GetComponent<NetworkObjectPoolDefault>(),
            };

            // GameMode.Host = Start a session with a specific name
            // GameMode.Client = Join a session with a specific name
            await _runnerInstance.StartGame(startGameArgs);

            if (_runnerInstance.IsServer)
            {
                _runnerInstance.LoadScene(sceneName);
            }
        }
    }
}