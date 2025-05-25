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
    public List<GameObject> ingredients; // ���� ȣȯ��

    [Header("Scoring")]
    public int perfectScore = 20;
    public int goodScore = 10;
    public int poorScore = -5;

    // ���� �޼����
    public List<GameObject> GetRecipe() => ingredients;
    public string GetBurgerName() => burgerName;
    public string GetOrderText() => orderText;

    // ���ο� �޼����
    public List<string> GetRequiredIngredientNames()
    {
        if (requiredIngredients.Count > 0)
            return requiredIngredients;

        // Legacy ����: GameObject �̸����� ����
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