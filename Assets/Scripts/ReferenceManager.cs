using TMPro;
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
    public RectTransform endGamePanel;
    public TextHelper textHelper;

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
    public Transform allCardPanel, selectedCardPanel;

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