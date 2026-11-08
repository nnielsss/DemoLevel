using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private static int _frames;
        private static double _start;
        private static float _gameStart;
        private static bool _setup;
        private static bool _captured;
        private static int _captureFrame;
        private static UnityEngine.AI.NavMeshAgent[] _agents;
        private static Vector3[] _positions;
        private static readonly List<string> _logs = new List<string>();
        private static bool _errors;
        private static string _screenshot;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString("PlayModeTest.State", "Idle");
            if (state == "WaitingForCompile")
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetString("PlayModeTest.State", "EnteringPlayMode");
                    EditorApplication.playModeStateChanged += Changed;
                    EditorApplication.isPlaying = true;
                };
            }
            else if (state == "EnteringPlayMode" || state == "InPlayMode")
            {
                EditorApplication.playModeStateChanged += Changed;
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.playModeStateChanged -= Changed;
                    SessionState.SetString("PlayModeTest.State", "InPlayMode");
                    EditorApplication.update += Update;
                }
            }
        }

        private static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            EditorApplication.playModeStateChanged -= Changed;
            SessionState.SetString("PlayModeTest.State", "InPlayMode");
            EditorApplication.update += Update;
        }

        private static void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) _errors = true;
            if ((message.Contains("[Test]") || type == LogType.Error || type == LogType.Exception) && _logs.Count < 50)
                _logs.Add(message);
        }

        private static void Update()
        {
            if (!EditorApplication.isPlaying) return;
            if (++_frames < 10) return;
            try
            {
                if (!_setup)
                {
                    _setup = true;
                    _start = EditorApplication.timeSinceStartup;
                    _gameStart = Time.time;
                    Application.logMessageReceived += Log;
                    _agents = Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>();
                    _positions = _agents.Select(a => a.transform.position).ToArray();
                    _screenshot = "Assets/Foliage_RuntimeCheck.png";
                    Debug.Log("[Test] NPC count " + _agents.Length);
                    return;
                }
                if (_captured)
                {
                    if (_frames - _captureFrame >= 3) Finish(null);
                    return;
                }
                if (Time.time - _gameStart < 5f && EditorApplication.timeSinceStartup - _start < 12d) return;
                ScreenCapture.CaptureScreenshot(_screenshot);
                SessionState.SetString("PlayModeTest.ScreenshotPath", _screenshot);
                _captured = true;
                _captureFrame = _frames;
            }
            catch (System.Exception e)
            {
                Finish(e.ToString());
            }
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public int plantCount;
            public int movingAgents;
            public float gameSeconds;
            public float maxGroundError;
            public string screenshotPath;
            public string[] logs;
        }

        private static void Finish(string error)
        {
            EditorApplication.update -= Update;
            var result = new TestResult();
            try
            {
                var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                var terrain = roots.SelectMany(g => g.GetComponentsInChildren<Terrain>(true)).Single();
                var scatter = roots.Single(g => g.name == "Foliage_Scatter");
                result.plantCount = scatter.transform.childCount;
                foreach (Transform plant in scatter.transform)
                {
                    float height = terrain.SampleHeight(plant.position) + terrain.transform.position.y;
                    result.maxGroundError = Mathf.Max(result.maxGroundError, Mathf.Abs(height - plant.position.y));
                }
                for (int i = 0; i < _agents.Length; i++)
                {
                    float movement = Vector3.Distance(_positions[i], _agents[i].transform.position);
                    Debug.Log("[Test] " + _agents[i].name + " movement=" + movement + " onNavMesh=" + _agents[i].isOnNavMesh + " path=" + _agents[i].pathStatus);
                    if (movement > 0.1f && _agents[i].isOnNavMesh) result.movingAgents++;
                }
                result.gameSeconds = Time.time - _gameStart;
                result.success = error == null && !_errors && result.plantCount == 950 && result.movingAgents == 3 && result.maxGroundError < 0.01f && RenderSettings.fog && RenderSettings.fogMode == FogMode.ExponentialSquared && terrain.terrainData == terrain.GetComponent<TerrainCollider>().terrainData;
                Debug.Log("[Test] plants=" + result.plantCount + " groundError=" + result.maxGroundError + " seconds=" + result.gameSeconds);
            }
            catch (System.Exception e)
            {
                error = e.ToString();
                result.success = false;
            }
            result.error = error;
            result.screenshotPath = _screenshot;
            result.logs = _logs.ToArray();
            Application.logMessageReceived -= Log;
            SessionState.SetString("PlayModeTest.Result", JsonUtility.ToJson(result));
            SessionState.SetString("PlayModeTest.State", "Done");
            EditorApplication.isPlaying = false;
        }
    }
}
