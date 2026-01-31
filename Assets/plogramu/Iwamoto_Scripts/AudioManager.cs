using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager instance;

    [Header("--- Audio Source ---")]
    [SerializeField] AudioSource musicSource; // BGM用
    [SerializeField] AudioSource sfxSource;   // 効果音用

    [Header("--- Audio Clip ---")]
    public AudioClip background; // BGM用の曲をセット
    public AudioClip ActionSE;   
    public AudioClip StartSE;   
    public AudioClip GoalSE;   
    public AudioClip GameOverSE;   


    private void Awake()
    {
        // シーンを跨いでもこのオブジェクトが重複しないようにする設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンを移動してもこのオブジェクトを壊さない
        }
        else
        {
            Destroy(gameObject); // すでに存在していたら自分を消す
        }
    }

    private void Start()
    {
        // ゲーム開始時にBGMを再生
        musicSource.clip = background;
        musicSource.Play();
    }

    // 好きな時にBGMを再生・変更するためのメソッド
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    // 好きな時に効果音を再生するためのメソッド
    public void PlaySE(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}