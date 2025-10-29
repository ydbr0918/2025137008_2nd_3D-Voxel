using UnityEngine;
using UnityEngine.SceneManagement;

public class BallHitResetScene : MonoBehaviour
{
    [Header("Tag to trigger reload")]
    public string triggerTag = "Monster";
    public float reloadDelay = 0.2f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(triggerTag))
            Invoke(nameof(Reload), reloadDelay);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
            Invoke(nameof(Reload), reloadDelay);
    }

    void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
