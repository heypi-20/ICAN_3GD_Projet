using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class S_GameResultCalcul : MonoBehaviour
{
    /* ──────────  UI  ────────── */
    [Header("Result-Panel References")]
    public GameObject settlementPanel;
    public TMP_Text[] killsTexts;          // left column
    public TMP_Text[] scoreTexts;          // right column
    public TMP_Text   timeBonusText;
    public TMP_Text   timeResultText;
    public TMP_Text   finalScoreText;

    /* ─── Mission-End Indicator ─── */
    [Header("Mission-End Indicator")]
    public GameObject indicatorRoot;       // parent object to enable / disable
    public TMP_Text   indicatorText;       // text component to animate
    [Tooltip("Full message that will be typed out")]
    public string indicatorMessage       = "MISSION COMPLETE!";
    [Tooltip("Peak scale during pop-in")]
    public float  indicatorPopScale      = 1.4f;
    [Tooltip("Pop-in duration (s)")]
    public float  indicatorPopDuration   = 0.25f;
    [Tooltip("Typing duration (s)")]
    public float  indicatorTypeDuration  = 1.0f;
    [Tooltip("Hide indicator after result menu appears?")]
    public bool   hideIndicatorOnMenu    = true;

    /* ───────── Bonus Mapping (curve) ─────── */
    [Header("Time-Bonus Curve")]
    [Tooltip(
        "X = elapsed minutes, Y = bonus percent (500 = 500 %).\n" +
        "Example default: 0 min ➜ 500 %, 5 min ➜ 300 %, 8 min ➜ 0 %.")]
    public AnimationCurve timeBonusCurve = new AnimationCurve(
        new Keyframe(0f, 500f),
        new Keyframe(5f, 300f),
        new Keyframe(8f, 0f));

    /* ───────── Animation Settings ───────── */
    [Header("Animation Settings")]
    [Tooltip("Target slow-motion Time.timeScale")]
    public float slowMotionScale        = 0.2f;
    [Tooltip("Seconds to ramp 1 → slowMotionScale")]
    public float slowMotionRampDuration = 0.6f;
    [Tooltip("Hold slow-motion this long before freeze")]
    public float slowMotionHoldDuration = 0.6f;

    [Tooltip("Delay between each row reveal (s)")]
    public float revealInterval = 0.15f;
    [Tooltip("Pop target scale for rows / labels")]
    public float popScale       = 1.2f;
    [Tooltip("Pop duration for rows / labels (s)")]
    public float popDuration    = 0.25f;

    [Tooltip("Counting duration per row (s)")]
    public float countDurationRow   = 0.8f;
    [Tooltip("Counting duration for final score (s)")]
    public float countDurationFinal = 1.0f;

    /* ───────── Runtime Values ───────── */
    public float timeChrono;
    public int   finalScore;

    S_ScoreManager  scoreManager;
    S_BossObjective mainObjective;

    bool menuActive    = false;   // true while result animations play
    bool skipRequested = false;   // true when user skips

    /* ───────────────────────────────────── */

    void Awake()
    {
        // All DOTween tweens run in real-time, ignoring Time.timeScale
        DOTween.defaultTimeScaleIndependent = true;

        // Clamp the bonus curve outside its last key
        timeBonusCurve.postWrapMode = WrapMode.ClampForever;
    }

    void Start()
    {
        scoreManager  = FindObjectOfType<S_ScoreManager>();
        mainObjective = FindObjectOfType<S_BossObjective>();
    }

    /* Public entry point */
    public void ShowResultScreen() => StartCoroutine(ResultRoutine());

    /* Dev hot-key & skip detection */
    void Update()
    {

        if (menuActive && !skipRequested && Input.GetMouseButtonDown(0))
            SkipAnimations();
    }

    /* ───────── Main Coroutine ───────── */
    IEnumerator ResultRoutine()
    {
        skipRequested = false;
        menuActive    = false;

        /* 0️⃣  Indicator animation (runs in parallel) */
        StartCoroutine(PlayIndicator());

        /* 1️⃣  Smooth slow-motion ramp */
        yield return DOTween.To(() => Time.timeScale,
                                v  => Time.timeScale = v,
                                slowMotionScale,
                                slowMotionRampDuration)
                            .SetUpdate(true)
                            .WaitForCompletion();

        yield return WaitForSecondsWithSkip(slowMotionHoldDuration);

        /* 2️⃣  Final freeze and prepare UI */
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        PopulateTexts();
        HideAllTexts();
        settlementPanel.SetActive(true);

        if (hideIndicatorOnMenu)
            indicatorRoot?.SetActive(false);

        menuActive = true;

        /* 3️⃣  Row reveals (kill + score together) */
        var data = scoreManager.scoreDatas;
        int rows = Mathf.Min(data.Count, Mathf.Min(killsTexts.Length, scoreTexts.Length));

        for (int i = 0; i < rows; i++)
        {
            int kills  = data[i].killed;
            int score  = Mathf.RoundToInt(data[i].totalScore);
            yield return RevealRow(killsTexts[i], scoreTexts[i], kills, score);
        }

        /* 4️⃣  Time + bonus labels */
        yield return RevealSimple(timeResultText);
        yield return RevealSimple(timeBonusText);

        /* 5️⃣  Final score with its own duration */
        yield return RevealNumber(finalScoreText, finalScore, countDurationFinal);

        menuActive = false;
    }

    /* ───────── Indicator Coroutine ───────── */
    IEnumerator PlayIndicator()
    {
        if (indicatorRoot == null || indicatorText == null) yield break;

        indicatorRoot.SetActive(true);
        indicatorText.text = "";
        indicatorText.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // pop-in
        seq.Append(indicatorText.transform
                .DOScale(indicatorPopScale, indicatorPopDuration)
                .From(0f)
                .SetEase(Ease.OutBack));
        
        StartCoroutine(TypeWriter(indicatorText, indicatorMessage, indicatorTypeDuration));

        seq.OnComplete(() => indicatorText.transform.localScale = Vector3.one);

        // wait while still allowing skip
        yield return WaitForSecondsWithSkip(indicatorPopDuration + indicatorTypeDuration);
    }
    IEnumerator TypeWriter(TMP_Text txt, string message, float duration)
    {
        txt.text = "";
        int len = message.Length;
        for (int i = 0; i < len; i++)
        {
            txt.text = message.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(duration / len);
        }
    }

    /* ───────── Populate & Hide ───────── */
    void PopulateTexts()
    {
        var list  = scoreManager.scoreDatas;
        int count = Mathf.Min(list.Count, Mathf.Min(killsTexts.Length, scoreTexts.Length));

        float totalSec = mainObjective.gameChrono;
        float minutes  = totalSec / 60f;

        // ⚠️  Bonus % now comes from AnimationCurve
        float bonusPct = Mathf.Max(timeBonusCurve.Evaluate(minutes), 0f);
        float multiplier = bonusPct * 0.01f;

        for (int i = 0; i < count; i++)
        {
            killsTexts[i].text  = list[i].killed.ToString();
            scoreTexts[i].text  = Mathf.RoundToInt(list[i].totalScore).ToString();
        }

        int mm = Mathf.FloorToInt(totalSec / 60f);
        int ss = Mathf.FloorToInt(totalSec % 60f);
        timeResultText.text = $"Time: {mm:D2}:{ss:D2}";
        timeBonusText.text  = $"x {bonusPct:F0}%";

        float sumRaw = list.Sum(d => d.totalScore);
        int   sumAdj = Mathf.RoundToInt(sumRaw * multiplier);
        finalScoreText.text = sumAdj.ToString();

        finalScore = sumAdj;
        timeChrono = totalSec;
    }

    void HideAllTexts()
    {
        foreach (var t in killsTexts)   t.transform.localScale = Vector3.zero;
        foreach (var t in scoreTexts)   t.transform.localScale = Vector3.zero;
        timeBonusText.transform.localScale  = Vector3.zero;
        timeResultText.transform.localScale = Vector3.zero;
        finalScoreText.transform.localScale = Vector3.zero;
    }

    /* ───────── Row Reveal (kill + score) ───────── */
    IEnumerator RevealRow(TMP_Text killTxt, TMP_Text scoreTxt,
                          int killTarget, int scoreTarget)
    {
        if (skipRequested)
        {
            killTxt.transform.localScale  = Vector3.one;
            scoreTxt.transform.localScale = Vector3.one;
            killTxt.text  = killTarget.ToString();
            scoreTxt.text = scoreTarget.ToString();
            yield break;
        }

        killTxt.text  = "0";
        scoreTxt.text = "0";

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        seq.Join(killTxt.transform.DOScale(popScale, popDuration)
                                   .From(0f)
                                   .SetEase(Ease.OutBack));
        seq.Join(DOTween.To(() => 0, v => killTxt.text = v.ToString(),
                            killTarget, countDurationRow)
                        .SetEase(Ease.Linear));

        seq.Join(scoreTxt.transform.DOScale(popScale, popDuration)
                                   .From(0f)
                                   .SetEase(Ease.OutBack));
        seq.Join(DOTween.To(() => 0, v => scoreTxt.text = v.ToString(),
                            scoreTarget, countDurationRow)
                        .SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            killTxt.transform.localScale  = Vector3.one;
            scoreTxt.transform.localScale = Vector3.one;
        });

        yield return WaitForSecondsWithSkip(Mathf.Max(popDuration, countDurationRow) + revealInterval);
    }

    /* ───────── Single Number Reveal ───────── */
    IEnumerator RevealNumber(TMP_Text txt, int target, float duration)
    {
        if (skipRequested)
        {
            txt.transform.localScale = Vector3.one;
            txt.text = target.ToString();
            yield break;
        }

        txt.text = "0";
        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Append(txt.transform.DOScale(popScale, popDuration)
                              .From(0f)
                              .SetEase(Ease.OutBack));
        s.Join(DOTween.To(() => 0, v => txt.text = v.ToString(),
                          target, duration)
                      .SetEase(Ease.Linear));
        s.OnComplete(() => txt.transform.localScale = Vector3.one);

        yield return WaitForSecondsWithSkip(Mathf.Max(popDuration, duration) + revealInterval);
    }

    /* ───────── Simple Label Reveal ───────── */
    IEnumerator RevealSimple(TMP_Text txt)
    {
        if (skipRequested)
        {
            txt.transform.localScale = Vector3.one;
            yield break;
        }

        txt.transform.localScale = Vector3.zero;
        txt.transform.DOScale(popScale, popDuration)
                     .From(0f)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true)
                     .OnComplete(() => txt.transform.localScale = Vector3.one);

        yield return WaitForSecondsWithSkip(popDuration + revealInterval);
    }

    /* ───────── Skip Helper ───────── */
    void SkipAnimations()
    {
        skipRequested = true;
        DOTween.CompleteAll();     // finish every tween instantly
        PopulateTexts();
        foreach (var t in killsTexts)   t.transform.localScale = Vector3.one;
        foreach (var t in scoreTexts)   t.transform.localScale = Vector3.one;
        timeBonusText.transform.localScale  = Vector3.one;
        timeResultText.transform.localScale = Vector3.one;
        finalScoreText.transform.localScale = Vector3.one;
    }

    IEnumerator WaitForSecondsWithSkip(float seconds)
    {
        float elapsed = 0f;
        while (!skipRequested && elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
