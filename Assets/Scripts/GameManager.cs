using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public int health;
    public List<string> items = new List<string>();
}

public class MapData
{
    public MapData(string mapName="Seoul_GamePlay_Scene")
    {
        this.mapName = mapName;
    }

    public string mapName { get; set; }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("플레이어 정보")]
    public PlayerData playerData = new PlayerData();

    [Header("맵 정보")]
    public MapData mapData = new MapData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    // 씬 이동 함수
    public void ChangeScene(string targetScene)
    {
        mapData.mapName = targetScene;
        // 씬 전환
        SceneManager.LoadScene(targetScene);
    }
}