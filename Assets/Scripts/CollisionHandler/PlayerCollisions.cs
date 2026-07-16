using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public GameManager GameManager;
    public GameObject blood;
    public GameObject explosion;
    public GameObject GameOverMenuUI;

    void OnTriggerEnter2D(Collider2D collisionInfo)
    {
        if (collisionInfo.CompareTag("Bullet"))
        {
            Debug.Log("DED");
            Destroy (gameObject);
            Instantiate (explosion, transform.position, Quaternion.identity);
            Instantiate (blood, transform.position, Quaternion.identity);
            FindObjectOfType<GameManager>().EndGame();
            GameOverMenuUI.SetActive(true);
        }
    }
}
