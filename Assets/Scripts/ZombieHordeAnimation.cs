using UnityEngine;

public class ZombieHordeAnimation : MonoBehaviour {

    [SerializeField] private Sprite[] zombieSprites = new Sprite[4];
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex;

    void Start(){

        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpriteIndex = Random.Range(0, zombieSprites.Length - 1);
    }

    public void CycleZombieSprite(){

        currentSpriteIndex++;
        if (currentSpriteIndex >= zombieSprites.Length) {
            currentSpriteIndex = 0;
        }
        spriteRenderer.sprite = zombieSprites[currentSpriteIndex];
    }
}
