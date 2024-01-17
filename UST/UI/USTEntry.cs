using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using USTManager.Data;

public class USTEntry : MonoBehaviour
{
    public Image Image;
    public Button IconButton, EntryButton;
    public Image EntryButtonImage;
    Toggle Toggle;
    private Sprite Icon;
    public TMP_Text Name, Author, Status;
    private USTSelectionScreen SelectionScreen;
    public CustomUST UST;
    static Color green = new Color(0f, 1f, 0f,1);
    static Color white = new Color(1f, 1f, 1f,1);

    private void Awake()
    {
        SelectionScreen = GetComponentInParent<USTSelectionScreen>();
        Toggle = GetComponentInChildren<Toggle>();
        EntryButton = GetComponent<Button>();
        EntryButtonImage = EntryButton.GetComponent<Image>();
        IconButton.onClick.AddListener(() => 
        {
            if(Toggle.isOn) Toggle.isOn = false;
            else Toggle.isOn = true;
        });
        EntryButton.onClick.AddListener(() => 
        {
            if(Toggle.isOn) Toggle.isOn = false;
            else Toggle.isOn = true;
        });
        Toggle.onValueChanged.AddListener((value) => 
        {
            if(value)
            {
                EntryButtonImage.color = green;
                SelectionScreen.SelectEntry(this);
            }
            else
            {
                EntryButtonImage.color = white;
                SelectionScreen.DeselectEntry(this);
            } 
        });
    }
    public void Select()
    {
        Toggle.isOn = true;
    }
    void OnDestroy()
    {
        IconButton.onClick.RemoveAllListeners();
    }

    public void SetData(CustomUST ust)
    {
        UST = ust;
        if(ust.Icon != null)
        {
            Icon = ust.Icon;
            Image.sprite = Icon;
        }
        else if(ust.UserColor != null)
        {
            Color color = ust.UserColor;
            Image.color = color;
        }
        Name.text = ust.Name;
        Author.text = "by: "+ust.Author;
    }
}
