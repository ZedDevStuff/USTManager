using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using USTManager.Utility;

public class ConflictEntry : MonoBehaviour
{
    private TMP_Text Title;
    private TMP_Dropdown Dropdown;
    private ConflictResolutionScreen Parent;
    public void Setup(string title, string[] options)
    {
        Title = transform.GetChild(0).GetComponent<TMP_Text>();
        Dropdown = transform.GetChild(1).GetComponent<TMP_Dropdown>();
        Parent = GetComponentInParent<ConflictResolutionScreen>();
        Dropdown.onValueChanged.AddListener((int value) => 
        {
            Parent.UpdateResult(Title.text, value);
        });
        Title.text = title;
        Dropdown.ClearOptions();
        Dropdown.AddOptions(options.ToList());
        Dropdown.value = 0;
        Parent.UpdateResult(Title.text,0);
    }
}