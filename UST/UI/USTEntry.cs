using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using USTManager.Data;

public class USTEntry : MonoBehaviour
{
    [SerializeField] public Image Image;
    [SerializeField] public Button IconButton;
    private Sprite Icon;
    [SerializeField] public TMP_Text Name, Author, Status;
    private USTSelectionScreen SelectionScreen;
    public CustomUST UST;

    private void Awake()
    {
        SelectionScreen = GetComponentInParent<USTSelectionScreen>();
        GetComponent<Button>().onClick.AddListener(() => { SelectionScreen.ManipulateEntry(gameObject); });
        IconButton.onClick.AddListener(() => 
        {
            if(SelectionScreen.SelectedEntry != null && SelectionScreen.SelectedEntry.UST.Equals(UST))
            {
                SelectionScreen.SelectedEntry.GetComponent<Button>().interactable = true;
                SelectionScreen.SelectedEntry.Status.text = "";
                SelectionScreen.SelectEntry(null);
            }
            else SelectionScreen.SelectEntry(gameObject); 
        });
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
