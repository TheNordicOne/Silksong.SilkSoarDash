using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm
{
    // Every sound, vibration and shake the soar uses is already referenced by an action on the game's superJumpFSM, so they are read from there.
    public static class SsdEffects
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For(typeof(SsdEffects));
        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
        private static readonly Dictionary<string, VibrationDataAsset> Vibrations = new Dictionary<string, VibrationDataAsset>();
        private static readonly Dictionary<string, RandomAudioClipTable> Tables = new Dictionary<string, RandomAudioClipTable>();
        private static readonly Dictionary<string, CameraShakeProfile> Shakes = new Dictionary<string, CameraShakeProfile>();
        private static AudioSource _player2D;
        private static CameraManagerReference _camera;
        private static AudioSource _chargeAudio;
        private static HeroController _owner;

        private static HeroController Hero => HeroController.instance;

        public static void Build(HeroController hero)
        {
            if (_owner != null && _owner == hero)
            {
                return;
            }

            _owner = hero;
            Clips.Clear();
            Vibrations.Clear();
            Tables.Clear();
            Shakes.Clear();
            _player2D = null;
            _camera = null;

            foreach (var state in hero.superJumpFSM.FsmStates)
            {
                foreach (var action in state.Actions)
                {
                    Collect(action);
                }
            }

            SsdLog.Debug("collected clips={Clips} vibrations={Vibrations} tables={Tables} shakes={Shakes} player2D={Player2D} camera={Camera}", Clips.Count, Vibrations.Count, Tables.Count, Shakes.Count, _player2D, _camera);
        }

        private static void Collect(FsmStateAction action)
        {
            switch (action)
            {
                case PlayAudioEvent play:
                    Add(Clips, play.audioClip.Value as AudioClip);
                    if (_player2D == null)
                    {
                        _player2D = play.audioPlayerPrefab.Value as AudioSource;
                    }
                    break;
                case SetAudioClip set:
                    Add(Clips, set.audioClip.Value as AudioClip);
                    break;
                case PlayVibrationV2 vibration:
                    Add(Vibrations, vibration.vibrationDataAsset.Value as VibrationDataAsset);
                    break;
                case PlayRandomAudioClipTable table:
                    Add(Tables, table.Table.Value as RandomAudioClipTable);
                    break;
                case PlayRandomAudioClipTableV3 tableV3:
                    Add(Tables, tableV3.Table.Value as RandomAudioClipTable);
                    break;
                case DoCameraShakeV4 shake:
                    Add(Shakes, shake.Profile.Value as CameraShakeProfile);
                    if (_camera == null)
                    {
                        _camera = shake.Camera.Value as CameraManagerReference;
                    }
                    break;
            }
        }

        private static void Add<T>(Dictionary<string, T> store, T asset) where T : Object
        {
            if (asset != null && !store.ContainsKey(asset.name))
            {
                store.Add(asset.name, asset);
            }
        }

        public static void PlayOneShot(string clip, float pitchMin, float pitchMax)
        {
            if (!Clips.TryGetValue(clip, out var audioClip))
            {
                SsdLog.Warning("clip missing name={Name}", clip);
                return;
            }

            Event(audioClip, pitchMin, pitchMax).SpawnAndPlayOneShot(Hero.transform.position);
        }

        // vanilla plays some clips through its 2D actor prefab, which makes them ignore the listener's distance
        public static void PlayOneShot2D(string clip, float pitchMin, float pitchMax)
        {
            if (!Clips.TryGetValue(clip, out var audioClip))
            {
                SsdLog.Warning("clip missing name={Name}", clip);
                return;
            }

            if (_player2D == null)
            {
                Event(audioClip, pitchMin, pitchMax).SpawnAndPlayOneShot(Hero.transform.position);
                return;
            }

            Event(audioClip, pitchMin, pitchMax).SpawnAndPlayOneShot(_player2D, Hero.transform.position);
        }

        public static void PlayVoice(string table)
        {
            PlayTableAt(table, Hero.transform.position);
        }

        public static void PlayTableAt(string table, Vector3 position)
        {
            if (!Tables.TryGetValue(table, out var clipTable))
            {
                SsdLog.Warning("clip table missing name={Name}", table);
                return;
            }

            clipTable.SpawnAndPlayOneShot(position);
        }

        public static void Vibrate(string asset)
        {
            if (!Vibrations.TryGetValue(asset, out var vibration))
            {
                SsdLog.Warning("vibration missing name={Name}", asset);
                return;
            }

            VibrationManager.PlayVibrationClipOneShot(vibration, new VibrationTarget(VibrationMotors.None));
        }

        public static void StartChargeLoop()
        {
            StopChargeLoop();
            if (!Clips.TryGetValue(SsdAudio.ChargeLoop, out var audioClip))
            {
                SsdLog.Warning("clip missing name={Name}", SsdAudio.ChargeLoop);
                return;
            }

            _chargeAudio = Event(audioClip, 1f, 1f).SpawnAndPlayOneShot(_player2D, Hero.transform.position, () => _chargeAudio = null);
        }

        public static void StopChargeLoop()
        {
            var audio = _chargeAudio;
            _chargeAudio = null;
            if (audio == null || !audio.isPlaying)
            {
                return;
            }

            Hero.StartCoroutine(FadeOut(audio, SsdAudio.ChargeLoopFadeTime));
        }

        // the loop object carries the AudioSource and the VibrationPlayer, so it is switched on and off as one
        public static void StartLoop(string clip)
        {
            var loop = SsdClones.SuperjumpLoop;
            if (loop == null || !Clips.TryGetValue(clip, out var audioClip))
            {
                SsdLog.Warning("loop missing clip={Clip}", clip);
                return;
            }

            var audio = loop.GetComponent<AudioSource>();
            loop.gameObject.SetActive(true);
            audio.clip = audioClip;
            audio.volume = 1f;
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }

        public static void StartLoopVibration()
        {
            var loop = SsdClones.SuperjumpLoop;
            if (loop == null)
            {
                return;
            }

            loop.gameObject.SetActive(true);
            loop.GetComponent<VibrationPlayer>().Play();
        }

        public static void StopLoop()
        {
            var loop = SsdClones.SuperjumpLoop;
            if (loop == null)
            {
                return;
            }

            loop.GetComponent<VibrationPlayer>().Stop();
            loop.GetComponent<AudioSource>().Stop();
            loop.gameObject.SetActive(false);
        }

        public static void PlayReady()
        {
            var ready = SsdClones.NailArtReady;
            if (ready == null)
            {
                return;
            }

            ready.gameObject.SetActive(true);
            var audio = ready.GetComponent<AudioSource>();
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }

        public static void StopReady()
        {
            var ready = SsdClones.NailArtReady;
            if (ready == null)
            {
                return;
            }

            ready.GetComponent<AudioSource>().Stop();
            ready.gameObject.SetActive(false);
        }

        public static void StartRumble()
        {
            if (_camera == null || !Shakes.TryGetValue(SsdCamera.TinyRumble, out var profile))
            {
                SsdLog.Warning("rumble missing camera={Camera}", _camera);
                return;
            }

            _camera.DoShake(profile, Hero, false);
        }

        public static void Shake(string shake)
        {
            if (_camera == null || !Shakes.TryGetValue(shake, out var profile))
            {
                SsdLog.Warning("shake missing name={Name} camera={Camera}", shake, _camera);
                return;
            }

            _camera.DoShake(profile, Hero, false);
        }

        public static void StopRumble()
        {
            if (_camera == null || !Shakes.TryGetValue(SsdCamera.TinyRumble, out var profile))
            {
                return;
            }

            _camera.CancelShake(profile);
        }

        private static AudioEvent Event(AudioClip clip, float pitchMin, float pitchMax)
        {
            return new AudioEvent
            {
                Clip = clip,
                PitchMin = pitchMin,
                PitchMax = pitchMax,
                Volume = 1f
            };
        }

        private static IEnumerator FadeOut(AudioSource audio, float duration)
        {
            var startVolume = audio.volume;
            var start = Time.time;
            while (audio.isPlaying && Time.time < start + duration)
            {
                var remaining = (start + duration - Time.time) / duration;
                audio.volume = remaining * remaining * startVolume;
                yield return null;
            }

            audio.Stop();
        }
    }
}
