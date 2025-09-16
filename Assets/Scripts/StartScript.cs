using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("GameScene");
        }
    }
    

}
