using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    [Header("Misc")]
    public static ReferenceManager Instance;
    public Canvas canvas;
    public RectTransform animationLayer;
    public GameObject minionHolderPrefab, minionPrefab;
    public RectTransform playerMinionPosition, enemyMinionPosition;
    public GameObject validZone;
    public Sprite tauntMinionSprite;
    public RectTransform blurPanel;

    [Header("Instance")]
    public CardManager cm;
    public BattleManager bm;
    public BuffManager buffm;
    public ArrowManager am;
    public EnemyAI ai;
    public AnimationManager anim;
    public SoundManager sm;

    [Header("Deck Builder")]
    public GameObject deckValidZone;
    public GameObject removeFromDeckValidZone;

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