using UnityEngine;

namespace USTManager.Patches
{
    public class USTTarget : MonoBehaviour
    {
        // This thing is so simple while fixing the whole mod
        public void Awake()
        {
            if(TryGetComponent<AudioSource>(out var source))
            {
                if(source.playOnAwake)
                {
                    source.Stop();
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                    source.Play();
                } 
                else
                {
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                }
            }
        }
        public void OnEnable()
        {
            if(TryGetComponent<AudioSource>(out var source))
            {
                if(source.playOnAwake)
                {
                    source.Stop();
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                    source.Play();
                } 
                else
                {
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                }
            }
        }
        public void Start() {}
    }
}