using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTool
{
    public class EditorConfig
    {
        public static string PREFAB_PATH = "";
        public static string ASSETBUNDLE = "";
        public static string OUTPUT_PATH = "";
    }

    public class ExportAssetBundleName : Editor
    {
        [MenuItem("Tool/SetFileBundleName")]
        static public void SetBundleName()
        {
            // ������Դ��AssetBundle�����ƺ��ļ���չ��
            UnityEngine.Object[] selects = Selection.objects;
            foreach (UnityEngine.Object selected in selects)
            {
                string path = AssetDatabase.GetAssetPath(selected);
                AssetImporter asset = AssetImporter.GetAtPath(path);
                asset.assetBundleName = selected.name; //����Bundle�ļ�������
                asset.assetBundleVariant = "unity3d";//����Bundle�ļ�����չ��
                asset.SaveAndReimport();

            }
            AssetDatabase.Refresh();
        }

        // ����assetbundle������(�޸�meta�ļ�)
        [MenuItem("Tools/SetAssetBundleName")]
        static void OnSetAssetBundleName()
        {
            UnityEngine.Object obj = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string[] extList = new string[] { ".prefab.meta", ".png.meta", ".jpg.meta", ".tga.meta" };
            //EditorUtil.Walk(path, extList, DoSetAssetBundleName);

            //ˢ�±༭��
            AssetDatabase.Refresh();
            Debug.Log("AssetBundleName�޸����");
        }
        
        static void DoSetAssetBundleName(string path)
        {
            path = path.Replace("\\", "/");
            int index = path.IndexOf(EditorConfig.PREFAB_PATH);
            string relativePath = path.Substring(path.IndexOf(EditorConfig.PREFAB_PATH) + EditorConfig.PREFAB_PATH.Length);
            string prefabName = relativePath.Substring(0, relativePath.IndexOf('.')) + EditorConfig.ASSETBUNDLE;
            StreamReader fs = new StreamReader(path);
            List<string> ret = new List<string>();
        
            string line;
            while((line = fs.ReadLine()) != null) {
                line = line.Replace("\n", "");
                if (line.IndexOf("assetBundleName:") != -1) {
                    line = "  assetBundleName: " + prefabName.ToLower();
                }
        
                ret.Add(line);
            }
        
            fs.Close();
            File.Delete(path);
        
            StreamWriter writer = new StreamWriter(path + ".tmp");
            foreach (var each in ret) {
                writer.WriteLine(each);
            }
            writer.Close();

            File.Copy(path + ".tmp", path);
            File.Delete(path + ".tmp");
        }
        
        [MenuItem("Tools/CreateAssetBundle")]
        static void OnCreateAssetBundle()
        {
            BuildPipeline.BuildAssetBundles(EditorConfig.OUTPUT_PATH);
            //ˢ�±༭��
            AssetDatabase.Refresh();
            Debug.Log("AssetBundle������");
        }
    }
}