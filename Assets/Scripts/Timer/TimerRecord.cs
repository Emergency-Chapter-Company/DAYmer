using System;

/// 기록된 시간 정보를 저장하는 클래스
[Serializable]
public class TimeRecord
{
    private string recordTime;      // "00:00:00" 형식으로 표시되는 시간
    private DateTime recordDate;    // 기록된 날짜 및 시간
    private float totalSeconds;     // 총 경과 시간(초 단위)

    private int timePerCoins;  // 코인 계산을 위한 시간 단위

    public TimeRecord(float seconds, int timePerCoin = 60)
    {
        totalSeconds = seconds;
        recordTime = FormatTime(seconds);
        recordDate = DateTime.Now;
        timePerCoins = timePerCoin;
    }

    // 초를 00:00:00 형식으로 변환
    private string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, secs);
    }

    // 기록 정보를 문자열로 변환
    //public override string ToString()
    //{
    //    return $"{recordTime} - {recordDate:yyyy/MM/dd HH:mm:ss}";
    //}
    public string GetRecordDateString()
    {
        return recordDate.ToString("yyyy/MM/dd HH:mm:ss");
    }

    // 기록된 시간 문자열 가져오기
    public string GetRecordTime()
    {
        return recordTime;
    }

    // 기록된 시간 초 가져오기
    public float GetTotalSeconds()
    {
        return totalSeconds;
    }

    // 기록된 시간에 따른 코인 계산
    public int GetCoins()
    {
        return (int)(totalSeconds / timePerCoins); // timePerCoins분당 1코인 지급
    }
}
