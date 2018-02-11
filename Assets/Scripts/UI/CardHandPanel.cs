using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandPanel : MonoBehaviour {
    public Grid gameGrid;

    [HideInInspector]
    public List<CardVisual> cards = new List<CardVisual>();

    [Header("DEBUG REMOVE PLEASE")]
    public Card card1;
    public Card card2;

    private bool pointerOver = false;
    private CardVisual cardHovered = null;
    private CardVisual cardDragged = null;

    private void Start()
    {
        AddCard(card1);
        AddCard(card1);
        AddCard(card2);
        AddCard(card1);
        AddCard(card2);
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
                rt.localScale = Vector3.one;
                rt.localPosition = new Vector3(leftMost + (x * cardWidth), 0, 0);
                rt.SetAsLastSibling();
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
                rt.localScale = Vector3.one;
                rt.localPosition = new Vector3(leftMost + (x * distanceBetweenCards), 0, 0);
                rt.SetAsLastSibling();
            }
        }
    }

    public void AddCard(Card cardType)
    {
        GameObject temp = Instantiate(cardType.cardVisual);
        temp.transform.SetParent(transform);
        temp.GetComponent<CardVisual>().cardType = cardType;
        cards.Add(temp.GetComponent<CardVisual>());
        RefreshCardPositions();
    }

    private void Update()
    {
        cardHovered = null;
        if (pointerOver && cardDragged == null) handleCardHover();
        HandleMouseInput(); // Event system needs an image so instead we do our own system
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
        float combinedCardWidth = cardWidth * cards.Count;
        float leftMost = (cardWidth / 2) - (panelWidth / 2);

        if (combinedCardWidth < panelWidth)
        {
            leftMost = (cardWidth / 2) - (combinedCardWidth / 2);
            distanceBetweenCards = cardWidth;
        }

        for (int x = 0; x < cards.Count; x++)
        {
            float cardPosition = leftMost + (x * distanceBetweenCards);
            if (mouseXLocal > (cardPosition - distanceBetweenCards / 2) && mouseXLocal < (cardPosition + distanceBetweenCards / 2))
            {
                RefreshCardPositions();
                RectTransform cardRt = cards[x].GetComponent<RectTransform>();
                cardRt.SetAsLastSibling();
                cardRt.localScale = new Vector3(1.1f, 1.1f, 1);
                cardRt.localPosition = new Vector3(cardRt.localPosition.x, 35, cardRt.localPosition.z);
                cardHovered = cards[x];
            }
        }
    }

    private void HandleMouseInput()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float UpperBound = rt.position.y + rt.rect.height / 2.0f;
        if (Input.mousePosition.y <= UpperBound)
        {
            if (!pointerOver) OnPointerEnter();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                OnDragStart();
            }
        } else
        {
            if (pointerOver) OnPointerExit();
        }

        if (cardDragged != null) OnDrag();

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (cardDragged != null) OnDragEnd();
            cardDragged = null;
        }
    }

    private void OnDragStart()
    {
        cardDragged = cardHovered;
    }

    private void OnDragEnd()
    {
        foreach (GridCell gc in gameGrid)
        {
            gc.RefreshVisuals();
        }
        RefreshCardPositions();
    }

    private void OnDrag()
    {
        if (pointerOver)
        {
            cardDragged.GetComponent<RectTransform>().position = Input.mousePosition;
            cardDragged.transform.SetAsLastSibling();
            foreach (GridCell gc in gameGrid)
            {
                gc.RefreshVisuals();
            }
        } else
        {
            cardDragged.GetComponent<RectTransform>().position = new Vector3(1000000, 1000000, 0);
            RaycastHit hit;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            GridCell mouseTargetCell = null;

            // omg kill me this code is so hacky
            if (Physics.Raycast(cameraRay, out hit))
            {
                GridCellVisual gcv = hit.transform.gameObject.GetComponent<GridCellVisual>();
                if (gcv == null) return;
                foreach (GridCell gc in gameGrid)
                {
                    if (gc.visual == gcv)
                    {
                        mouseTargetCell = gc;
                        break;
                    }
                }
            }
            if (mouseTargetCell == null) return;

            // Hover correct pattern
            foreach (GridCell gc in gameGrid)
            {
                gc.RefreshVisuals();
            }
            for (int i = 0; i < 25; i++)
            {
                if (!cardDragged.cardType.tile.tiles[i]) continue;
                int dx = (i % 5) - 2;
                int dy = (i / 5) - 2;

                int x = mouseTargetCell.gridLocation.x - dx;
                int y = mouseTargetCell.gridLocation.y - dy;

                if (x < 0 || y < 0 || x >= gameGrid.size.x || y >= gameGrid.size.y) continue;

                gameGrid[x, y].visual.setHoverState();
            }
        }
        
    }
}
