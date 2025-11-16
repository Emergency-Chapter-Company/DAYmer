using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int coin;
    public int specialCoin;

    // 저장용 TimeRecord 데이터 리스트
    public List<TimeRecordData> savedTimeRecords = new List<TimeRecordData>();
}

[System.Serializable]
public class TimeRecordData
{
    public float totalSeconds;
    public string recordedDate;
}

public class SaveController : MonoBehaviour
{
    private string path;        // 저장 경로
    private string folderPath;  // 저장 폴더 경로

    private void Awake()
    {
        //path = Application.persistentDataPath + "/savedata.json";

        folderPath = Path.Combine(
#if UNITY_EDITOR
        Application.dataPath
#else
        Application.persistentDataPath
#endif
        , "Save");
        path = Path.Combine(folderPath, "SaveData.json");

        // 폴더 없으면 자동 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"📁 Save 폴더 생성됨: {folderPath}");
        }
    }

    public SaveData Load()
    {
        if (!File.Exists(path))
        {
            Debug.Log($"⚠️ 세이브 파일 없음, 새로 생성 : {path}");
            Save(new SaveData());
            return new SaveData();
        }

        string json = File.ReadAllText(path);
        SaveData loaded = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"📂 저장 데이터 로드 완료 : {path}");
        return loaded;
    }

    public void Save(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);

        // 저장 전 폴더 검사
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        File.WriteAllText(path, json);
        Debug.Log($"💾 세이브 완료 : {path}");
    }
}
