using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class PlaylistScroll : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Offset;
    public int ListOffset;
    public float Velocity;

    public Playlist Playlist;
    public PlaylistScrollItem ItemSample;
    public List<PlaylistScrollItem> Items;
    public int ListPadding = 10;
    public float ItemSize = 52;

    bool isDragging;
    RectTransform self;
    float oldOffset = float.NaN;
    float oldVelocity = float.NaN;
    float oldTime;

    public RectTransform MainCanvas;
    public RectTransform SelectedSongBox;
    public TMP_Text SongNameLabel;
    public TMP_Text ArtistNameLabel;
    public TMP_Text DataLabel;
    
    public RectTransform ProfileBar;
    public RectTransform ActionBar;

    bool isScrolling = false;
    bool isSelectionShown = false;
    bool isAnimating = false;
    PlaylistScrollItem SelectedItem;
    int SelectedIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        self = GetComponent<RectTransform>();
        InitItems();
    }

    void InitItems()
    {
        Items.Clear();
        for (int a = -ListPadding; a <= ListPadding; a++)
        {
            PlaylistScrollItem item = Instantiate(ItemSample, transform);
            item.SetSong(Playlist.Items[Modulo(a, Playlist.Items.Count)]);
            Items.Add(item);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            if (Offset == oldOffset && Velocity == oldVelocity) 
            {
                oldTime += Time.deltaTime;
                if (oldTime > .1f) Velocity = 0;
            }
            else 
            {
                oldTime = 0;
            }
        }
        else 
        {
            if (Mathf.Abs(Velocity) < 10) 
            {
                Velocity = 0;
                Offset += (Mathf.Round(Offset / ItemSize) * ItemSize - Offset) * (1 - Mathf.Pow(.001f, Time.deltaTime));
                isScrolling = false;
            }
            else 
            {
                Velocity *= Mathf.Pow(.3f * Mathf.Min(Mathf.Abs(Velocity / 100), 1), Time.deltaTime);
                Offset += Velocity * Time.deltaTime;
            }
        }

        
        if (oldOffset != Offset)
        {
            int pos = -Mathf.RoundToInt(Offset / ItemSize);

            while (ListOffset < pos)
            {
                PlaylistScrollItem item = Items[0];
                Items.RemoveAt(0);
                Items.Add(item);
                ListOffset++;
                item.SetSong(Playlist.Items[Modulo(ListOffset + ListPadding, Playlist.Items.Count)]);
                isScrolling = true;
            }
            while (ListOffset > pos)
            {
                PlaylistScrollItem item = Items[Items.Count - 1];
                Items.RemoveAt(Items.Count - 1);
                Items.Insert(0, item);
                ListOffset--;
                item.SetSong(Playlist.Items[Modulo(ListOffset - ListPadding, Playlist.Items.Count)]);
                isScrolling = true;
            }

            Offset = Modulo(Offset, ItemSize * Playlist.Items.Count);
            ListOffset = pos = -Mathf.RoundToInt(Offset / ItemSize);

            float ofs = Offset + (pos - ListPadding) * ItemSize;
            foreach (PlaylistScrollItem item in Items) 
            {
                RectTransform rt = (RectTransform)item.transform;
                float realOfs = (ofs + Mathf.Clamp(ofs * .6f, -32, 32)) * Mathf.Min(Mathf.Abs(ofs) / ItemSize * 2 + .1f, 1);
                rt.anchoredPosition = new Vector2(Ease.Get(Mathf.Abs(realOfs / self.rect.height * 2), "Circle", EaseMode.In) * self.rect.width / 2, -realOfs);
                ofs += ItemSize;
            }

            if (isSelectionShown == true && isAnimating == false)
            {
                SelectedSongBox.anchoredPosition = GetSelectionOffset();
            }
        }

        if (isSelectionShown == false && isScrolling == false)
        {
            if (!isAnimating) StartCoroutine(ShowSelection());
        }
        else if (isSelectionShown == true && isScrolling == true)
        {
            if (!isAnimating) StartCoroutine(HideSelection());
        }
        isSelectionShown = !isScrolling;

        oldOffset = Offset;
        oldVelocity = Velocity;
    }

    Vector2 GetSelectionOffset()
    {
        float y = ((RectTransform)SelectedItem.transform).anchoredPosition.y;
        return Vector2.up * (y * Mathf.Abs(y) / ItemSize / 2);
    }

    IEnumerator ShowSelection()
    {
        SelectedIndex = -Mathf.RoundToInt(Offset / ItemSize);
        SelectedItem = Items[ListPadding];
        SongNameLabel.text = SelectedItem.SongNameLabel.text;
        ArtistNameLabel.text = SelectedItem.ArtistNameLabel.text;
        DataLabel.text = SelectedItem.DataText;

        SelectedSongBox.gameObject.SetActive(true);
        SelectedItem.CoverImage.gameObject.SetActive(false);
        isAnimating = true;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, "Exponential", EaseMode.InOut);
            Rect coverRect = SelectedItem.CoverImage.rectTransform.rect;
            Vector2 coverPos = MainCanvas.InverseTransformPoint(SelectedItem.CoverImage.transform.position);

            SelectedSongBox.sizeDelta = Vector2.Lerp(coverRect.size, new Vector2(0, 100), ease);
            SelectedSongBox.anchorMin = Vector2.Lerp(new Vector2(.5f, .5f), new Vector2(0, .5f), ease);
            SelectedSongBox.anchorMax = Vector2.Lerp(new Vector2(.5f, .5f), new Vector2(1, .5f), ease);
            SelectedSongBox.anchoredPosition = Vector2.Lerp(coverPos, GetSelectionOffset(), ease);
            SelectedItem.SongNameLabel.rectTransform.anchoredPosition = new Vector2(9 + 200 * ease, SelectedItem.SongNameLabel.rectTransform.anchoredPosition.y);
            SelectedItem.ArtistNameLabel.rectTransform.anchoredPosition = new Vector2(10 + 200 * ease, SelectedItem.ArtistNameLabel.rectTransform.anchoredPosition.y);
            SelectedItem.SongNameLabel.alpha = SelectedItem.ArtistNameLabel.alpha = 1 - ease;

            float ease2 = Ease.Get(Mathf.Max(value * 2 - 1, 0), "Quadratic", EaseMode.Out);
            SongNameLabel.rectTransform.anchoredPosition = new Vector2(100 - 50 * ease2, SongNameLabel.rectTransform.anchoredPosition.y);
            ArtistNameLabel.rectTransform.anchoredPosition = new Vector2(101 - 50 * ease2, ArtistNameLabel.rectTransform.anchoredPosition.y);
            DataLabel.rectTransform.anchoredPosition = new Vector2(102 - 50 * ease2, DataLabel.rectTransform.anchoredPosition.y);
            SongNameLabel.alpha = ArtistNameLabel.alpha = DataLabel.alpha = ease2;
            
            float ease3 = Ease.Get(value, "Quintic", EaseMode.Out);
            ProfileBar.anchoredPosition = new Vector2(0, -40 * ease3);
            ActionBar.anchoredPosition = new Vector2(0, 40 * ease3);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .6f)
        {
            LerpSelection(a);
            yield return null;
        }
        LerpSelection(1);

        isAnimating = false;
        if (isScrolling == true) StartCoroutine(HideSelection());
    }

    IEnumerator HideSelection()
    {
        isAnimating = true;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, "Exponential", EaseMode.Out);
            Rect coverRect = SelectedItem.CoverImage.rectTransform.rect;
            Vector2 coverPos = MainCanvas.InverseTransformPoint(SelectedItem.CoverImage.transform.position);

            SelectedSongBox.sizeDelta = Vector2.Lerp(new Vector2(0, 100), coverRect.size, ease);
            SelectedSongBox.anchorMin = Vector2.Lerp(new Vector2(0, .5f), new Vector2(.5f, .5f), ease);
            SelectedSongBox.anchorMax = Vector2.Lerp(new Vector2(1, .5f), new Vector2(.5f, .5f), ease);
            SelectedSongBox.anchoredPosition = Vector2.Lerp(GetSelectionOffset(), coverPos, ease);
            SelectedItem.SongNameLabel.rectTransform.anchoredPosition = new Vector2(209 - 200 * ease, SelectedItem.SongNameLabel.rectTransform.anchoredPosition.y);
            SelectedItem.ArtistNameLabel.rectTransform.anchoredPosition = new Vector2(210 - 200 * ease, SelectedItem.ArtistNameLabel.rectTransform.anchoredPosition.y);
            SelectedItem.SongNameLabel.alpha = SelectedItem.ArtistNameLabel.alpha = ease;

            float ease2 = Ease.Get(value, "Quadratic", EaseMode.Out);
            SongNameLabel.rectTransform.anchoredPosition = new Vector2(50 - 100 * ease2, SongNameLabel.rectTransform.anchoredPosition.y);
            ArtistNameLabel.rectTransform.anchoredPosition = new Vector2(51 - 100 * ease2, ArtistNameLabel.rectTransform.anchoredPosition.y);
            DataLabel.rectTransform.anchoredPosition = new Vector2(52 - 100 * ease2, DataLabel.rectTransform.anchoredPosition.y);
            SongNameLabel.alpha = ArtistNameLabel.alpha = DataLabel.alpha = 1 - ease2;
            
            float ease3 = 1 - Ease.Get(value, "Quintic", EaseMode.Out);
            ProfileBar.anchoredPosition = new Vector2(0, -40 * ease3);
            ActionBar.anchoredPosition = new Vector2(0, 40 * ease3);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            LerpSelection(a);
            if (Math.Abs(SelectedIndex + Offset / ItemSize) > ListPadding) break;
            yield return null;
        }

        SelectedSongBox.gameObject.SetActive(false);
        SelectedItem.CoverImage.gameObject.SetActive(true);
        isAnimating = false;
        if (isScrolling == false) StartCoroutine(ShowSelection());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = isScrolling || !isAnimating;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Offset -= eventData.delta.y;
            Velocity = -eventData.delta.y / Time.deltaTime;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public int Modulo(int a, int b) => ((a % b) + b) % b;
    public float Modulo(float a, float b) => ((a % b) + b) % b;
}
