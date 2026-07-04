//--------------------------------------
//
//  AudioManager.cs
//
//  概要
//  BGM・SEの音量を調整するシングルトン
//
//  更新履歴
//
//  2026/07/04  作成  
//
//--------------------------------------
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager:MonoBehaviour
{
    public static AudioManager Instance { get;private set; }

    [Tooltip("AudioMixerをアタッチ")]
    [SerializeField] private AudioMixer audioMixer;

    //音量は0～100で管理(10刻み)
    private int _bgmVolume = 50;
    private int _seVolume = 50;

    public int BgmVolume => _bgmVolume;
    public int SeVolume => _seVolume;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  //シーンをまたいで維持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBgmVolume(int value)
    {
        _bgmVolume = Mathf.Clamp(value, 0, 100);
        // AudioMixerはdB単位なので変換(0は-80dB,100は0dB)
        float db = _bgmVolume == 0 ? -80f : Mathf.Log10(-BgmVolume / 100f) * 20f;
        audioMixer.SetFloat("BGMVolume", db);
    }

    public void SetSeVolume(int value)
    {
        _seVolume = Mathf.Clamp(value, 0, 100);
        float db = _seVolume == 0 ? -80f : Mathf.Log10(_seVolume / 100f) * 20f;
        audioMixer.SetFloat("SEVolume",db);
    }
}
