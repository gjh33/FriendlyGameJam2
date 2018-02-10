using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandPanel : MonoBehaviour {
    public GameObject testCardObject;
    public List<CardVisual> cards = new List<CardVisual>();

    private bool pointerOver = false;

    private void Start()
    {
        GameObject temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        temp = Instantiate(testCardObject);
        temp.transform.SetParent(transform);
        cards.Add(temp.GetComponent<CardVisual>());
        RefreshCardPositions();
    }

    public void RefreshCardPositions()
    {
        // Compute some stats
        float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
        float panelWidth = GetComponent<RectTransform>().rect.width;
        float combinedCardWidth = cardWidth * cards.Count;

        // Center align cards edge to edge
        if (combinedCardWidth < panelWidth)
        {
            // position of left most card centered
            float leftMost = (cardWidth / 2) - (combinedCardWidth / 2);
            for (int x = 0; x < cards.Count; x++)
            {
                RectTransform rt = cards[x].GetComponent<RectTransform>();
                rt.localPosition = new Vector3(leftMost + (x * cardWidth), 0, 0);
            }
        }
        // Overlap cards
        else
        {
            float distanceBetweenCards = (panelWidth - cardWidth) / (cards.Count - 1);
            float leftMost = (cardWidth / 2) - (panelWidth / 2);

            for (int x = 0; x < cards.Count; x++)
            {
                RectTransform rt = cards[x].GetComponent<RectTransform>();
                rt.localPosition = new Vector3(leftMost + (x * distanceBetweenCards), 0, 0);
                rt.SetAsLastSibling();
            }
        }
    }

    private void Update()
    {
        if (pointerOver) handleCardHover();
    }

    public void OnPointerEnter()
    {
        pointerOver = true;
    }

    public void OnPointerExit()
    {
        pointerOver = false;
        RefreshCardPositions();
    }

    private void handleCardHover()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float mouseXLocal = Input.mousePosition.x - rt.position.x;
        float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
        float panelWidth = GetComponent<RectTransform>().rect.width;
        float distanceBetweenCards = (panelWidth - cardWidth) / (cards.Count - 1);
        float leftMost = (cardWidth / 2) - (panelWidth / 2);

        for (int x = 0; x < cards.Count; x++)
        {
            float cardPosition = leftMost + (x * distanceBetweenCards);
            if (mouseXLocal > (cardPosition - distanceBetweenCards / 2) && mouseXLocal < (cardPosition + distanceBetweenCards / 2))
            {
                RefreshCardPositions();
                cards[x].transform.SetAsLastSibling();
            }
        }
    }
}
