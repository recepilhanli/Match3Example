using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Level
{

    using Debug = Utils.Logger.Debug;
    
    public class ParticleEffectManager : MonoSingleton<ParticleEffectManager>
    {

        public enum ParticleEffectType
        {
            Bomb,
            Firework,

            Block_Green,
            Block_Red,
            Block_Blue,
            Block_Yellow,
            Block_Purple,
            Block_Pink,

            HighlightingStar,
        }


        [SerializeField] List<ParticleEffectData> _particleEffects = new List<ParticleEffectData>(); //unity does not serialize dictionaries..
        public Dictionary<ParticleEffectType, ParticleEffectData> particleEffects = new Dictionary<ParticleEffectType, ParticleEffectData>();


        protected override void Awake()
        {
            base.Awake();
            foreach (var effect in _particleEffects)
            {
                effect.Init(effect.maxCount);
                particleEffects.Add(effect.type, effect);
            }

            _particleEffects = null;
        }

        public ParticleSystem SpawnEffect(ParticleEffectType type, Vector3 position)
        {
            return particleEffects[type].Spawn(position);
        }

        public ParticleSystem SpawnEffect(BlastableType type, Vector3 position)
        {
            return particleEffects[SerializeBlastableType(type)].Spawn(position);
        }


        public static ParticleEffectType SerializeBlastableType(BlastableType type)
        {
            return type switch
            {
                BlastableType.Bomb => ParticleEffectType.Bomb,
                BlastableType.Firework => ParticleEffectType.Firework,
                BlastableType.ColorGreen => ParticleEffectType.Block_Green,
                BlastableType.ColorRed => ParticleEffectType.Block_Red,
                BlastableType.ColorBlue => ParticleEffectType.Block_Blue,
                BlastableType.ColorYellow => ParticleEffectType.Block_Yellow,
                BlastableType.ColorPurple => ParticleEffectType.Block_Purple,
                BlastableType.ColorPink => ParticleEffectType.Block_Pink,
                _ => ParticleEffectType.Block_Pink,
            };
        }

        [System.Serializable]
        public class ParticleEffectData
        {

            [Tooltip("The maximum number of particles that can be active in the scene at the same time")] public byte maxCount;
            public ParticleSystem particlePrefab;
            public ParticleEffectType type;

            public float duration => particlePrefab.main.duration;

            public Stack<ParticleSystem> reservedPool = null;
            public Queue<ParticleSystem> activePool = null;

            public void Init(int size)
            {
                if (reservedPool != null)
                {
                    Debug.LogWarning("Particle effect already initialized");
                    return;
                }
                reservedPool = new Stack<ParticleSystem>(size);
                activePool = new Queue<ParticleSystem>(size);
            }

            public ParticleSystem Spawn(Vector3 position)
            {

                ParticleSystem effect;

                if (activePool.Count >= maxCount)
                {
                    effect = activePool.Dequeue();
                    effect.Play();
                }

                else if (reservedPool.Count > 0)
                {
                    effect = reservedPool.Pop();
                    DespawnEffect(effect).Forget();
                }
                else
                {
                    effect = Instantiate(particlePrefab, position, Quaternion.identity);
                    effect.transform.SetParent(instance.transform);
                    DespawnEffect(effect).Forget();
                }

                effect.transform.position = position;
                effect.gameObject.SetActive(true);
                activePool.Enqueue(effect);
                return effect;
            }

            public async UniTaskVoid DespawnEffect(ParticleSystem effect)
            {
                await UniTask.WaitForSeconds(duration);
                HideParticle(effect);
                activePool.Dequeue();
                reservedPool.Push(effect);
            }

            public void HideParticle(ParticleSystem effect)
            {
                effect.gameObject.SetActive(false);
            }

        }
    }


}