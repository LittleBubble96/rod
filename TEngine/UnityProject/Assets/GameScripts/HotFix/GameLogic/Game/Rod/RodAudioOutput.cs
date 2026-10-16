using UnityEngine;

namespace GameLogic.Rod
{
    /// <summary>入口独占的两个音轨：循环音乐和点击音效。连点复用音效声道，避免无界叠加语音实例。</summary>
    public sealed class RodAudioOutput
    {
        private readonly AudioSource music;
        private readonly AudioSource sound;

        public RodAudioOutput(GameObject owner, RodUISkin skin)
        {
            music = owner.AddComponent<AudioSource>();
            sound = owner.AddComponent<AudioSource>();
            music.playOnAwake = sound.playOnAwake = false;
            music.loop = true;
            music.volume = .4f;
            music.clip = skin.musicClip;
            sound.clip = skin.clickClip;
        }

        /// <summary>仅在存储提交成功后的快照上应用；关闭音乐停止解码，重新开启继续播放循环音轨。</summary>
        public void Apply(RodSaveData save)
        {
            sound.mute = save.soundMuted;
            if (save.musicMuted) { if (music.isPlaying) music.Pause(); }
            else if (music.clip && !music.isPlaying) music.Play();
        }

        public void Click()
        {
            if (!sound.mute && sound.clip) sound.Play();
        }
    }
}
