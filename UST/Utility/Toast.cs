using System.Collections;
using TMPro;
using UnityEngine;

namespace USTManager.Utility
{
    public class Toast : MonoBehaviour
    {
        public string Message;
        private TMP_Text Text;
        public float Duration = 2f;
        void Start()
        {
            Text = GetComponentInChildren<TMP_Text>();
        }
        IEnumerator Life()
        {
            yield return new WaitForSeconds(Duration);
            Destroy(gameObject);
        }
        // I forgot to add the canvas to the bundle so its unused for now
        public static void Show(string message, float duration = 2f)
        {
            GameObject toast = GameObject.Instantiate(Plugin.ToastPrefab);
            toast.GetComponent<Toast>().Duration = duration;
            toast.GetComponent<Toast>().Message = message;
        }
    }
}