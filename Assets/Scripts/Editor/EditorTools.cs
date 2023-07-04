using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑工具
/// </summary>
public class EditorTools : EditorWindow
{
    private static string mToDeleteKey = "";
    private static string mCurOID = "";

    [MenuItem("工具/清理账号")]
    static void CleanAccount()
    {
        PlayerPrefs.DeleteKey("oid");
    }


    [MenuItem("工具/清理所有PlayerPrefs")]
    static void CleanAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    

    public class sAtlasInfoList
    {
        public List<sAtlasInfo> atlas = new List<sAtlasInfo>();
    }

    public class sAtlasInfo
    {
        public string name;
        public string bundle;
    }

    static private List<sAtlasInfo> GetAtlasByName(string name, sAtlasInfoList list)
    {
        List<sAtlasInfo> infos = new List<sAtlasInfo>();
        foreach (var info in list.atlas)
        {
            if (info.name == name)
            {
                infos.Add(info);
            }
        }
        return infos;
    }

    [MenuItem("工具/工具集合")]
    static void CleanOIDPlayerPrefs()
    {
        EditorTools.OpenToolSetWindow();
    }
    public static void OpenToolSetWindow()
    {
        //Method to open the Window
        var window = GetWindow<EditorTools>("工具集合");
        window.minSize = new Vector2(350, 650);
        window.maxSize = new Vector2(355, 655);
        var position = window.position;
        position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;
        mCurOID = PlayerPrefs.GetString("oid");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.HelpBox("通用简单工具都在此", MessageType.None);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal("box");
        mCurOID = GUILayout.TextField(mCurOID);
        if (GUILayout.Button("设置账号"))
        {
            PlayerPrefs.SetString("oid", mCurOID);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        mToDeleteKey = GUILayout.TextField(mToDeleteKey);
        if (GUILayout.Button("删除"))
        {
            PlayerPrefs.DeleteKey(mToDeleteKey);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}