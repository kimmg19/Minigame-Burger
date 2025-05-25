using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Droppable : MonoBehaviour, IDroppable
{
    [Header("Drop Settings")]
    [SerializeField] private float stackOffset = 0.15f;

    [Header("Ingredient Management")]
    public List<string> ingredientNames = new List<string>();
    public List<int> ingredientMaxAmounts = new List<int>();
    public List<int> ingredientCurrentAmounts = new List<int>();
    public List<Vector3> ingredientPositions = new List<Vector3>();

    // 기존 호환성
    public int stackCount = 0;
    public List<GameObject> droppedItems = new List<GameObject>();
    public List<GameObject> originalIngredients;
    public GameObject burgerPrefab;

    private GameObject instantiatedBurger;

    private void Start()
    {
        InitializeIngredients();
    }

    public bool CanAcceptDrop(IDraggable draggable)
    {
        return draggable != null;
    }

    public void OnItemDropped(IDraggable draggable)
    {
        var draggableComponent = draggable as Component;
        if (draggableComponent == null) return;

        // 1. 위치 설정
        SetDropPosition(draggableComponent.transform);

        // 2. 스프라이트 변경
        HandleSpriteChange(draggable);

        // 3. 재료 관리
        UpdateIngredients(draggable.ItemName);

        // 4. 새 클론 생성
        CreateNewClone(draggable.ItemName);

        // 5. 게임 로직
        ProcessDrop(draggable);

        // 6. 사운드
        SoundManager.instance?.PlaySFX(SoundManager.ESfx.SFX_PUT);
    }

    // 핵심: 버거 데이터 반환
    public BurgerData GetBurgerData()
    {
        var burgerData = new BurgerData();

        foreach (var item in droppedItems)
        {
            if (item != null)
            {
                string ingredientName = item.name.Replace("(Clone)", "").Trim();
                burgerData.AddIngredient(ingredientName);
            }
        }

        return burgerData;
    }

    private void SetDropPosition(Transform itemTransform)
    {
        itemTransform.SetParent(transform);
        Vector3 newPosition = transform.position;
        newPosition.y += stackCount * stackOffset;
        itemTransform.position = newPosition;

        stackCount++;
        droppedItems.Add(itemTransform.gameObject);
    }

    private void HandleSpriteChange(IDraggable draggable)
    {
        var draggableComponent = draggable as Draggable;
        if (draggableComponent == null) return;

        var spriteRenderer = draggableComponent.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        string itemName = draggable.ItemName;

        // 번 처리
        if (itemName == "Bun")
        {
            var sprite = stackCount == 1 ? draggableComponent.replacementSprite : draggableComponent.originalSprite;
            if (sprite != null) spriteRenderer.sprite = sprite;
        }

        // 소스 처리
        if (new[] { "Ketchup2", "Mayo2", "Bulgogi2" }.Contains(itemName) && draggableComponent.SauceSprite != null)
        {
            spriteRenderer.sprite = draggableComponent.SauceSprite;
        }
    }

    private void UpdateIngredients(string itemName)
    {
        int index = ingredientNames.IndexOf(itemName);
        if (index >= 0 && index < ingredientCurrentAmounts.Count)
        {
            ingredientCurrentAmounts[index]--;
        }
    }

    private void CreateNewClone(string itemName)
    {
        int index = ingredientNames.IndexOf(itemName);
        if (index == -1 || index >= ingredientPositions.Count) return;

        GameObject original = originalIngredients?.FirstOrDefault(ingredient => ingredient.name == itemName);
        if (original == null) return;

        GameObject newClone = Instantiate(original, ingredientPositions[index], original.transform.rotation);
        newClone.name = itemName;
        newClone.transform.localScale = Vector3.one * 0.2f;

        var newDraggable = newClone.GetComponent<Draggable>();
        if (newDraggable != null)
        {
            bool hasStock = ingredientCurrentAmounts[index] > 0;
            newDraggable.isDraggable = hasStock;
            newClone.GetComponent<Collider2D>().enabled = hasStock;
        }
    }

    private void ProcessDrop(IDraggable draggable)
    {
        var draggableComponent = draggable as Draggable;
        if (draggableComponent != null)
        {
            draggableComponent.isDraggable = false;
        }

        // 햄버거 완성 확인
        int bunCount = droppedItems.Count(item => item.name.Contains("Bun"));
        if (bunCount >= 3)
        {
            CreateHamburger();
        }
    }

    private void CreateHamburger()
    {
        GameObject hamburger = new GameObject("Hamburger");
        hamburger.transform.position = transform.position;
        transform.SetParent(hamburger.transform);

        foreach (var draggable in FindObjectsOfType<Draggable>())
        {
            draggable.isDraggable = false;
        }
    }

    private void InitializeIngredients()
    {
        if (ingredientNames.Count > 0) return;

        ingredientNames = new List<string> { "Bun", "Patty", "Lettuce", "Tomato", "Cheese", "Pickle", "Onion", "Ketchup2", "Mayo2", "Bulgogi2" };
        ingredientMaxAmounts = new List<int> { 10, 10, 10, 10, 10, 10, 10, 9999, 9999, 9999 };
        ingredientCurrentAmounts = new List<int>(ingredientMaxAmounts);

        ingredientPositions = new List<Vector3>
        {
            new Vector3(-7.57f, -0.8f, 0f), new Vector3(-5.05f, -0.95f, 0f), new Vector3(-2.47f, -0.9f, 0f),
            new Vector3(0, -0.92f, 0f), new Vector3(2.55f, -0.87f, 0f), new Vector3(5.04f, -0.85f, 0f),
            new Vector3(7.6f, -0.9f, 0f), new Vector3(5.892f, 1.14f, 0f), new Vector3(7, 1.14f, 0f), new Vector3(8.1f, 1.14f, 0f)
        };
    }

    // 기존 메서드들 (호환성)
    public void UpdateIngredientAmount(string ingredientName, int newAmount)
    {
        int index = ingredientNames.IndexOf(ingredientName);
        if (index != -1)
        {
            ingredientCurrentAmounts[index] = newAmount;
        }
    }

    public void MakeClonesDraggable()
    {
        foreach (var draggable in FindObjectsOfType<Draggable>())
        {
            bool inDropArea = draggable.transform.parent == transform;
            draggable.isDraggable = !inDropArea;
            draggable.GetComponent<Collider2D>().enabled = !inDropArea;
        }
    }

    public void ClearAllDroppedItems()
    {
        foreach (var item in droppedItems)
        {
            if (item != null) Destroy(item);
        }

        droppedItems.Clear();
        stackCount = 0;

        if (transform.parent?.name == "Hamburger")
        {
            var hamburger = transform.parent.gameObject;
            transform.SetParent(null);
            Destroy(hamburger);
        }

        if (instantiatedBurger != null)
        {
            Destroy(instantiatedBurger);
            instantiatedBurger = null;
        }

        foreach (var draggable in FindObjectsOfType<Draggable>())
        {
            draggable.isDraggable = true;
        }
    }
}