using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单UI管理器
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // 主菜单状态枚举
    private enum MenuState { Start, LevelSelect, CharacterSelect }
    private MenuState currentState = MenuState.Start;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject levelSelectPanel;
    public GameObject characterSelectPanel;

    void Start()
    {
        // 触发播放主菜单音乐
        this.TriggerEvent(EventName.PlayMusic, new MusicEventArgs { musicType = MusicType.MainMenu });
        ShowPanel(MenuState.Start);
    }

    void Update()
    {
        // 监听 ESC 键返回逻辑
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == MenuState.LevelSelect)
            {
                ShowPanel(MenuState.Start);
            }
            else if (currentState == MenuState.CharacterSelect)
            {
                ShowPanel(MenuState.LevelSelect);
            }
        }
    }

    // 切换面板显示
    private void ShowPanel(MenuState newState)
    {
        currentState = newState;
        startPanel.SetActive(currentState == MenuState.Start);
        levelSelectPanel.SetActive(currentState == MenuState.LevelSelect);
        characterSelectPanel.SetActive(currentState == MenuState.CharacterSelect);
    }

    // StartPanel 按钮绑定的方法
    public void OnClickStartGame()
    {
        ShowPanel(MenuState.LevelSelect);
    }

    public void OnClickExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // LevelSelectPanel 按钮绑定的方法
    public void OnClickSelectMap(string mapName)
    {
        // 将选择的地图保存到全局数据中
        GameData.SelectedMapName = mapName;
        ShowPanel(MenuState.CharacterSelect);
    }

    // CharacterSelectPanel 按钮绑定的方法
    public void OnClickSelectCharacter(int characterIndex)
    {
        // 将选择的角色索引保存到全局数据中
        GameData.SelectedPlayerIndex = characterIndex;

        // 开始加载对应的地图场景
        Debug.Log($"准备进入 {GameData.SelectedMapName}，操作角色索引：{characterIndex}");
        SceneManager.LoadScene(GameData.SelectedMapName);
    }
}
