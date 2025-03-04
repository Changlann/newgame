using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("UI组件")]
    public Image logoImage;           // 游戏LOGO图片
    public Image backgroundImage;     // 背景图片
    public Button startGameButton;    // 开始游戏按钮
    public Button quitGameButton;     // 结束游戏按钮
    public Button creditsButton;      // 制作清单按钮

    [Header("场景设置")]
    public string gameSceneName = "GameScene"; // 游戏场景的名称

    void Start()
    {
        // 为按钮添加点击事件监听
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
    }

    // 开始游戏
    void StartGame()
    {
        // 加载游戏场景
        SceneManager.LoadScene(gameSceneName);
    }

    // 退出游戏
    void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 显示制作清单
    void ShowCredits()
    {
        // 这里可以添加显示制作清单的逻辑
        // 比如激活一个包含制作清单的面板
        Debug.Log("显示制作清单");
    }
}