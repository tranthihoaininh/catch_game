using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCatchingClothController : MonoBehaviour
{
    public int currentScore = 0;
    public bool isAlive = true;
    [SerializeField] private AudioClip effectClip, nopeClip;
    [SerializeField] private GameObject bombEffectPrefab, scoreEffectPrefab;
    [SerializeField] public int[] clothThresholds = { 10, 20, 30 };
    [SerializeField] private GameObject[] cloths;
    public Vector3 originPos;
    private Transform originParent;
    private Camera mainCamera;
    public string characterId = "1000";
    [SerializeField] private AudioSource effectSource;
    // Start is called before the first frame update
    void Start()
    {
        originParent = transform.parent;
        originPos = transform.position;
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (isAlive)
            transform.position = new Vector3(mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)).x, originPos.y, originPos.z);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bomb")
        {
            isAlive = false;
            Vibration.VibrateNope();
            var go = Instantiate(bombEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(go, 1);
            effectSource.PlayOneShot(nopeClip);
        }
        else
        {
            currentScore++;
            var index = GetCurrentClothIndex();
            for (int i = 0; i < cloths.Length; i++)
                cloths[i].SetActive(i <= index);
            Vibration.VibratePop();
            var go = Instantiate(scoreEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(go, 1);
            effectSource.PlayOneShot(effectClip);
        }
        Destroy(other.gameObject);
    }
    public void ResetCharacter()
    {
        transform.SetParent(originParent);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }
    public int GetCurrentClothIndex()
    {
        var index = -1;
        for (int i = 0; i < clothThresholds.Length; i++)
            if (currentScore >= clothThresholds[i]) index = i;
        return index;
    }
}
