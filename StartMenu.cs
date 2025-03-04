using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("UI���")]
    public Image logoImage;           // ��ϷLOGOͼƬ
    public Image backgroundImage;     // ����ͼƬ
    public Button startGameButton;    // ��ʼ��Ϸ��ť
    public Button quitGameButton;     // ������Ϸ��ť
    public Button creditsButton;      // �����嵥��ť

    [Header("��������")]
    public string gameSceneName = "GameScene"; // ��Ϸ����������

    void Start()
    {
        // Ϊ��ť��ӵ���¼�����
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
    }

    // ��ʼ��Ϸ
    void StartGame()
    {
        // ������Ϸ����
        SceneManager.LoadScene(gameSceneName);
    }

    // �˳���Ϸ
    void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ��ʾ�����嵥
    void ShowCredits()
    {
        // ������������ʾ�����嵥���߼�
        // ���缤��һ�����������嵥�����
        Debug.Log("��ʾ�����嵥");
    }
}