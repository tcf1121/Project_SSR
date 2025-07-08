using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;

    [SerializeField] private Dictionary<string, AudioClip> sfxDict;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sfxDict = new Dictionary<string, AudioClip>();

        // 불러올 하위 폴더 경로들 (Resources/Audio/Effect/ 하위)
         string[] folders = {
            "Audio/Effect/Item",
            "Audio/Effect/UI",
            "Audio/Effect/Monster",
            "Audio/Effect/Boss",
            "Audio/Effect/Object",
            "Audio/Effect/Player",
            "Audio/Effect/Item/Arm",
            "Audio/Effect/Item/Body",
            "Audio/Effect/Item/Head",
            "Audio/Effect/Item/Boss/FinalBoss"
        };

        foreach (string folder in folders)
        {
            LoadFolder(folder);
        }

        Debug.Log($"[SoundManager] 효과음 자동 등록 완료: {sfxDict.Count}개");
    }

    private void LoadFolder(string path)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
        foreach (var clip in clips)
        {
            if (clip == null) continue;

            // 예: "item_heal", "ui_click"
            string key = clip.name.ToLower();

            if (!sfxDict.ContainsKey(key))
            {
                sfxDict.Add(key, clip);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] 중복된 키: {key} - 기존 클립 유지");
            }
        }
    }

    public void PlaySFX(string key)
    {
        key = key.ToLower();

        if (sfxDict.TryGetValue(key, out var clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 등록되지 않은 효과음 키: '{key}'");
        }
    }
    //public void PlayBGM()
    //{
    //    // BGM 재생 로직을 여기에 추가하세요.
    //    // 예: AudioSource.PlayClipAtPoint(bgmClip, Vector3.zero);
    //    Debug.Log("[SoundManager] BGM 재생 로직은 아직 구현되지 않았습니다.");
    //}
}