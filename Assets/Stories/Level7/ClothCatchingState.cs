using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using Pixelplacement;
using TMPro;
public class ClothCatchingState : State
{
    [SerializeField] private Transform background;
    [SerializeField] private RectTransform progressMask;
    [SerializeField] private GameObject startCanvas, progressCanvas, endCanvas, heartEffect;
    [SerializeField] private Transform[] spawnPositons;
    [SerializeField] private GameObject[] propPrefabs;
    [SerializeField] private TextMeshProUGUI countDownLabel, countDownClockLabel, scoreLabel, endScoreLabel, rewardLabel;
    [SerializeField] private PlayerCatchingClothController player;
    [SerializeField] private int matchDuration = 30;
    [SerializeField] private ParticleSystem.MinMaxCurve spawnIntervalRange = new ParticleSystem.MinMaxCurve(0.15f, 3.5f);
    [SerializeField] private int[] moveDurations = new int[] { 5, 4, 3, 2, 1 };
    private float originMaskHeight;
    // Start is called before the first frame update
    private void Start()
    {
        originMaskHeight = progressMask.sizeDelta.y;
        if (background != null)
        {
            background.localScale *= Screen.height * 9f / 16f / Screen.width;
        }
    }
    void OnEnable()
    {
        StartCoroutine(GameLoop());
    }
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(GameStartStartState());
        yield return StartCoroutine(GamePlayState());
    }
    private IEnumerator GameStartStartState()
    {
        startCanvas.GetComponent<State>().ChangeState(startCanvas.gameObject);
        var counter = 3;
        countDownLabel.gameObject.SetActive(true);
        countDownLabel.text = "Tap To Start";
        yield return new WaitForSeconds(1);
        while (!Input.GetMouseButtonDown(0)) yield return null;
        while (counter >= 0)
        {
            countDownLabel.color = new Color(1, 1, 1, 0.5f);
            countDownLabel.transform.localScale = new Vector3(2, 2, 2);
            Tween.LocalScale(countDownLabel.transform, new Vector3(1, 1, 1), 0.2f, 0, Tween.EaseOutBack);
            Tween.Value(countDownLabel.color, Color.white, (Color value) =>
            {
                countDownLabel.color = value;
            }, 0.2f, 0);
            countDownLabel.text = counter.ToString();
            yield return new WaitForSeconds(1);
            counter--;
        }
        countDownLabel.gameObject.SetActive(false);
        progressMask.sizeDelta = new Vector2(progressMask.sizeDelta.x, 0);
    }
    private IEnumerator GamePlayState()
    {
        progressCanvas.GetComponent<State>().ChangeState(progressCanvas.gameObject);
        var timeStamp = Time.time;
        var spawnInterval = Random.Range(2f, 5f);
        var spawnTimeStamp = Time.time - spawnInterval;
        while (Time.time - timeStamp < matchDuration && player.isAlive)
        {
            yield return new WaitForSeconds(1);
            scoreLabel.text = player.currentScore.ToString();
            var remainTime = matchDuration + timeStamp - Time.time;
            countDownClockLabel.text = string.Format("{0:D2}:{1:D2}", Utils.GetMinutes(remainTime), Utils.GetSeconds(remainTime));
            progressMask.sizeDelta = new Vector2(progressMask.sizeDelta.x, originMaskHeight * player.currentScore / player.clothThresholds[player.clothThresholds.Length - 1]);
            heartEffect.SetActive(player.currentScore >= player.clothThresholds[player.clothThresholds.Length - 1]);
            var lastSpawnIndex = -1;
            if (Time.time - spawnTimeStamp >= spawnInterval)
            {
                var spawnIndex = Random.Range(0, spawnPositons.Length);
                while (spawnIndex == lastSpawnIndex)
                    spawnIndex = Random.Range(0, spawnPositons.Length);
                lastSpawnIndex = spawnIndex;
                var prop = Instantiate(propPrefabs[Random.Range(0, propPrefabs.Length)], spawnPositons[spawnIndex].position, Quaternion.identity, transform);
                Tween.Value(prop.transform.position, prop.transform.position - new Vector3(0, 180, 0), (Vector3 value) =>
                {
                    if (prop != null) prop.transform.position = value;
                }, moveDurations[player.GetCurrentClothIndex() + 1], 0);
                Destroy(prop, moveDurations[player.GetCurrentClothIndex() + 1]);
                spawnTimeStamp = Time.time;
                spawnInterval = Random.Range(spawnIntervalRange.constantMin, spawnIntervalRange.constantMax);
            }
        }
        player.isAlive = false;
        StartCoroutine(GameEndState());
    }
    private IEnumerator GameEndState()
    {
        yield return null;
        endCanvas.GetComponent<State>().ChangeState(endCanvas.gameObject);
        endScoreLabel.text = player.currentScore.ToString();
        rewardLabel.text = "x" + player.currentScore.ToString();
    }
    public void OnClickReplay()
    {
        player.currentScore = 0;
        player.ResetCharacter();
        player.isAlive = true;
        StartCoroutine(GameLoop());
    }
}
