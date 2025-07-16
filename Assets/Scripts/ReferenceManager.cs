using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager Instance;
    public Canvas canvas;
    public RectTransform animationLayer;
    public GameObject minionHolderPrefab, minionPrefab;
    public RectTransform playerMinionPosition, enemyMinionPosition;
    public GameObject validZone;

    public CardManager cm;
    public BattleManager bm;
    public ArrowManager am;
    public EnemyAI ai;
    public AnimationManager aq;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}