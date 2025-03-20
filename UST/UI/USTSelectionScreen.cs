using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using USTManager.Data;
using USTManager;
using System.Linq;
using USTManager.Utility;
using System.IO;

public class USTSelectionScreen : MonoBehaviour
{
    private static List<RectTransform> Entries = new List<RectTransform>();
    private ScrollRect ScrollRect;
    private GameObject EntryPrefab;
    private Button CreateButton, OpenFolderButton, RefreshButton, ExitButton, ConfirmButton;
    public static USTSelectionScreen Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if(EntryPrefab == null) EntryPrefab = Plugin.SelectionScreenEntryPrefab;
        if(CreateButton == null) CreateButton = transform.GetChild(4).GetChild(0).GetComponent<Button>();
        if(OpenFolderButton == null)
        {
            OpenFolderButton = transform.GetChild(4).GetChild(1).GetComponent<Button>();
            OpenFolderButton.onClick.AddListener(() => 
            {
                switch(Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                        System.Diagnostics.Process.Start("explorer.exe", Plugin.USTDir);
                        break;
                    case RuntimePlatform.OSXPlayer:
                        System.Diagnostics.Process.Start("open", Plugin.USTDir);
                        break;
                    case RuntimePlatform.LinuxPlayer:
                        System.Diagnostics.Process.Start("xdg-open", Plugin.USTDir);
                        break;
                };
            });
        }
        if(RefreshButton == null) 
        {
            RefreshButton = transform.GetChild(4).GetChild(2).GetComponent<Button>();
            RefreshButton.onClick.AddListener(() => { Refresh(); });
        }
        if(ExitButton == null)
        {
            ExitButton = transform.GetChild(1).GetComponent<Button>();
            ExitButton.onClick.AddListener(() => 
            {
                SelectedEntries.Clear();
                gameObject.SetActive(false); 
            });
        }
        if(ConfirmButton == null)
        {
            ConfirmButton = transform.GetChild(2).GetComponent<Button>();
            ConfirmButton.onClick.AddListener(() => 
            {
                if(SelectedEntries.Count != 0) 
                {
                    if(SelectedEntries.Count == 1)
                    {
                        Manager.LoadUST(SelectedEntries[0].UST);
                        CurrentUST = SelectedEntries[0].UST;
                        PersistentEntries.Clear();
                        SelectedEntries.ForEach(x => PersistentEntries.Add(x.UST.Path));
                        if(File.Exists(Plugin.LastUSTs)) File.Delete(Plugin.LastUSTs);
                        Manager.SaveUST();
                    }
                    else if(SelectedEntries.Count > 1)
                    {
                        Conflict conflict = ConflictResolver.Merge(SelectedEntries.Select(x => x.UST).ToArray());
                        if(conflict.Validate(out CustomUST ust))
                        {
                            Manager.LoadUST(ust);
                            CurrentUST = ust;
                            PersistentEntries.Clear();
                            SelectedEntries.ForEach(x => PersistentEntries.Add(x.UST.Path));
                            SelectedEntries.Clear();
                            if(File.Exists(Plugin.LastUSTs)) File.Delete(Plugin.LastUSTs);
                            Manager.SaveUST();
                        }
                        else
                        {
                            GameObject screen = Instantiate(Plugin.ConflictResolutionScreenPrefab, transform);
                            screen.GetComponent<ConflictResolutionScreen>().Setup(conflict);
                            return;
                        }
                    }
                }
                else
                {
                    Manager.UnloadUST();
                    PersistentEntries.Clear();
                    SelectedEntries.Clear();
                    if(File.Exists(Plugin.LastUSTs)) File.Delete(Plugin.LastUSTs);
                }
                gameObject.SetActive(false);
            });
        }
        ScrollRect = GetComponentInChildren<ScrollRect>();
    }
    public static Conflict CurrentConflict;
    public bool Confirm(Conflict conflict)
    {
        CurrentConflict = null;
        if(conflict.Validate(out CustomUST ust))
        {
            Manager.LoadUST(ust);
            CurrentConflict = conflict;
            CurrentUST = ust;
            PersistentEntries.Clear();
            SelectedEntries.ForEach(x => PersistentEntries.Add(x.UST.Path));
            SelectedEntries.Clear();
            gameObject.SetActive(false);
            if(File.Exists(Plugin.LastUSTs)) File.Delete(Plugin.LastUSTs);
            Manager.SaveUST();
            return true;
        }
        else return false;
    }
    public static void InternalConfirm(List<string> entries, CustomUST ust)
    {
        if(entries != null && ust != null)
        {
            Manager.LoadUST(ust, true);
            CurrentUST = ust;
            PersistentEntries.Clear();
            PersistentEntries = entries;
        }
    }
    public List<USTEntry> SelectedEntries = new();
    public static List<string> PersistentEntries = new();
    public static CustomUST CurrentUST { get; private set;}
    
    void OnEnable()
    {
        Refresh();
    }
    void OnDisable()
    {
        SelectedEntries.Clear();
        gameObject.SetActive(false);
    }
    public void SelectEntry(USTEntry entry)
    {
        if(entry != null && !SelectedEntries.Contains(entry))
        {
            SelectedEntries.Add(entry);
        }
    }
    public void DeselectEntry(USTEntry entry)
    {
        if(entry != null && SelectedEntries.Contains(entry)) SelectedEntries.Remove(entry);
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
            if(CurrentUST != null)
            {
                if(PersistentEntries.Contains(entry.Path))
                {
                    Debug.Log("Selected");
                    ustEntry.Select();
                }
            }
            else newEntry.GetComponent<Button>().interactable = true;
            Entries.Add(newEntry.GetComponent<RectTransform>());
        }
    }
}
