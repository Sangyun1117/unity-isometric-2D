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

    [Header("�÷��̾� ����")]
    public PlayerData playerData = new PlayerData();

    [Header("�� ����")]
    public MapData mapData = new MapData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� ����
        }
    }

    // �� �̵� �Լ�
    public void ChangeScene(string targetScene)
    {
        mapData.mapName = targetScene;
        // �� ��ȯ
        SceneManager.LoadScene(targetScene);
    }
}