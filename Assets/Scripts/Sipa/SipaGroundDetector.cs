using UnityEngine;

public class SipaGroundDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CheckPato(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckPato(collision.collider);
    }

    private void CheckPato(Collider other)
    {
        if (!other.CompareTag("Sipa"))
            return;

        if (SipaManager.Instance == null)
            return;

        if (!SipaManager.Instance.IsGameStarted)
            return;

        SipaManager.Instance.LoseGame();
    }
}