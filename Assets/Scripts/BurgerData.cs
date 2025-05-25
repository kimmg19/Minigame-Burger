using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class BurgerData
{
    public List<string> ingredients = new List<string>();

    public void AddIngredient(string ingredientName)
    {
        ingredients.Add(ingredientName);
    }

    public bool MatchesRecipe(BurgerRecipe recipe)
    {
        var requiredIngredients = recipe.GetRequiredIngredientNames();

        if (ingredients.Count != requiredIngredients.Count)
            return false;

        for (int i = 0; i < requiredIngredients.Count; i++)
        {
            if (i >= ingredients.Count || ingredients[i] != requiredIngredients[i])
                return false;
        }
        
        return true;
    }

    public float CalculateAccuracy(BurgerRecipe recipe)
    {
        var requiredIngredients = recipe.GetRequiredIngredientNames();
        if (requiredIngredients.Count == 0) return 0f;

        int correctCount = 0;
        int minLength = Mathf.Min(ingredients.Count, requiredIngredients.Count);

        for (int i = 0; i < minLength; i++)
        {
            if (ingredients[i] == requiredIngredients[i])
                correctCount++;
        }

        return (float)correctCount / requiredIngredients.Count;
    }
}