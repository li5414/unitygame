﻿using SDK.Common;
using System.Collections.Generic;
namespace SDK.Lib
{
    /**
     * @brief 自动更新系统
     */
    public class AutoUpdateSys
    {
        public List<string> m_loadingPath = new List<string>();
        public List<string> m_loadedPath = new List<string>();
        public List<string> m_failedPath = new List<string>();

        public void loadMiniVersion()
        {
            Ctx.m_instance.m_versionSys.m_miniLoadResultDisp = miniVerLoadResult;
            Ctx.m_instance.m_versionSys.m_LoadResultDisp = verLoadResult;
            Ctx.m_instance.m_versionSys.loadMiniVerFile();
        }

        public void miniVerLoadResult(bool needUpdate)
        {
            Ctx.m_instance.m_versionSys.loadVerFile();
        }

        public void verLoadResult()
        {
            if(Ctx.m_instance.m_versionSys.m_needUpdateVer) // 如果需要更新
            {
                // 开始正式加载文件
                loadAllUpdateFile();
            }
        }

        public void loadAllUpdateFile()
        {
            foreach (KeyValuePair<string, FileVerInfo> kv in Ctx.m_instance.m_versionSys.m_webVer.m_path2HashDic)
            {
                if(Ctx.m_instance.m_versionSys.m_localVer.m_path2HashDic.ContainsKey(kv.Key))
                {
                    if(Ctx.m_instance.m_versionSys.m_localVer.m_path2HashDic[kv.Key].m_fileMd5 != kv.Value.m_fileMd5)
                    {
                        loadOneUpdateFile(kv.Key, kv.Value);
                    }
                }
            }
        }

        public void loadOneUpdateFile(string path, FileVerInfo fileInfo)
        {
            m_loadingPath.Add(path);
            UtilApi.delFileNoVer(path);     // 删除当前目录下已经有的 old 文件

            LoadParam param = Ctx.m_instance.m_poolSys.newObject<LoadParam>();
            param.m_path = path;

            param.m_resLoadType = ResLoadType.eLoadWeb;
            param.m_version = fileInfo.m_fileMd5;

            param.m_loaded = onLoaded;
            param.m_failed = onFailed;

            Ctx.m_instance.m_resLoadMgr.loadData(param);
            Ctx.m_instance.m_poolSys.deleteObj(param);
        }

        protected void onLoaded(IDispatchObject resEvt)
        {
            m_loadedPath.Add((resEvt as DataResItem).path);
            m_loadingPath.Remove((resEvt as DataResItem).path);

            if(m_loadingPath.Count == 0)
            {
                onUpdateEnd();
            }
        }

        protected void onFailed(IDispatchObject resEvt)
        {
            m_failedPath.Add((resEvt as DataResItem).path);
            m_loadingPath.Remove((resEvt as DataResItem).path);

            if (m_loadingPath.Count == 0)
            {
                onUpdateEnd();
            }
        }

        protected void onUpdateEnd()
        {
            // 进入游戏
        }
    }
}