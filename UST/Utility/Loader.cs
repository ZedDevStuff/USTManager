using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace USTManager.Utility
{
    public class Loader
    {
        public static AudioClip LoadClipFromPath(string path)
        {
            if(File.Exists(path))
            {
                string ext = Path.GetExtension(path);
                if(ext == ".wav" || ext == ".mp3" || ext == ".ogg")
                {
                    AudioClip clip;
                    UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
                    request.SendWebRequest();
                    while(!request.isDone) {}
                    clip = DownloadHandlerAudioClip.GetContent(request);
                    clip.name = "[UST] " + Path.GetFileNameWithoutExtension(path);
                    return clip;
                }
            }
            return null;
        }
    }
}