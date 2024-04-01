using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static Building passedBuildingData = new Building();

    public static void changeScene(string scene)
    {
        SceneManager.LoadSceneAsync(scene);
    }

    public void Quit()
    {
        Application.Quit();
    }

}
