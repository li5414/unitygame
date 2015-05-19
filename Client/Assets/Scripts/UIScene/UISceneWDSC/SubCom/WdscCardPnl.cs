﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SDK.Common;
using SDK.Lib;

namespace Game.UI
{
    public enum WdscCardPnl_BtnIndex
    {
        eBtnPre,
        eBtnNext,

        eBtnTotal,
    }

    /// <summary>
    /// 我的收藏卡牌显示面板
    /// </summary>
    public class WdscCardPnl : InterActiveEntity
    {
        public SceneWDSCData m_sceneWDSCData;
        public UIGrid m_CardList = new UIGrid();            // 收藏卡牌数据
        public List<SCUICardItemCom> m_SCUICardItemList = new List<SCUICardItemCom>();

        public PageInfo[] m_pageArr = new PageInfo[(int)EnPlayerCareer.ePCTotal];

        public List<CardItemBase>[] m_filterCardListArr = new List<CardItemBase>[(int)EnPlayerCareer.ePCTotal];      // 每一个职业一个列表，过滤后的数据，主要用来显示
        public int m_filterMp = 8;          // 过滤的 Mp 值
        public Text m_textPageNum;

        protected Button[] m_btnArr = new Button[(int)LeftBtnPnl_BtnIndex.eBtnJobTotal];

        public int filterMp
        {
            get
            {
                return m_filterMp;
            }
            set
            {
                m_filterMp = value;
                buildFilterList();     // 生成过滤列表
                destroyAndUpdateCardList();
            }
        }

        public override void Awake()
        {
            int idx = 0;
            while (idx < (int)EnPlayerCareer.ePCTotal)
            {
                m_pageArr[idx] = new PageInfo();
                ++idx;
            }
        }

        public override void Start()
        {
            m_sceneWDSCData.m_onClkCard = onClkCard;

            m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnPre] = UtilApi.getComByP<Button>(m_sceneWDSCData.m_sceneUIGo, SceneSCPath.BtnPrePage);
            m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnNext] = UtilApi.getComByP<Button>(m_sceneWDSCData.m_sceneUIGo, SceneSCPath.BtnNextPage);

            UtilApi.addEventHandle(m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnPre], onPreBtnClk);       // 前一页
            UtilApi.addEventHandle(m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnNext], onNextBtnClk);       // 后一页

            m_CardList.setGameObject(UtilApi.GoFindChildByPObjAndName("wdscjm/page/CardList"));
            m_CardList.cellWidth = 0.00473f;
            m_CardList.cellHeight = 0.00663f;
            m_CardList.maxPerLine = 4;

            // 当前页号
            m_textPageNum = UtilApi.getComByP<Text>(m_sceneWDSCData.m_sceneUIGo, SceneSCPath.TextPageNum);

            int idx = 0;
            for(idx = 0; idx < (int)EnPlayerCareer.ePCTotal; ++idx)
            {
                m_filterCardListArr[idx] = new List<CardItemBase>();
            }
        }

        /// <summary>
        /// 销毁8张牌
        /// </summary>
        protected void destroyCrad()
        {
            GameObject go;
            for (int i = m_CardList.getChildCount() - 1; i >= 0; i--)
            {
                go = m_CardList.GetChild(i).gameObject;
                UtilApi.Destroy(go);
            }

            m_SCUICardItemList.Clear();
        }

        public void destroyAndUpdateCardList()
        {
            destroyCrad();
            updateCardList();
            updatePreNextBtnState();
            updatePageNo();
        }

        protected void updateCardList()
        {
            GameObject tmpGO;
            GameObject go;

            if (m_filterCardListArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].Count > 0)
            {
                int idx = 0;
                CardItemBase cardItem;
                SCUICardItemCom uicardItem;
                while (bInRangeByPageAndIdx(m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx, idx))
                {
                    cardItem = m_filterCardListArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx][m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].m_curPageIdx * (int)SCCardNumPerPage.eNum + idx];

                    tmpGO = Ctx.m_instance.m_modelMgr.getSceneCardModel((CardType)cardItem.m_tableItemCard.m_type).getObject() as GameObject;
                    if (tmpGO != null)
                    {
                        go = UtilApi.Instantiate(tmpGO) as GameObject;
                        m_CardList.AddChild(go.transform);
                        UtilApi.normalPos(go.transform);
                        go.transform.localScale = new Vector3(0.003661138f, 0.003661138f, 0.003661138f);
                        uicardItem = new SCUICardItemCom();
                        uicardItem.setGameObject(go);
                        uicardItem.cardItemBase = cardItem;
                        m_SCUICardItemList.Add(uicardItem);
                        m_SCUICardItemList[m_SCUICardItemList.Count - 1].m_clkCB = m_sceneWDSCData.m_onClkCard;

                        UtilApi.updateCardDataNoChange(cardItem.m_tableItemCard, uicardItem.getGameObject());
                        UtilApi.updateCardDataChange(cardItem.m_tableItemCard, uicardItem.getGameObject());
                    }
                    ++idx;
                }

                m_CardList.Reposition();
            }
        }

        public void updatePreNextBtnState()
        {
            if(canMovePre())
            {
                m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnPre].interactable = true;
            }
            else
            {
                m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnPre].interactable = false;
            }

            if (canMoveNext())
            {
                m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnNext].interactable = true;
            }
            else
            {
                m_btnArr[(int)WdscCardPnl_BtnIndex.eBtnNext].interactable = false;
            }
        }

        public void updatePageNo()
        {
            m_textPageNum.text = string.Format("第{0}/{1}页", m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].m_curPageIdx + 1, m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].getTotalPageDesc());
        }

        // 收藏中前一页
        public void onPreBtnClk()
        {
            if (m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].canMovePreInCurTagPage())      // 如果当前 TabPage 没有到开始
            {
                --m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].m_curPageIdx;
                destroyAndUpdateCardList();
            }
            else
            {
                int curTagPage = getPreTagPage();
                if (bInTabPageRang(curTagPage))
                {
                    int curTagPageCnt = m_pageArr[curTagPage].totalPage;
                    m_pageArr[curTagPage].m_curPageIdx = curTagPageCnt - 1;      // 最后一页
                    m_sceneWDSCData.m_leftBtnPnl.updateByCareer(curTagPage);
                }
            }
        }

        // 收藏中后一页
        public void onNextBtnClk()
        {
            if (m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].canMoveNextInCurTagPage())
            {
                ++m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].m_curPageIdx;
                destroyAndUpdateCardList();
            }
            else
            {
                int curTagPage = getNextTagPage();
                if (bInTabPageRang(curTagPage))
                {
                    m_pageArr[curTagPage].m_curPageIdx = 0;      // 第一页
                    m_sceneWDSCData.m_leftBtnPnl.updateByCareer(curTagPage);
                }
            }
        }

        // 判断当前 TabPage 是否可以向前翻页
        public bool canMovePre()
        {
            if (m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].canMovePreInCurTagPage())       // 当前页就可以移动
            {
                return true;
            }
            else        // 当前页不能移动，需要跨页移动
            {
                int curTagPage = getPreTagPage();
                if(bInTabPageRang(curTagPage))
                {
                    return true;
                }
            }

            return false;
        }

        // 是否可以移动到之前的 TabPage 
        protected int getPreTagPage()
        {
            int curTagPage = m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx;
            while (--curTagPage >= 0)   // 如果 TaPage 前面还有
            {
                if (m_filterCardListArr[curTagPage].Count > 0)     // 如果当前页面有内容
                {
                    break;
                }
            }

            return curTagPage;
        }

        // 判断当前的 TabPage 是否可以向后翻页
        public bool canMoveNext()
        {
            if (m_pageArr[m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx].canMoveNextInCurTagPage())
            {
                return true;
            }

            int curTagPage = getNextTagPage();
            if (bInTabPageRang(curTagPage))
            {
                return true;
            }

            return false;
        }

        protected int getNextTagPage()
        {
            int curTagPage = m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx;
            while (++curTagPage < (int)EnPlayerCareer.ePCTotal)   // 如果 TaPage 后面还有
            {
                if (m_filterCardListArr[curTagPage].Count > 0)     // 如果当前页面有内容
                {
                    break;
                }
            }

            return curTagPage;
        }

        protected bool bInTabPageRang(int idx)
        {
            return (0 <= idx && idx < (int)EnPlayerCareer.ePCTotal);
        }

        public void updatePageUI()
        {
            if (m_sceneWDSCData.m_pClassFilterPnl.m_tabBtnIdx >= 0)
            {
                destroyAndUpdateCardList();
            }
        }

        protected bool bInRangeByPageAndIdx(int page, int idx)
        {
            return (m_pageArr[page].m_curPageIdx * (int)SCCardNumPerPage.eNum + idx < m_filterCardListArr[page].Count && 
                    idx < (int)SCCardNumPerPage.eNum);
        }

        // 点击收藏界面中卡牌面板中的一张卡牌
        public void onClkCard(SCUICardItemCom ioItem)
        {
            if (m_sceneWDSCData.m_wdscCardSetPnl.m_curTaoPaiMod == WdscmTaoPaiMod.eTaoPaiMod_Editset)
            {
                m_sceneWDSCData.m_wdscCardSetPnl.addCard2EditCardSet(ioItem.m_cardItemBase.m_tujian.id);
            }
        }

        public void buildFilterList()
        {
            int idx = 0;
            int idy = 0;

            for (idy = 0; idy < (int)EnPlayerCareer.ePCTotal; ++idy)
            {
                m_filterCardListArr[idy].Clear();

                for (idx = 0; idx < Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy].Count; ++idx)
                {
                    if (m_filterMp == 8)       // 全部
                    {
                        m_filterCardListArr[idy].Add(Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy][idx]);
                    }
                    else if (m_filterMp == 7)       // 大于等于 7 
                    {
                        if (Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy][idx].m_tableItemCard.m_magicConsume >= m_filterMp)
                        {
                            m_filterCardListArr[idy].Add(Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy][idx]);
                        }
                    }
                    else        // 等于
                    {
                        if (Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy][idx].m_tableItemCard.m_magicConsume == m_filterMp)
                        {
                            m_filterCardListArr[idy].Add(Ctx.m_instance.m_dataPlayer.m_dataCard.m_cardListArr[idy][idx]);
                        }
                    }
                }

                m_filterCardListArr[idy].Sort(cmpCardFunc);            // 排序卡牌

                m_pageArr[idy].m_curPageIdx = 0;        // 重置索引
                m_pageArr[idy].totalPage = (m_filterCardListArr[idy].Count + ((int)SCCardNumPerPage.eNum - 1)) / (int)SCCardNumPerPage.eNum;
                m_pageArr[idy].m_cardCount = m_filterCardListArr[idy].Count;
            }
        }

        protected int cmpCardFunc(CardItemBase a, CardItemBase b)
        {
            int ret = 0;
            if(a.m_tableItemCard.m_magicConsume < b.m_tableItemCard.m_magicConsume)
            {
                ret = -1;
            }
            else if(a.m_tableItemCard.m_magicConsume > b.m_tableItemCard.m_magicConsume)
            {
                ret = 1;
            }
            else    // 相等
            {
                if(a.m_tujian.id < b.m_tujian.id)
                {
                    ret = -1;
                }
                else if(a.m_tujian.id > b.m_tujian.id)
                {
                    ret = 1;
                }
                else
                {
                    ret = 0;
                }
            }

            return ret;
        }
    }
}