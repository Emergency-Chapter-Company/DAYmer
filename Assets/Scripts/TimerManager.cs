using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// 타이머 전체 기능
public class TimerManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;           // 시간 표시 텍스트 (00:00:00)
    [SerializeField] private Button startButton;                 // 시작 버튼
    [SerializeField] private Button pauseButton;                 // 일시정지 버튼
    [SerializeField] private Button stopButton;                  // 정지 및 기록 버튼

    [Header("Button Texts")]
    [SerializeField] private TextMeshProUGUI startButtonText;
    [SerializeField] private TextMeshProUGUI pauseButtonText;
    [SerializeField] private TextMeshProUGUI stopButtonText;

    [Header("Record UI")]
    [SerializeField] private Transform recordsContent;           // 기록이 추가될 Content
    [SerializeField] private GameObject recordItemPrefab;        // RecordItem 프리팹
    [SerializeField] private Button clearAllButton;                  // 전체 기록 삭제 버튼

    // 타이머 상태
    private float currentTime = 0f;   // 현재 경과 시간
    private bool isRunning = false;   // 실행 중 여부
    private bool isPaused = false;    // 일시정지 여부

    // 기록 관리 (일단은 콘솔만 출력)
    private List<TimeRecord> timeRecords = new List<TimeRecord>();      // 실제 데이터
    private List<GameObject> recordUIItems = new List<GameObject>();    // UI 아이템들

    private void Start()
    {
        SetupButtonListeners();
        UpdateTimeDisplay();
        UpdateButtonStates();
    }

    private void Update()
    {
        // 타이머가 실행 중이고 일시정지되지 않았을 때만 시간 증가
        if (isRunning && !isPaused)
        {
            currentTime += Time.deltaTime;
            UpdateTimeDisplay();
        }
    }

    /// UI 초기화
    /// 버튼 이벤트 리스너 설정
    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClick);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClick);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClick);

    }

    /// 시작 버튼 클릭 이벤트
    private void OnStartButtonClick()
    {
        if (!isRunning)
        {
            // 처음 시작
            isRunning = true;
            isPaused = false;
            Debug.Log("타이머 시작");
        }
        else if (isPaused)
        {
            // 일시정지 상태에서 재개
            isPaused = false;
            Debug.Log("타이머 재개");
        }

        UpdateButtonStates();
    }

    /// 일시정지 버튼 클릭 이벤트
    private void OnPauseButtonClick()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
            Debug.Log("타이머 일시정지");
            UpdateButtonStates();
        }
    }

    /// 정지 버튼 클릭 이벤트 (기록 저장 및 리셋)
    private void OnStopButtonClick()
    {
        if (isRunning)
        {
            // 현재 시간을 기록
            if (currentTime > 0)
            {
                TimeRecord newRecord = new TimeRecord(currentTime);
                timeRecords.Add(newRecord);

                // 기록을 콘솔에 출력
                Debug.Log($"기록 저장: {newRecord.recordTime}");

                // UI에 기록 추가
                AddRecordToUI(newRecord);
            }

            // 타이머 리셋
            ResetStopwatch();
        }
    }

    /// 타이머 리셋
    private void ResetStopwatch()
    {
        currentTime = 0f;
        isRunning = false;
        isPaused = false;
        UpdateTimeDisplay();
        UpdateButtonStates();
        Debug.Log("타이머 리셋");
    }

    /// 시간 표시 업데이트 (00:00:00 형식)
    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int hours = (int)(currentTime / 3600);
            int minutes = (int)((currentTime % 3600) / 60);
            int seconds = (int)(currentTime % 60);

            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    /// 버튼 상태 업데이트 (활성화/비활성화)
    private void UpdateButtonStates()
    {
        if (startButton != null)
        {
            // 시작 버튼: 실행 중이 아니거나 일시정지 상태일 때 활성화
            startButton.interactable = !isRunning || isPaused;
        }

        if (pauseButton != null)
        {
            // 일시정지 버튼: 실행 중이고 일시정지되지 않았을 때만 활성화
            pauseButton.interactable = isRunning && !isPaused;
        }

        if (stopButton != null)
        {
            // 정지 버튼: 실행 중일 때만 활성화
            stopButton.interactable = isRunning;
        }
        if (clearAllButton != null)
        {
            clearAllButton.onClick.AddListener(OnClearAllButtonClick);
        }
    }


    /// UI에 기록 보이기
    private void AddRecordToUI(TimeRecord record)
    {
        // null 체크
        if (recordItemPrefab == null || recordsContent == null)
        {
            Debug.LogWarning("RecordItem 프리팹 또는 Content가 연결되지 않았습니다!");
            return;
        }

        // 프리팹 복사해서 생성
        GameObject recordItem = Instantiate(recordItemPrefab, recordsContent);
        recordUIItems.Add(recordItem);

        // 기록 번호와 시간 설정
        TextMeshProUGUI recordText = recordItem.GetComponentInChildren<TextMeshProUGUI>();
        if (recordText != null)
        {
            int recordNumber = timeRecords.Count;
            recordText.text = $"#{recordNumber} - {record.recordTime}";
        }

        // 삭제 버튼 설정
        Button deleteButton = recordItem.GetComponentInChildren<Button>();
        if (deleteButton != null)
        {
            // 현재 recordItem을 캡처해서 람다에 전달
            GameObject itemToDelete = recordItem;
            deleteButton.onClick.AddListener(() => DeleteRecordUI(itemToDelete));
        }

        Debug.Log($"UI에 기록 추가됨: #{timeRecords.Count}");
    }

    /// 개별 기록 UI 삭제
    private void DeleteRecordUI(GameObject recordItem)
    {
        if (recordItem != null && recordUIItems.Contains(recordItem))
        {
            int index = recordUIItems.IndexOf(recordItem);

            // UI에서 제거
            recordUIItems.Remove(recordItem);
            Destroy(recordItem);

            Debug.Log($"UI 기록 #{index + 1} 삭제 (데이터는 유지)");
            Debug.Log($"남은 UI 기록 수: {recordUIItems.Count}");
            Debug.Log($"실제 데이터 기록 수: {timeRecords.Count}");

            // 번호 재정렬
            RefreshRecordNumbers();
        }
    }

    /// 전체 기록 UI 삭제
    private void OnClearAllButtonClick()
    {
        // 모든 UI 아이템 삭제
        foreach (GameObject item in recordUIItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        recordUIItems.Clear();

        Debug.Log("=== 전체 UI 기록 삭제 ===");
        Debug.Log($"UI 기록 수: {recordUIItems.Count}");
        Debug.Log($"실제 데이터 기록 수: {timeRecords.Count} (유지됨)");
        Debug.Log("========================");
    }

    /// 기록 번호 재정렬
    private void RefreshRecordNumbers()
    {
        for (int i = 0; i < recordUIItems.Count; i++)
        {
            if (recordUIItems[i] != null)
            {
                TextMeshProUGUI recordText = recordUIItems[i].GetComponentInChildren<TextMeshProUGUI>();
                if (recordText != null)
                {
                    // UI는 순서대로 #1, #2, #3...
                    // 하지만 실제 데이터는 원래 인덱스 유지
                    // 여기서는 UI 표시 번호만 바꿈
                    string currentText = recordText.text;

                    // 기존 시간 정보 추출 (- 이후 부분)
                    int dashIndex = currentText.IndexOf(" - ");
                    if (dashIndex > 0)
                    {
                        string timeInfo = currentText.Substring(dashIndex);
                        recordText.text = $"#{i + 1}{timeInfo}";
                    }
                }
            }
        }
    }

    /// 에디터에서 디버그용 정보 표시
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"상태: {(isRunning ? (isPaused ? "일시정지" : "실행 중") : "정지")}");
            GUI.Label(new Rect(10, 30, 300, 20), $"기록 수: {timeRecords.Count}");
        }
    }
}