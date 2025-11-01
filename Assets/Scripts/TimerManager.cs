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

    // 타이머 상태
    private float currentTime = 0f;   // 현재 경과 시간
    private bool isRunning = false;   // 실행 중 여부
    private bool isPaused = false;    // 일시정지 여부

    // 기록 관리 (일단은 콘솔만 출력)
    private List<TimeRecord> timeRecords = new List<TimeRecord>();

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
                Debug.Log($"기록 저장: {newRecord.recordTime}");
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