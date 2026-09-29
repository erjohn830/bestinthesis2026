using UnityEngine;

public class GameCarousel : MonoBehaviour
{
    public RectTransform carousel;
    public float panelWidth = 1920f;

    int currentIndex = 0;

    public void NextPanel()
    {
        currentIndex++;
        if (currentIndex > 2)
            currentIndex = 2;

        Move();
    }

    public void PreviousPanel()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = 0;

        Move();
    }

    void Move()
    {
        carousel.anchoredPosition =
            new Vector2(-currentIndex * panelWidth, 0);
    }
}