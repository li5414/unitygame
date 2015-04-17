using SDK.Common;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace SDK.Lib
{
    public class LangMgr
    {
        protected LangID m_langID = LangID.zh_CN;           // 当前语言，默认简体中文
        protected XmlNodeList m_nodeList = null;                   // 整个的 xml 中 <t> 列表
        protected Dictionary<LangID, LangAttrItem> m_ID2FileName = new Dictionary<LangID, LangAttrItem>();  // 语言到文件名字的映射
        protected XmlNodeList m_tmpEleList;         // 临时的元素列表
        protected XmlElement m_tmpEle;              // 临时的元素
        protected bool m_isLoaded = false;                  // 语言文件是否加载
        protected bool m_hasItem = false;

        public LangMgr()
        {
            m_ID2FileName[LangID.zh_CN] = new LangAttrItem();
            m_ID2FileName[LangID.zh_CN].m_filePath = Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathLangXml] + "zh_CN.xml";
        }

        public void getText(LangTypeId typeId, int itemIdx)
        {
            if (!m_isLoaded)
            {
                loadXml();
            }

            m_hasItem = false;

            if(null != m_nodeList)
            {
                if ((int)typeId < m_nodeList.Count)
                {
                    m_tmpEleList = m_nodeList[(int)typeId].ChildNodes as XmlNodeList;
                    if(itemIdx < m_tmpEleList.Count)
                    {
                        m_hasItem = true;
                        m_tmpEle = m_tmpEleList[itemIdx] as XmlElement;
                        Ctx.m_instance.m_shareData.m_retLangStr = m_tmpEle.InnerText;
                    }
                }
            }

            if (!m_hasItem)
            {
                Ctx.m_instance.m_shareData.m_retLangStr = "default string";
            }
        }

        //<?xml version="1.0" encoding="utf-8"?>
        //<!-- type 就是一个功能 item 就是一项，顺序千万不要乱，否则都乱了  -->
        //<msg>
        //    <t>
        //        <i>数据结构</i>
        //    </t>
        //</msg>
        public void loadXml()
        {
            m_isLoaded = true;
            LoadParam param = Ctx.m_instance.m_poolSys.newObject<LoadParam>();
            param.m_loadNeedCoroutine = false;
            param.m_resNeedCoroutine = false;
            param.m_path = Ctx.m_instance.m_pPakSys.getCurResPakPathByResPath(m_ID2FileName[m_langID].m_filePath);
            param.m_loaded = onLoaded;
            param.m_failed = onFailed;
            Ctx.m_instance.m_resLoadMgr.loadResources(param);
            Ctx.m_instance.m_poolSys.deleteObj(param);
        }

        // 加载一个表完成
        public void onLoaded(IDispatchObject resEvt)
        {
            IResItem res = resEvt as IResItem;                         // 类型转换
            //Ctx.m_instance.m_log.debugLog_1(LangItemID.eItem0, res.GetPath());    // 这行执行的时候 m_isLoaded 设置加载标志，但是 m_nodeList 还没有初始化
            Ctx.m_instance.m_log.log("local xml loaded");

            string text = res.getText(m_ID2FileName[m_langID].m_filePath);
            if (text != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(text);

                XmlNode xn = xmlDoc.SelectSingleNode("msg");
                m_nodeList = xn.ChildNodes;
            }

            // 卸载资源
            Ctx.m_instance.m_resLoadMgr.unload(res.GetPath());
        }

        public void onFailed(IDispatchObject resEvt)
        {
            IResItem res = resEvt as IResItem;
            Ctx.m_instance.m_log.debugLog_1(LangItemID.eItem1, res.GetPath());

            // 卸载资源
            Ctx.m_instance.m_resLoadMgr.unload(res.GetPath());
        }
    }
}