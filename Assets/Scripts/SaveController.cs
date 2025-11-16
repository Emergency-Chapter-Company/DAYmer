using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int coin;
    public int specialCoin;

    // 저장용 TimeRecord 데이터 리스트
    public List<TimeRecordData> savedRecords = new List<TimeRecordData>();
}

[System.Serializable]
public class TimeRecordData
{
    public float totalSeconds;
    public string recordedDate;
}

public class SaveController : MonoBehaviour
{
    private string path; // 저장 경로

    private void Awake()
    {
        //path = Application.persistentDataPath + "/savedata.json";
        path = Path.Combine(Application.dataPath, "SaveData.json"); // (디버깅) 프로젝트 경로로 설정
    }

    public SaveData Load()
    {
        if (!File.Exists(path))
        {
            Debug.Log($"세이브 파일 생성 : {path}");
            Save(new SaveData());
            return new SaveData();
        }

        string json = File.ReadAllText(path);
        SaveData loaded = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"로드 완료 : {path}");
        return loaded;
    }

    public void Save(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
        Debug.Log($"세이브 완료 : {path}");
    }
}
