using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class GameBalanceManager : MonoBehaviour
{
    [SerializeField]
    private Category economy;

    [SerializeField]
    private Category people;

    [SerializeField]
    private Category military;

    [SerializeField]
    private Category religion;

    public Category lastCategory;

    public TextMeshPro EconomyFeedBack3D;
    public TextMeshPro PeopleFeedBack3D;
    public TextMeshPro MilitaryFeedBack3D;
    public TextMeshPro ReligionFeedBack3D;

    private float timeProgressionFactor = 1.0f;
    private float gameTime;

    [SerializeField]
    private int basePointsToAdd = 3;

    void Update()
    {
        gameTime += Time.deltaTime;
        UpdateTimeProgressionFactor();
    }

    void UpdateTimeProgressionFactor()
    {
        // Decrease factor over time to increase randomness
        timeProgressionFactor = Mathf.Max(0.1f, 1.0f - gameTime / 10000.0f);
    }

    public (Category, Category) SelectDilemmaCategories()
    {
        float mean = CalculateMean();
        List<Category> categories = new List<Category>() { economy, people, military, religion };

        // Randomly select the first category, influenced by deviation from mean and timeProgressionFactor
        Category firstCategory = ChooseCategoryBasedOnDeviation(categories, mean);

        // Ensure the second category is different from the first
        Category secondCategory = ChooseCategoryBasedOnDeviation(categories.Where(c => c != firstCategory).ToList(), mean);
        lastCategory=secondCategory;
        print("nig " + firstCategory.Type + " " + secondCategory.Type);

        return (firstCategory, secondCategory);
    }

    Category ChooseCategoryBasedOnDeviation(List<Category> categories, float mean)
    {
        if (Random.value > timeProgressionFactor)
        {
            print("nig- foi random");
            return categories[Random.Range(0, categories.Count)];
        }
        else
        {
            
            var tempCat=categories.OrderByDescending(c => Mathf.Abs(c.Points - mean)).First();
            if (lastCategory != tempCat)
            {
                
            } 
            else
            {
                tempCat=categories[Random.Range(0, categories.Count)];
            }
            
            return tempCat;

        }
    }

    float CalculateMean()
    {
        return (economy.Points + people.Points + military.Points + religion.Points) / 4;
    }

    private void ApplyPointsBalance(Category impactCategory, bool isIncrement)
    {
        int randomFactor = 2;

        // Adjusting points with a mix of fixed and dynamic changes
        int change = Mathf.CeilToInt(basePointsToAdd);

         

        if (isIncrement && impactCategory.Points > 75)
        {
            change = Mathf.Max(1, change / 2); // Less benefit if high
        }
        else if (!isIncrement && impactCategory.Points < 10)
        {
            change = Mathf.Max(1, change / 2); // Less prejudice if low
        }

        change = change + Random.Range(-2, 3);

        if (isIncrement)
        {
            impactCategory.AddRemovePoints(change);
            impactCategory.AddBuildings(change);
            ShowPointsChange(impactCategory,"+");
        }
        else
        {
            impactCategory.AddRemovePoints(-change);
            impactCategory.RemoveBuildings(change);
            ShowPointsChange(impactCategory,"-");
        }

        CheckForEndGameConditions();
    }

    public void ApplyIncrementPointsBalance(Category positiveImpactCategory)
    {
        ApplyPointsBalance(positiveImpactCategory, true);
    }

    public void ApplyDecrementPointsBalance(Category negativeImpactCategory)
    {
        ApplyPointsBalance(negativeImpactCategory, false);
    }
    private void CheckForEndGameConditions()
    {
        Debug.Log("Game over conditions not implemented");
    }

    public void ShowPointsChange(Category category, string sign)
    {
        TextMeshPro feedbackTextMesh = null;

        switch (category.Type)
        {
            case CategoryType.Economy:
                feedbackTextMesh = EconomyFeedBack3D;
                break;
            case CategoryType.People:
                feedbackTextMesh = PeopleFeedBack3D;
                break;
            case CategoryType.Military:
                feedbackTextMesh = MilitaryFeedBack3D;
                break;
            case CategoryType.Religion:
                feedbackTextMesh = ReligionFeedBack3D;
                break;
        }

        if (feedbackTextMesh != null)
        {
            feedbackTextMesh.text = sign;
            feedbackTextMesh.gameObject.SetActive(true);
            StartCoroutine(ScaleText(feedbackTextMesh, 0.3f, 1.5f)); // Scale up
            StartCoroutine(HidePointsChangeFeedback(feedbackTextMesh));
        }
    }

    private IEnumerator ScaleText(TextMeshPro textMesh, float startScale, float endScale, float duration = 0.5f)
    {
        float currentTime = 0;
        Vector3 initialScale = Vector3.one * startScale;
        Vector3 finalScale = Vector3.one * endScale;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration;
            textMesh.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);
            yield return null;
        }
        textMesh.transform.localScale = finalScale;
    }

    private IEnumerator HidePointsChangeFeedback(TextMeshPro feedbackTextMesh)
    {
        yield return new WaitForSeconds(4f); // Wait for 2 seconds
        StartCoroutine(ScaleText(feedbackTextMesh, 1.5f, 0.3f)); // Scale down
        yield return new WaitForSeconds(0.5f); // Additional wait for scaling down
        feedbackTextMesh.gameObject.SetActive(false);
    }
}

