using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using UnityEngine;

namespace LapisPlayer
{
    public class SoundBankLoader
    {
        static private SoundBankLoader _instance;
        static public SoundBankLoader Instance
        {
            get
            {
                if (_instance == null) _instance = new SoundBankLoader();
                return _instance;
            }
        }

        public async Task<AudioClip> LoadAudioClip(string soundBank, string eventHash)
        {
            string filePath = ConfigManager.Instance.SoundBanks + "/" + soundBank + "/" + eventHash + ".wav";
            string fullPath = "file:///" + filePath;
            AudioClip clip = null;
            float startTime = Time.time;

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
            www.SendWebRequest();

            try
            {
                while (!www.isDone)
                {
                    await Task.Delay(5);
                    if (Time.time > startTime + 10)
                    {
                        throw new TimeoutException();
                    }
                }

                if (www.result == UnityWebRequest.Result.ProtocolError) Debug.Log($"{www.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }

            return clip;
        }
    }
}
