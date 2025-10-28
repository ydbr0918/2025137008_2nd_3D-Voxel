using UnityEngine;

public static class SceneTransitData
{
    // 다음 스테이지 씬 이름(권장) 또는 빌드 인덱스
    public static string NextSceneName = "";
    public static int NextBuildIndex = -1;

    // 엘리베이터 내부 보여주는 시간/페이드 시간
    public static float RideSeconds = 5f;
    public static float FadeSeconds = 0.35f;
}
