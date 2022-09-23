using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Jyx2.Middleware;
using Jyx2.ResourceManagement;
using Plugins.Auxiliary.AndroidBridge;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

namespace Jyx2
{
    /// <summary>
    /// 游戏运行时的初始化
    /// </summary>
    public static class RuntimeEnvSetup
    {
        private static bool _isSetup;
        public static MODRootConfig CurrentModConfig { get; set; } = null;
        public static string CurrentModId { get; set; } = "";
        
        public static async UniTask Setup()
        {
            if (_isSetup) return;
            
            _isSetup = true;
            
            DebugInfoManager.Init();
            
            // 全局配置表
            var t = Resources.Load<GlobalAssetConfig>("GlobalAssetConfig");
            if (t != null)
            {
                GlobalAssetConfig.Instance = t;
                await t.OnLoad();
            }

            // 加载当前Mod
            await LoadCurrentMod();
            
            // 增加persistentDataPath下的Mods文件夹作为Mod读取路径
            GlobalAssetConfig.Instance.localModPath.Add(Path.Combine(Application.persistentDataPath,"Mods"));
            // 安卓部分权限获取，用于MOD加载
            if (Application.platform == RuntimePlatform.Android)
            {
                // 调用弹窗显示
                AndroidTools.ShowToast($"当前加载MOD：{CurrentModId}");
                // 获取安卓应用程序权限
                await GetAndroidPermissions();
                // 调用文件夹选择一个目录作为MOD存放目录
                AndroidTools.PickDirectory(path =>
                {
                    // 增加MOD读取路径
                    GlobalAssetConfig.Instance.localModPath.Add(path);
                    Debug.Log("本地MOD读取路径：" + string.Join(";", GlobalAssetConfig.Instance.localModPath.ToArray()));
                });
            }
            // 针对Windows单独设定路径
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
            {
                // TODO:此处需要增加steam的创意工坊路径
                // GlobalAssetConfig.Instance.localModPath.Add( thePathToSteamMod);
            }
            // 读取路径去重
            GlobalAssetConfig.Instance.localModPath = GlobalAssetConfig.Instance.localModPath.Distinct().ToList();

            await ResLoader.Init();
            await ResLoader.LoadMod(CurrentModId); 
            
            CurrentModConfig = await ResLoader.LoadAsset<MODRootConfig>("Assets/ModSetting.asset");
            
            /*
#if UNITY_EDITOR
            foreach (var path in GlobalAssetConfig.Instance.localModPath)
            {
                var dirPath = $"{path}/Mods/{CurrentModId}/Configs";
                if (!File.Exists($"{dirPath}/Datas.bytes"))
                {
                    CurrentModConfig.GenerateConfigs();
                }
                else
                {
                    ExcelTools.WatchConfig(dirPath, () =>
                    {
                        CurrentModConfig.GenerateConfigs();
                        Debug.Log("File Watcher! Reload success! -> " + dirPath);
                    }); 
                }
            }
#endif
*/
      
            GameSettingManager.Init();
            await Jyx2ResourceHelper.Init();
        }
        
        

        private static async UniTask LoadCurrentMod()
        {
            if (PlayerPrefs.HasKey("CURRENT_MOD"))
            {
                CurrentModId = PlayerPrefs.GetString("CURRENT_MOD");
            }
            else
            {
#if UNITY_EDITOR
                var path = SceneManager.GetActiveScene().path;
                if (path.Contains($"{Application.persistentDataPath}/Mods/"))
                {

                    CurrentModId = path.Split('/')[2];
                }
                else
                {
                    CurrentModId = GlobalAssetConfig.Instance.startModId;
                }
#else
                CurrentModId = GlobalAssetConfig.Instance.startModId;
#endif
            }
        }

        /// <summary>
        /// 获取安卓应用程序的权限，在游戏启动前进行触发
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async UniTask GetAndroidPermissions()
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            Permission.RequestUserPermission("android.permission.MANAGE_EXTERNAL_STORAGE");
            // 获取文件权限
            AndroidTools.GetFileAccessPermission();
        }
    }
}