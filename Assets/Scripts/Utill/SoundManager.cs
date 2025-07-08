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

        // �ҷ��� ���� ���� ��ε� (Resources/Audio/Effect/ ����)
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

        Debug.Log($"[SoundManager] ȿ���� �ڵ� ��� �Ϸ�: {sfxDict.Count}��");
    }

    private void LoadFolder(string path)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
        foreach (var clip in clips)
        {
            if (clip == null) continue;

            // ��: "item_heal", "ui_click"
            string key = clip.name.ToLower();

            if (!sfxDict.ContainsKey(key))
            {
                sfxDict.Add(key, clip);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] �ߺ��� Ű: {key} - ���� Ŭ�� ����");
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
            Debug.LogWarning($"[SoundManager] ��ϵ��� ���� ȿ���� Ű: '{key}'");
        }
    }
    //public void PlayBGM()
    //{
    //    // BGM ��� ������ ���⿡ �߰��ϼ���.
    //    // ��: AudioSource.PlayClipAtPoint(bgmClip, Vector3.zero);
    //    Debug.Log("[SoundManager] BGM ��� ������ ���� �������� �ʾҽ��ϴ�.");
    //}
}