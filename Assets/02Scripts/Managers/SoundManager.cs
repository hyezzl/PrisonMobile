using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SFXType
{
    Hit,        // 자원캐기
    PickUp,        
    Money,      // 돈
    Complete,   // 존 완료
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("AudioSource Ref")]
    public AudioSource sfxSource01;
    public AudioSource sfxSource02;
    public AudioSource loopSfxSource;       // 루프용 SFX오디오 소스

    [Header("Clip List")]
    public AudioClip[] sfxClips;

    [Header("Sound Setting")]
    public float fadeDuration = 1f;

    [Header("BTN")]
    public Button muteBtn;
    public Sprite[] muteOrNot;



    // 현재 사운드 상태값
    public bool isSFXPlaying = false;

    // onlySFX 변수
    public bool isOnlySfxPlaying = false;

    // Mute
    private bool isMute = false;

    private Coroutine bgmCoroutine;


    protected override void DoAwake()
    {
        base.DoAwake();
    }


    private void OnEnable()
    {
        muteBtn.onClick.AddListener(ToggleMute);
    }
    private void OnDisable()
    {
        muteBtn.onClick.RemoveListener(ToggleMute);

    }


    // SFX 재생
    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex < 0 || sfxIndex >= sfxClips.Length) return;
        sfxSource01.PlayOneShot(sfxClips[sfxIndex]);
    }

    // SFX 중복없이 재생
    public void PlayOnlySFX(int sfxIndex)
    {
        if (sfxIndex < 0 || sfxIndex >= sfxClips.Length) return;

        if (!isOnlySfxPlaying)
        {
            isOnlySfxPlaying = true;
            sfxSource01.PlayOneShot(sfxClips[sfxIndex]);

            // 소리 길이만큼 기다렸다가 플래그 리셋
            StartCoroutine(ResetOnlySFX(sfxClips[sfxIndex].length));
        }
    }

    // Loop될 SFX 재생
    public void PlayLoopSFX(int sfxIndex)
    {
        if (!loopSfxSource.isPlaying)
        {
            loopSfxSource.clip = sfxClips[sfxIndex];
            loopSfxSource.loop = true;
            loopSfxSource.Play();
        }
    }


    // SFX 즉시 종료
    public void StopSFX()
    {
        sfxSource01.Stop();
        sfxSource02.Stop();
        loopSfxSource.Stop();

        isOnlySfxPlaying = false;
        isSFXPlaying = false;
    }

    // 코루틴
    public IEnumerator ResetOnlySFX(float delay)
    {
        yield return new WaitForSeconds(delay);

        isOnlySfxPlaying = false;
    }

    public void ToggleMute()
    {
        isMute = !isMute;

        AudioListener.volume = isMute ? 0f : 1f;
        muteBtn.image.sprite = isMute ? muteOrNot[1] : muteOrNot[0];
    }
}
