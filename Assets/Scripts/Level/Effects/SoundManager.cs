using UnityEngine;

namespace Game.Level
{

    public class SoundManager : MonoSingleton<SoundManager>
    {
        public static Sounds sounds => instance._sounds;
        public AudioSource commonAudioSource;
        [SerializeField] private Sounds _sounds = new Sounds();

        public void PlaySound(AudioClip clip, float volume = 1f) => commonAudioSource.PlayOneShot(clip, volume);

        [System.Serializable]
        public class Sounds
        {
            public AudioClip blastColored;
            public AudioClip blastBomb;
            public AudioClip blastFirework;
            public AudioClip mergeFirework;
            public AudioClip singleFirework;
            public AudioClip cantBlast;
            public AudioClip buttonTap;
        }
    }
}