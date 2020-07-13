using UnityEngine;

public class SpritePlayer : MonoBehaviour
{
    [System.Serializable]
    public class SpriteClip
    {
        [SerializeField] Texture[] m_textures = default;
        [SerializeField] AudioClip m_audioClip = default;
        [SerializeField] bool m_loop = default;
        [SerializeField] float m_framesBetweenUpdates = default;
        public Texture[] Textures { get { return m_textures; } }

        public AudioClip AudioClip => m_audioClip;
        public bool Loop { get { return m_loop; } }
        public float FramesBetweenUpdates { get { return m_framesBetweenUpdates; } }
    }

    [SerializeField] SpriteClip[] m_clips = default;
    public int ClipCount { get { return m_clips.Length; } }

    [SerializeField] MeshRenderer m_renderer = default;
    [SerializeField] AudioSource m_audioSource = default;

    [SerializeField] bool m_autoStart = default;

    Material m_material = default;

    public bool IsPlaying { get; private set; } = false;

    int m_lastFrameUpdate = 0;
    int m_currentTextureIndex = 0;
    int m_currentClipIndex = 0;

    void Start()
    {
        m_material = m_renderer.material;
        m_material.mainTexture = m_clips[m_currentClipIndex].Textures[m_currentTextureIndex];

        if (m_audioSource != null)
        {
            m_audioSource.clip = m_clips[m_currentClipIndex].AudioClip;
        }

        if (m_autoStart)
        {
            Play();
        }
    }

    void LateUpdate()
    {
        if (IsPlaying)
        {
            if ((Time.frameCount - m_lastFrameUpdate) > m_clips[m_currentClipIndex].FramesBetweenUpdates)
            {
                m_currentTextureIndex++;
                if (m_currentTextureIndex >= m_clips[m_currentClipIndex].Textures.Length)
                {
                    if (m_clips[m_currentClipIndex].Loop)
                    {
                        m_currentTextureIndex = 0;
                    }
                    else
                    {
                        Stop();
                        return;
                    }
                }

                m_material.mainTexture = m_clips[m_currentClipIndex].Textures[m_currentTextureIndex];
                gameObject.SetActive(m_clips[m_currentClipIndex].Textures[m_currentTextureIndex] != null);

                m_lastFrameUpdate = Time.frameCount;
            }
        }
    }

    public void SetClip(int clipIndex)
    {
        if (clipIndex < m_clips.Length)
        {
            m_currentClipIndex = clipIndex;
            m_currentTextureIndex = 0;

            if (m_audioSource != null)
            {
                m_audioSource.clip = m_clips[m_currentClipIndex].AudioClip;
            }
        }
        else
        {
            Debug.LogError("Clip: " + clipIndex + " does not exist in clip list");
        }
    }

    public void Play()
    {
        IsPlaying = true;
        gameObject.SetActive(true);
        if (m_audioSource != null)
        {
            m_audioSource.Play();
        }
    }

    public void Pause()
    {
        IsPlaying = false;
        //gameObject.SetActive(false);
        if (m_audioSource != null)
        {
            m_audioSource.Stop();
        }
    }

    public void Stop()
    {
        IsPlaying = false;
        gameObject.SetActive(false);

        m_currentClipIndex = 0;
        m_currentTextureIndex = 0;
        m_material.mainTexture = m_clips[m_currentClipIndex].Textures[m_currentTextureIndex];
        if (m_audioSource != null)
        {
            m_audioSource.clip = m_clips[m_currentClipIndex].AudioClip;
            m_audioSource.Stop();
        }
    }
}
