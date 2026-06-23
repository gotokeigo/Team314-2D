using UnityEngine;
using UnityEngine.UI;

public class ExperienceBarUI : MonoBehaviour
{
    [SerializeField] private Slider expSlider;

    private void Update()
    {
        if (ExperienceManager.Instance == null)
            return;

        expSlider.maxValue =
            ExperienceManager.Instance.NextLevelXp;

        expSlider.value =
            ExperienceManager.Instance.CurrentXp;
    }
}
