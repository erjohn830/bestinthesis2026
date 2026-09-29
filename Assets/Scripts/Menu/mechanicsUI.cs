using UnityEngine;

public class MechanicsUI : MonoBehaviour
{
    public GameObject luksongBakaMechanics;
    public GameObject patinteroMechanics;
    public GameObject iceIceWaterMechanics;

    void Start()
    {
        CloseAll();
    }

    void CloseAll()
    {
        if (luksongBakaMechanics != null)
            luksongBakaMechanics.SetActive(false);

        if (patinteroMechanics != null)
            patinteroMechanics.SetActive(false);

        if (iceIceWaterMechanics != null)
            iceIceWaterMechanics.SetActive(false);
    }

    public void ShowLuksongBaka()
    {
        CloseAll();
        luksongBakaMechanics.SetActive(true);
    }

    public void ShowPatintero()
    {
        CloseAll();
        patinteroMechanics.SetActive(true);
    }

    public void ShowIceIceWater()
    {
        CloseAll();
        iceIceWaterMechanics.SetActive(true);
    }

    public void CloseMechanics()
    {
        CloseAll();
    }
}