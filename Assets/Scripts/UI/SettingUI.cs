using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
using System;

public class SettingUI : UIBase
{
    private const string Master = "MASTER";
    private const string Bgm = "BGM";
    private const string Effect = "EFFECT";

    enum Buttons
    {
        SettingBtn,
        ExitBtn,
    }
    enum Sliders
    {
        MasterSlider,
        BgmSlider,
        EffectSlider,
    }
    enum Images
    {
        BackGround,
    }

    [SerializeField] AudioMixer audioMixer;
    private Image settingImage;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));

        settingImage = GetImage((int)Images.BackGround);

        SetPlayerPrefs();
        InitSlider(Sliders.MasterSlider, SetMasterVolume, Master);
        InitSlider(Sliders.BgmSlider, SetBgmVolume, Bgm);
        InitSlider(Sliders.EffectSlider, SetEffectVolume, Effect);

        GetButton((int)Buttons.SettingBtn).gameObject.BindEvent((PointerEventData data) => { settingImage.gameObject.SetActive(true); });
        GetButton((int)Buttons.ExitBtn).gameObject.BindEvent((PointerEventData data) => { settingImage.gameObject.SetActive(false); });
    }

    #region SoundSlider
    private void SetPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(Master) && PlayerPrefs.HasKey(Bgm) && PlayerPrefs.HasKey(Effect)) return;

        PlayerPrefs.SetFloat(Master, 0.5f);
        PlayerPrefs.SetFloat(Bgm, 0.5f);
        PlayerPrefs.SetFloat(Effect, 0.5f);
    }

    private void InitSlider(Sliders s, UnityEngine.Events.UnityAction<float> action, string channel)
    {
        Slider slider = Get<Slider>((int)s);
        slider.onValueChanged.AddListener(action);
        slider.value = PlayerPrefs.GetFloat(channel);
    }

    private void SetMasterVolume(float value)
    {
        audioMixer.SetFloat(Master, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(Master, value);
    }
    private void SetBgmVolume(float value)
    {
        audioMixer.SetFloat(Bgm, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(Bgm, value);
    }
    private void SetEffectVolume(float value)
    {
        audioMixer.SetFloat(Effect, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(Effect, value);
    }
    #endregion
}
