using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/BurgerRecipe", fileName = "BurgerRecipe")]
public class BurgerRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    [SerializeField] private string burgerName;
    [SerializeField] private string orderText;

    [Header("Required Ingredients (In Order)")]
    [SerializeField] private List<string> requiredIngredients = new List<string>();

    [Header("Legacy Support")]
    public List<GameObject> ingredients; // 기존 호환성

    [Header("Scoring")]
    public int perfectScore = 20;
    public int goodScore = 10;
    public int poorScore = -5;

    // 기존 메서드들
    public List<GameObject> GetRecipe() => ingredients;
    public string GetBurgerName() => burgerName;
    public string GetOrderText() => orderText;

    // 새로운 메서드들
    public List<string> GetRequiredIngredientNames()
    {
        if (requiredIngredients.Count > 0)
            return requiredIngredients;

        // Legacy 지원: GameObject 이름에서 추출
        return ingredients?.Select(go => go.name).ToList() ?? new List<string>();
    }

    public bool MatchesExactly(BurgerData burger)
    {
        return burger.MatchesRecipe(this);
    }

    public float CalculateAccuracy(BurgerData burger)
    {
        return burger.CalculateAccuracy(this);
    }
}