using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


//EditorWindowを継承することでUnityのエディター上にウィンドウを作れるようになる
public class SceneSwitcher : EditorWindow
{
    [MenuItem("Tools/シーン切り替え君")]
    public static void ShowWindow()
    {
        GetWindow<SceneSwitcher>("シーン切り替え君");
    }

    private void OnGUI()
    {
        GUILayout.Label("シーン一覧", EditorStyles.boldLabel);

        //BuildSettingに登録されているシーンを取得
        foreach (var scene in EditorBuildSettings.scenes)
        {
            //シーン名だけ取り出す
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);

            if (GUILayout.Button(sceneName))
            {
                //保存確認してからシーンを開く
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scene.path);
                }
            }
        }
    }
}
