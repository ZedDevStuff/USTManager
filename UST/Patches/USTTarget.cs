using UnityEngine;
using USTManager.Preprocessor;
using USTManager.Utility;

namespace USTManager.Patches
{
    public class USTTarget : MonoBehaviour
    {
        private bool Preprocessed = false;
        // This thing is so simple while fixing the whole mod
        public void Awake()
        {
            if(TryGetComponent<AudioSource>(out var source))
            {
                //Preprocess();
                if(source.playOnAwake)
                {
                    source.playOnAwake = false;
                    source.Stop();
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                    source.playOnAwake = true;
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
                //Preprocess();
                if(source.playOnAwake)
                {
                    source.playOnAwake = false;
                    source.Stop();
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                    source.playOnAwake = true;
                    source.Play();
                } 
                else
                {
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                }
            }
        }
        public void Start() {}

        public void Preprocess()
        {
            if(!Preprocessed)
            {
                Logging.Log($"Preprocessing {name}", Color.cyan);
                foreach(BasePreprocessor preprocessor in Registry.Preprocessors)
                {
                    preprocessor.Apply([GetComponent<AudioSource>()]);
                }
                //Preprocessed = true;
            }
        }
    }
}