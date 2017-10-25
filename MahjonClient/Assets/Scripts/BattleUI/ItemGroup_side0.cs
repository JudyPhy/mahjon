﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGroup_side0 : MonoBehaviour
{
    private List<Item_card> m_inhandList = new List<Item_card>();
    private List<Item_card> m_exchangeList = new List<Item_card>();
    private List<Item_card> m_pgList = new List<Item_card>();
    private List<Item_card> m_discardList = new List<Item_card>();
    
    private Pool m_pool;

    public SideInfo SideInfo
    {
        get { return m_sideInfo; }
    }
    private SideInfo m_sideInfo;

    private int m_curInhandItemIndex;
    private Vector3 m_inhandStartPos;
    private Vector3 m_inhandSpace;
    
    private Vector3 m_pgStartPos;
    private Vector3 m_ppggSpace;

    private Vector3 m_exchangeStartPos;
    private Vector3 m_exchangeStartSpace;
    private Vector3 m_exchangeEndPos;
    private Vector3 m_exchangeEndSpace;
    private Vector3 m_exchangeUpOffset;

    private int m_curDiscardItemIndex;
    private Vector3 m_discardStartPos;
    private Vector3 m_discardSpaceX;
    private Vector3 m_discardSpaceY;
    private Vector3 m_discardAniStartPos;

    private void Awake()
    {
        m_pool = new Pool("Item_card");
    }

    public void Init(SideInfo sideInfo)
    {
        m_sideInfo = sideInfo;
        m_curInhandItemIndex = 0;
        SetCardPos();
    }

    private void SetCardPos()
    {
        m_inhandStartPos = CardPos.InhandStartPos(m_sideInfo.SideIndex);
        m_inhandSpace = CardPos.InhandSpace(m_sideInfo.SideIndex);

        m_pgStartPos = CardPos.PGStartPos(m_sideInfo.SideIndex);
        m_ppggSpace = CardPos.PPGGSpace(m_sideInfo.SideIndex);

        m_exchangeStartPos = CardPos.ExchangeStartPos(m_sideInfo.SideIndex);
        m_exchangeStartSpace = CardPos.ExchangeStartSpace(m_sideInfo.SideIndex);
        m_exchangeEndPos = CardPos.ExchangeEndPos(m_sideInfo.SideIndex);
        m_exchangeEndSpace = CardPos.ExchangeEndSpace(m_sideInfo.SideIndex);
        m_exchangeUpOffset = CardPos.ExchangeUpOffset(m_sideInfo.SideIndex);

        m_discardStartPos = CardPos.DiscardStartPos(m_sideInfo.SideIndex);
        m_discardSpaceX = CardPos.DiscardSpaceX(m_sideInfo.SideIndex);
        m_discardSpaceY = CardPos.DiscardSpaceY(m_sideInfo.SideIndex);
        m_discardAniStartPos = CardPos.DiscardAniStartPos(m_sideInfo.SideIndex);
    }

    public void DrawInhandCard(int count)
    {
        Debug.Log("DrawInhandCard: player[" + m_sideInfo.OID + "], count=" + count + "]");
        for (int i = 0; i < count; i++)
        {
            PlaceOneInhandCard(m_sideInfo.CardList[m_curInhandItemIndex]);
        }
    }

    private void PlaceOneInhandCard(Card card)
    {
        Item_card item = m_pool.GetObject<Item_card>();
        m_inhandList.Add(item);
        item.transform.parent = transform;
        item.transform.localPosition = m_inhandStartPos + m_curInhandItemIndex * m_inhandSpace;
        item.UpdateUI(m_sideInfo.SideIndex, card);
        m_curInhandItemIndex++;
        if (m_sideInfo.SideIndex == 1)
        {
            int curDepth = 14 - m_curInhandItemIndex + 5;
            item.SetDepth(curDepth);
        }
    }

    public void SortInhandCards()
    {
        Debug.Log("SortForExchange");
        List<Card> list = m_sideInfo.GetCardList(CardStatus.InHand);
        list.Sort((card1, card2) => { return card1.Id.CompareTo(card2.Id); });
        m_pool.RecycleAll();
        m_inhandList.Clear();
        for (m_curInhandItemIndex = 0; m_curInhandItemIndex < list.Count;)
        {
            PlaceOneInhandCard(list[m_curInhandItemIndex]);
        }
    }

    public void PutExchangeCardsToCenter()
    {
        Debug.Log("PlayExchangeCardsAni");
        SortInhandCards();
        List<Card> list = m_sideInfo.GetCardList(CardStatus.Exchange);
        for (int i = 0; i < list.Count; i++)
        {
            Item_card item = m_pool.GetObject<Item_card>();
            m_exchangeList.Add(item);
            item.transform.parent = transform;
            item.transform.localPosition = m_exchangeStartPos + m_exchangeStartSpace * i;
            item.transform.localScale = Vector3.one * 1.5f;
            item.ShowBack(m_sideInfo.SideIndex);
            if (m_sideInfo.SideIndex == 1)
            {
                int curDepth = 3 - i + 5;
                item.SetDepth(curDepth);
            }
            iTween.MoveTo(item.gameObject, iTween.Hash("position", m_exchangeEndPos + m_exchangeEndSpace * i, "islocal", true, "time", 0.5f));
            iTween.ScaleTo(item.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f));
        }
    }

    public void ExchangePlayerCards(pb.ExchangeType type)
    {
        switch (type)
        {
            case pb.ExchangeType.ClockWise:
                PlayClockWiseAni();
                break;
            case pb.ExchangeType.AntiClock:
                PlayAntiClockWiseAni();
                break;
            case pb.ExchangeType.Opposite:
                PlayOppositeAni();
                break;
            default:
                break;
        }
    }

    private void PlayClockWiseAni()
    {       
        int fromIndex = m_sideInfo.SideIndex;
        int toIndex = fromIndex - 1;
        if (toIndex < 0)
        {
            toIndex = 3;
        }
        PlayExchangeAni(toIndex);
    }

    private void PlayAntiClockWiseAni()
    {
        int fromIndex = m_sideInfo.SideIndex;
        int toIndex = fromIndex + 1;
        if (toIndex > 3)
        {
            toIndex = 0;
        }
        PlayExchangeAni(toIndex);
    }

    private void PlayOppositeAni()
    {
        int fromIndex = m_sideInfo.SideIndex;
        int toIndex = fromIndex + 2;
        if (toIndex > 3)
        {
            toIndex -= 4;
        }
        PlayExchangeAni(toIndex);
    }

    private void PlayExchangeAni(int toIndex)
    {
        Vector3 toPos = CardPos.ExchangeEndPos(toIndex);
        Vector3 toOffset = CardPos.ExchangeEndSpace(toIndex);
        for (int i = 0; i < m_exchangeList.Count; i++)
        {
            Item_card item = m_exchangeList[i];
            item.ShowBack(toIndex);
            iTween.MoveTo(item.gameObject, iTween.Hash("position", toPos[toIndex] + toOffset[toIndex] * i, "islocal", true, "time", 0.4f, "delay", 0.5f));
        }
    }

    public void ShowExchangeCards()
    {
        SortInhandCards();
        Vector3 curPos = m_inhandStartPos + m_curInhandItemIndex * m_inhandSpace;
        List<Card> exchange = m_sideInfo.GetCardList(CardStatus.Exchange);
        for (int i = 0; i < exchange.Count; i++)
        {
            exchange[i].Status = CardStatus.InHand;
            Item_card item = m_pool.GetObject<Item_card>();
            item.transform.parent = transform;
            item.transform.localPosition = curPos + i * m_inhandSpace + m_exchangeUpOffset;
            item.UpdateUI(m_sideInfo.SideIndex, exchange[i]);
            if (m_sideInfo.SideIndex == 1)
            {
                int curDepth = 14 - m_curInhandItemIndex + i + 5;
                item.SetDepth(curDepth);
            }
            Vector3 endPos = item.transform.localPosition - m_exchangeUpOffset;
            iTween.MoveTo(item.gameObject, iTween.Hash("position", endPos, "islocal", true, "time", 0.5f, "delay", 0.5f));
        }
    }

    public void SortForLack()
    {
        if (m_sideInfo.SideIndex == 0)
        {
            SortInhandCards();
        }
        else
        {
            m_curInhandItemIndex += 3;
        }
    }

    public void SortForDiscard()
    {
        Debug.Log("SortForDiscard: m_inhandList count=" + m_inhandList.Count);
        List<Card> deal = m_sideInfo.GetCardList(CardStatus.Deal);
        if (deal.Count != 0)
        {
            deal[0].Status = CardStatus.InHand;
            PlaceOneInhandCard(deal[0]);
        }
        m_inhandList[m_inhandList.Count - 1].transform.localPosition += m_inhandSpace / 10.0f;
    }

    public void UnChooseDiscard(int preDiscardOid)
    {
        for (int i = 0; i < m_inhandList.Count; i++)
        {
            if (m_inhandList[i].Info.OID == preDiscardOid)
            {
                m_inhandList[i].UnChoose();
            }
        }
    }

    public void PlayDiscardAni(Card discard)
    {
        Debug.Log("PlayDiscardAni discardOid:" + discard.OID + ", discardId:" + discard.Id);
        //animation
        Item_card item = m_pool.GetObject<Item_card>();
        m_discardList.Add(item);
        item.transform.parent = transform;
        item.UpdateUI(m_sideInfo.SideIndex, discard);
        item.transform.localPosition = m_discardAniStartPos;
        if (m_sideInfo.SideIndex == 1)
        {
            int curDepth = 14 - m_curDiscardItemIndex % 10 + 5;
            item.SetDepth(curDepth);
        }
        else if (m_sideInfo.SideIndex == 2)
        {
            int curDepth = 5 - m_curDiscardItemIndex / 10;
            item.SetDepth(curDepth);
        }
        item.transform.localScale = Vector3.one * 1.2f;
        Vector3 to = m_discardStartPos + m_curDiscardItemIndex / 10 * m_discardSpaceY + m_curDiscardItemIndex % 10 * m_discardSpaceX;
        iTween.MoveTo(item.gameObject, iTween.Hash("position", to, "islocal", true, "time", 0.5f));
        iTween.ScaleTo(item.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f));
    }

    private void SortPGCards()
    {
        m_pgList.Clear();
        Dictionary<int, Card> pgCards = new Dictionary<int, Card>();
        List<Card> pList = m_sideInfo.GetCardList(CardStatus.Peng);
        List<Card> gList = m_sideInfo.GetCardList(CardStatus.Gang);
        for (int i = 0; i < pList.Count; i++)
        {
            if (!pgCards.ContainsKey(pList[i].Id))
            {
                pgCards.Add(pList[i].Id, pList[i]);
            }
        }
        for (int i = 0; i < gList.Count; i++)
        {
            int id = gList[i].Id;
            if (!pgCards.ContainsKey(id))
            {
                pgCards.Add(id, gList[i]);
            }
            else if (pgCards[id].Status == CardStatus.Gang && (!pgCards[id].IsFromOther && gList[i].IsFromOther))
            {
                pgCards[id] = gList[i];
            }
        }
        int itemIndex = 0;
        int n = 0;
        foreach (Card card in pgCards.Values)
        {
            for (int i = 0; i < 3; i++)
            {
                Item_card item = m_pool.GetObject<Item_card>();
                m_pgList.Add(item);
                item.UpdateUI(m_sideInfo.SideIndex, card);
                item.transform.localPosition = m_pgStartPos + itemIndex * m_ppggSpace - m_ppggSpace / 10 * n;
                itemIndex++;
            }
            if (card.Status == CardStatus.Gang)
            {
                Item_card item = m_pool.GetObject<Item_card>();
                m_pgList.Add(item);
                if (card.IsFromOther)
                {
                    item.UpdateUI(m_sideInfo.SideIndex, card);
                }
                else
                {
                    item.ShowBack(m_sideInfo.SideIndex);
                }
                item.transform.localPosition = m_pgStartPos + (itemIndex - 2) * m_ppggSpace - m_ppggSpace / 10 * n + new Vector3(0, 10, 0);
            }
            n++;
        }
    }

    public void SortAllCards()
    {
        SortInhandCards();
        SortPGCards();
    }

    // Update is called once per frame
    void Update()
    {

    }
}