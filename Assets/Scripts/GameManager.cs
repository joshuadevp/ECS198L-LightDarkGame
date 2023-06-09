using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool Pausing { get; private set; }
    public Canvas InGameCanvas;
    public Canvas UICanvas;

    [Header("Game Difficulty Settings")]
    [Tooltip("How much stronger each enemy gets per second")]
    [SerializeField] private float enemyStrengthOverTimeMultiplier = 0.01f;
    [Tooltip("How much stronger each enemy gets per level")]
    [SerializeField] private float enemyStrengthOverLevelMultiplier = 1f;

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject damageInfoPrefab;

    private float startTime;
    private float level = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            this.enabled = false;
        }
        startTime = Time.time;
        Pausing = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TogglePause());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Pausing = !Pausing;
        }
    }

    private void SpawnEnemy()
    {
        Instantiate(enemyPrefab, transform.position, transform.rotation);
    }

    public void SpawnDamageInfo(Vector3 position, float damage)
    {
        GameObject dmgInfo = Instantiate(damageInfoPrefab, InGameCanvas.transform);
        dmgInfo.transform.position = position;
        dmgInfo.GetComponent<TMPro.TMP_Text>().text = damage.ToString();
        Destroy(dmgInfo, 1.5f);
    }

    /// <summary>
    /// To be fine tuned for game feel: strengthen enemy stats based on time and level
    /// </summary>
    /// <returns> The modifier to be applied on enemy's stats</returns>
    public float CalculateEnemyModifier()
    {
        return 1 + (Time.time - startTime) * enemyStrengthOverTimeMultiplier + level * enemyStrengthOverLevelMultiplier;
    }

    public IEnumerator TogglePause()
    {
        float savedTimeScale;
        while (true) {
            yield return new WaitUntil(() => Pausing == true);
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitUntil(() => Pausing == false);
            Time.timeScale = savedTimeScale;
        }
    }
}