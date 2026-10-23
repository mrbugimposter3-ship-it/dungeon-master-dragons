using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster
{
    /// <summary>
    /// Procedural stand-ins for every cue. Drop a clip in Assets/Resources/Audio
    /// with the same name (pickup, hit, roar, ui, win, lose, ambient, torch, swing, heal, crit)
    /// and it replaces the synth.
    /// </summary>
    public class SoundBoard : MonoBehaviour
    {
        public static SoundBoard Instance { get; private set; }

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource oneShots;
        AudioSource ambient;
        AudioSource torch;

        void Awake()
        {
            Instance = this;
            oneShots = gameObject.AddComponent<AudioSource>();
            oneShots.playOnAwake = false;
            oneShots.spatialBlend = 0f;

            ambient = gameObject.AddComponent<AudioSource>();
            ambient.loop = true;
            ambient.spatialBlend = 0f;
            ambient.volume = 0.18f;
            ambient.clip = Load("ambient", LoopNoise("ambient", 2.5f, 70f, 0.25f));
            ambient.Play();

            torch = gameObject.AddComponent<AudioSource>();
            torch.loop = true;
            torch.spatialBlend = 0f;
            torch.volume = 0f;
            torch.clip = Load("torch", LoopNoise("torch", 1.2f, 400f, 0.15f));
            torch.Play();
        }

        public void SetTorchVolume(float volume)
        {
            if (torch != null) torch.volume = volume;
        }

        public void Play(string name)
        {
            AudioClip clip = Get(name);
            if (clip == null || oneShots == null) return;
            oneShots.pitch = Random.Range(0.94f, 1.06f);
            oneShots.PlayOneShot(clip, 0.8f);
        }

        AudioClip Get(string name)
        {
            AudioClip clip;
            if (clips.TryGetValue(name, out clip)) return clip;
            clip = Load(name, Synth(name));
            clips[name] = clip;
            return clip;
        }

        static AudioClip Load(string name, AudioClip fallback)
        {
            var loaded = Resources.Load<AudioClip>("Audio/" + name);
            return loaded != null ? loaded : fallback;
        }

        static AudioClip Synth(string name)
        {
            switch (name)
            {
                case "ui": return Tone("ui", new[] { 660f }, 0.08f, 0.3f);
                case "pickup": return Tone("pickup", new[] { 523f, 659f, 784f }, 0.09f, 0.35f);
                case "hit": return NoiseBurst("hit", 0.12f, 0.4f, 180f);
                case "swing": return NoiseBurst("swing", 0.1f, 0.28f, 90f);
                case "roar": return Tone("roar", new[] { 70f, 55f, 40f }, 0.28f, 0.45f);
                case "win": return Tone("win", new[] { 523f, 659f, 784f, 1046f }, 0.16f, 0.35f);
                case "lose": return Tone("lose", new[] { 392f, 311f, 247f }, 0.22f, 0.35f);
                case "heal": return Tone("heal", new[] { 880f, 1174f }, 0.1f, 0.25f);
                case "crit": return Tone("crit", new[] { 740f, 988f, 1318f }, 0.07f, 0.32f);
                default: return Tone(name, new[] { 440f }, 0.1f, 0.2f);
            }
        }

        static AudioClip Tone(string name, float[] freqs, float noteLen, float volume)
        {
            int rate = 44100;
            int noteSamples = Mathf.Max(1, (int)(rate * noteLen));
            int total = noteSamples * freqs.Length;
            var data = new float[total];
            for (int n = 0; n < freqs.Length; n++)
            {
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = i / (float)rate;
                    float env = Mathf.Exp(-3.2f * i / noteSamples);
                    data[n * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * env * volume;
                }
            }
            return Clip(name, data, rate);
        }

        static AudioClip NoiseBurst(string name, float dur, float volume, float tone)
        {
            int rate = 44100;
            int samples = Mathf.Max(1, (int)(rate * dur));
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float env = 1f - (i / (float)samples);
                env *= env;
                phase += tone / rate;
                float n = (Random.value * 2f - 1f);
                data[i] = (n * 0.7f + Mathf.Sin(phase * 2f * Mathf.PI) * 0.3f) * env * volume;
            }
            return Clip(name, data, rate);
        }

        static AudioClip LoopNoise(string name, float dur, float tone, float volume)
        {
            int rate = 22050;
            int samples = Mathf.Max(1, (int)(rate * dur));
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                phase += tone / rate;
                float n = Random.value * 2f - 1f;
                float env = 0.65f + 0.35f * Mathf.Sin(i / (float)samples * Mathf.PI * 2f);
                data[i] = (n * 0.55f + Mathf.Sin(phase * 2f * Mathf.PI) * 0.45f) * volume * env;
            }
            var clip = Clip(name, data, rate);
            return clip;
        }

        static AudioClip Clip(string name, float[] data, int rate)
        {
            var clip = AudioClip.Create(name, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
