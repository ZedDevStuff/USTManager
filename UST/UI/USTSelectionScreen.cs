using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using USTManager.Data;
using USTManager;
using System.Linq;

public class USTSelectionScreen : MonoBehaviour
{
    private static List<RectTransform> Entries = new List<RectTransform>();
    private ScrollRect ScrollRect;
    [SerializeField] private GameObject EntryPrefab;
    [SerializeField] private Button CreateButton, DeleteButton, RefreshButton, ExitButton, ConfirmButton;

    List<CustomUST> Data = new();

    private void Awake()
    {
        if(EntryPrefab == null) EntryPrefab = Plugin.SelectionScreenEntryPrefab;
        if(CreateButton == null) CreateButton = transform.GetChild(4).GetChild(0).GetComponent<Button>();
        if(DeleteButton == null)
        {
            DeleteButton = transform.GetChild(4).GetChild(1).GetComponent<Button>();
            DeleteButton.onClick.AddListener(() => { DeleteEntry(); });
        }
        if(RefreshButton == null) RefreshButton = transform.GetChild(4).GetChild(2).GetComponent<Button>();
        if(ExitButton == null)
        {
            ExitButton = transform.GetChild(1).GetComponent<Button>();
            ExitButton.onClick.AddListener(() => 
            { 
                if(SelectedEntry != null) SelectedEntry.GetComponent<Button>().interactable = true;
                ManipulatingEntry = null;
                DeleteButton.interactable = false;
                gameObject.SetActive(false); 
            });
        }
        if(ConfirmButton == null)
        {
            ConfirmButton = transform.GetChild(2).GetComponent<Button>();
            ConfirmButton.onClick.AddListener(() => 
            {
                ManipulatingEntry = null;
                DeleteButton.interactable = false;
                Manager.LoadUST(SelectedEntry == null ? null : SelectedEntry.UST);
                gameObject.SetActive(false);
            });
        }
        ScrollRect = GetComponentInChildren<ScrollRect>();
    }
    public USTEntry SelectedEntry { get; private set;}
    public USTEntry ManipulatingEntry { get; private set;}
    public static CustomUST SelectedUST { get; private set;}
    
    void OnEnable()
    {
        Refresh();
    }
    public void SelectEntry(GameObject obj)
    {
        if(SelectedEntry != null)
        {
            SelectedEntry.GetComponent<Button>().interactable = true;
            SelectedEntry.Status.text = "";
        }
        if(obj == null)
        {
            if(SelectedEntry != null)
            {
                SelectedEntry.GetComponent<Button>().interactable = true;
                SelectedEntry.Status.text = "";
                SelectedEntry = null;
                SelectedUST = null;
            }
        }
        else
        {
            SelectedEntry = obj.GetComponent<USTEntry>();
            SelectedEntry.GetComponent<Button>().interactable = false;
            SelectedEntry.Status.text = "Current";
            SelectedUST = SelectedEntry.UST;
        }
    }
    public void ManipulateEntry(GameObject obj)
    {
        ManipulatingEntry = obj.GetComponent<USTEntry>();
        DeleteButton.interactable = true;
    }
    public void DeleteEntry()
    {
        if(ManipulatingEntry != null)
        {
            Manager.DeleteUST(ManipulatingEntry.UST);
            SelectedUST = null;
            Destroy(ManipulatingEntry.gameObject);
            ManipulatingEntry = null;
        }
        DeleteButton.interactable = false;
    }
    public void AddNew()
    {
        // Later will open another screen to create a custom UST
    }

    public void Refresh()
    {
        Manager.CheckUSTs();
        RefreshUI(Manager.AllUSTs);
    }

    private void RefreshUI(List<CustomUST> data)
    {
        if(data == null) return;
        
        foreach (RectTransform entry in Entries)
        {
            if(entry != null && entry.gameObject != null) Destroy(entry.gameObject);
        }
        Entries.Clear();
        foreach (CustomUST entry in data)
        {
            GameObject newEntry = Instantiate(EntryPrefab, ScrollRect.content);
            USTEntry ustEntry = newEntry.GetComponent<USTEntry>();
            ustEntry.SetData(entry);
            if(SelectedUST != null && SelectedUST.Equals(entry))
            {
                newEntry.GetComponent<Button>().interactable = false;
                ustEntry.Status.text = "Current";
                SelectedEntry = ustEntry;
                SelectedUST = SelectedEntry.UST;
            }
            else newEntry.GetComponent<Button>().interactable = true;
            Entries.Add(newEntry.GetComponent<RectTransform>());
        }
        Data = data;
    }
}
