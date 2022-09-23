using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jyx2;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MOD
{
    public class ModPanelNew : Jyx2_UIBase
    {
        public Dropdown m_Dropdown;
        public GameObject ModChangedSuggestLabel;

        public void Start()
        {
            m_Dropdown.onValueChanged.RemoveAllListeners();
            m_Dropdown.onValueChanged.AddListener(OnValueChanged);
            m_Dropdown.ClearOptions();
            m_Dropdown.AddOptions(LoadLocalModList());
            m_Dropdown.value = m_Dropdown.options.FindIndex(o => o.text == RuntimeEnvSetup.CurrentModId);
        
            ModChangedSuggestLabel.gameObject.SetActive(false);
        }


        /// <summary>
        /// 获取本地MOD列表
        /// </summary>
        /// <returns></returns>
        public List<string> LoadLocalModList()
        {
            List<string> modsList = new();
            foreach (var path in GlobalAssetConfig.Instance.localModPath)
            {
                var direction = new DirectoryInfo(path);
                var folders = direction.GetDirectories("*", SearchOption.TopDirectoryOnly);
                modsList.AddRange(folders.Select(t => t.Name));
            }

            return modsList;
        }

        public void OnClose()
        {
            var selectMod = m_Dropdown.options[m_Dropdown.value].text;
            if(selectMod != RuntimeEnvSetup.CurrentModId)
            {
                PlayerPrefs.SetString("CURRENT_MOD", selectMod);
                PlayerPrefs.Save();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
            this.Hide();
        }

        protected override void OnCreate()
        {
        
        }

        void OnValueChanged(int index)
        {
            // 切换Mod
            var selectMod = m_Dropdown.options[m_Dropdown.value].text;
            ModChangedSuggestLabel.gameObject.SetActive(selectMod != RuntimeEnvSetup.CurrentModId);
        }
    
    
        public void OnUploadMod()
        {
        
        }

        public void OnOpenURL(string url)
        {
            Jyx2.Middleware.Tools.openURL(url);
        }


        public void OnOpenSteamWorkshop()
        {
        
        }
    }
}
