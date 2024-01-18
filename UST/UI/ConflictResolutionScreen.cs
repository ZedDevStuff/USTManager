using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using USTManager;
using USTManager.Utility;

public class ConflictResolutionScreen : MonoBehaviour
{
    private ScrollRect ScrollRect;
    private Conflict result;
    private USTSelectionScreen Parent;
    private Button ConfirmButton, ExitButton;
    void OnDisable()
    {
        Destroy(gameObject);
    }
    public void Setup(Conflict conflict)
    {
        Parent = transform.parent.GetComponent<USTSelectionScreen>();
        Debug.Log("Parent: " + Parent == null ? "null" : Parent.name);
        ScrollRect = GetComponentInChildren<ScrollRect>();
        ConfirmButton = transform.GetChild(2).transform.GetChild(1).GetComponent<Button>();
        ConfirmButton.onClick.AddListener(() => { Confirm(); });
        ExitButton = transform.GetChild(2).transform.GetChild(0).GetComponent<Button>();
        ExitButton.onClick.AddListener(() => { Destroy(gameObject); });
        result = conflict;
        foreach(var entry in conflict.Conflicts)
        {
            GameObject obj = Instantiate(Plugin.ConflictEntryPrefab, ScrollRect.content);
            obj.AddComponent<ConflictEntry>().Setup(entry.Key, entry.Value.Select(x => x.Name).ToArray());
        }
    }
    public void UpdateResult(string key, int index)
    {
        result.Resolve(key, result.Conflicts[key][index]);
    }
    public void Confirm()
    {
        Parent.Confirm(result);
    }
}