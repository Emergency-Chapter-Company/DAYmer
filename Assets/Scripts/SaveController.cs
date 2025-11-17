using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int coin;
    public int specialCoin;

    /* 데이터 리스트 */
    public List<TimeRecordData> savedTimeRecords = new List<TimeRecordData>();      // TimeRecordData 참조
    public List<int> ownedItemIDs = new List<int>();                                // 인벤토리 아이템 ID 리스트
    public List<PlacedItemSaveData> placedItems = new List<PlacedItemSaveData>();   // 배치된 아이템 데이터 리스트
}

[System.Serializable]
public class TimeRecordData
{
    public float totalSeconds;
    public string recordedDate;
}

[System.Serializable]
public class PlacedItemSaveData
{
    public int itemID;
    public Vector3 position;
    public Quaternion rotation;
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
