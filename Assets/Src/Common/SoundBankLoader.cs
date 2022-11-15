using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

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

        private (string, AudioType) GetFileExt()
        {
            return ConfigManager.Instance.SoundExtension switch
            {
                "wav" => (".wav", AudioType.WAV),
                "ogg" => (".ogg", AudioType.OGGVORBIS),
                "mp3" => (".mp3", AudioType.MPEG),
                _ => (".wav", AudioType.WAV),
            };
        }
        
        public IEnumerator PlayAudioClip (string fileName, AudioType audioType, AudioSource source)
        {
            string filePath = ConfigManager.Instance.SoundBanks + "/" + fileName;
            string fullPath = "file:///" + filePath;
            AudioClip clip = null;

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Audio path: " + filePath);
                Debug.Log($"{www.error}");
            }
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(www);
            }

            source.clip = clip;
            source.Play();
        }

        public async Task<AudioClip> LoadAudioClip(string fileName, AudioType audioType)
        {
            string filePath = ConfigManager.Instance.SoundBanks + "/" + fileName;
            string fullPath = "file:///" + filePath;
            AudioClip clip = null;
            float startTime = Time.time;

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType);
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

                if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Audio path: " + filePath);
                    Debug.Log($"{www.error}");
                }
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
        public Task<AudioClip> LoadAudioClip(string soundBank, string eventHash)
        {
            var (ext, audioType) = GetFileExt();
            return LoadAudioClip(soundBank + "/" + eventHash + ext, audioType);
        }
    }
}
