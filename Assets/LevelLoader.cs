using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    public float transitionTime = 1f;

    public Animator transition;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        // play animation
        transition.SetTrigger("Start");

        // wait for animation to finish
        yield return new WaitForSeconds(transitionTime);

        // load the level
        SceneManager.LoadScene(levelIndex);
    }
}
