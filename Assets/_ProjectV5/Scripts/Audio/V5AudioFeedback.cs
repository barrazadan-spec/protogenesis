using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(AudioSource))]
    public class V5AudioFeedback : MonoBehaviour
    {
        public bool Enabled = true;
        public float Volume = 0.18f;
        private AudioSource source;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = Volume;
            BuildClip("division", 523.25f, 0.12f);
            BuildClip("gene", 659.25f, 0.14f);
            BuildClip("structure", 392f, 0.10f);
            BuildClip("warning", 220f, 0.18f);
            BuildClip("victory", 783.99f, 0.28f);
            BuildClip("defeat", 146.83f, 0.28f);
        }

        public void PlayCue(string cue)
        {
            if (!Enabled || source == null) return;
            AudioClip clip;
            if (!clips.TryGetValue(cue, out clip)) clip = clips.ContainsKey("structure") ? clips["structure"] : null;
            if (clip == null) return;
            source.PlayOneShot(clip, Volume);
        }

        private void BuildClip(string key, float frequency, float duration)
        {
            int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.Clamp01((float)i / samples) * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * env * 0.35f;
            }
            AudioClip clip = AudioClip.Create("V5_" + key, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            clips[key] = clip;
        }
    }
}
