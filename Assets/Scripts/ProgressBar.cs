using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private Image _progressBarImage;

    private Category _parentCategory;

    [SerializeField]
    private GameObject _debugPointsText;
    private void Start()
    {
        _parentCategory = GetComponentInParent<Category>();
        if (_parentCategory == null)
        {
            Debug.LogError("Category component not found on the parent object.");
        }
    }

    private void Update()
    {
        if (_parentCategory != null)
        {
            UpdateProgressBar(_parentCategory.Points, _parentCategory.MaxPoints);
        }
    }

    public void UpdateProgressBar(float points, float maxPoints)
    {
        if (maxPoints != 0)
        {
            _progressBarImage.fillAmount = 0.1f+ (points / maxPoints);
        }
        else
        {
            Debug.LogError("MaxPoints is zero, cannot update progress bar.");
        }

        if (_debugPointsText != null)
        {
            TextMeshProUGUI tmp = _debugPointsText.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = points.ToString();
            }
            else
            {
                Debug.LogError("_debugPointsText does not have a TextMeshProUGUI component.");
            }
        }
        else
        {
            Debug.LogError("_debugPointsText is not initialized.");
        }
    }
}