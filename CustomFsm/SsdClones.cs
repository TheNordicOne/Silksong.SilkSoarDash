using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm
{
    // The game's own soar objects are shared with vanilla Silk Soar, so we run on copies instead.
    public static class SsdClones
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For(typeof(SsdClones));

        private const string ClonePrefix = "SSD ";

        private static readonly List<Transform> Created = new List<Transform>();

        private static HeroController _owner;

        public static Transform AnticEffectL { get; private set; }
        public static Transform AnticEffectR { get; private set; }
        public static Transform ChargedEffect { get; private set; }
        public static Transform ChargingFader { get; private set; }
        public static Transform ExtraThrowEffect { get; private set; }
        public static Transform HarpoonThrowEffect { get; private set; }
        public static Transform ExtraGroundEffect { get; private set; }
        public static Transform DashEffect { get; private set; }
        public static Transform GrabEffect { get; private set; }
        public static Transform CatchEffect { get; private set; }
        public static Transform Thread { get; private set; }
        public static Transform ThreadLoop { get; private set; }
        public static Transform ThrowNeedle { get; private set; }
        public static Transform RetractNeedle { get; private set; }
        public static Transform StickNeedle { get; private set; }
        public static Transform Damager { get; private set; }
        public static Transform SuperjumpLoop { get; private set; }
        public static Transform NailArtReady { get; private set; }

        public static void Build(HeroController hero)
        {
            if (_owner != null && _owner == hero)
            {
                return;
            }

            Release();

            _owner = hero;

            AnticEffectL = Clone(hero, SsdObjects.AnticEffectL);
            AnticEffectR = Clone(hero, SsdObjects.AnticEffectR);
            ChargedEffect = Clone(hero, SsdObjects.ChargedEffect);
            ChargingFader = Clone(hero, SsdObjects.ChargingFader);

            ExtraGroundEffect = Clone(hero, SsdObjects.ExtraGroundEffect);
            CatchEffect = Clone(hero, SsdObjects.CatchEffect);

            ExtraThrowEffect = ClonePointed(hero, SsdObjects.ExtraThrowEffect);
            Thread = ClonePointed(hero, SsdObjects.Thread);

            // the harpoon dash objects are drawn sideways already
            HarpoonThrowEffect = Clone(hero, SsdObjects.HarpoonThrowEffect);
            DashEffect = Clone(hero, SsdObjects.DashEffect);
            GrabEffect = Clone(hero, SsdObjects.GrabEffect);
            StickNeedle = Clone(hero, SsdObjects.StickNeedle);

            ThreadLoop = ClonePointed(hero, SsdObjects.ThreadLoop);

            ThrowNeedle = CloneTurned(hero, SsdObjects.ThrowNeedle);
            RetractNeedle = CloneTurned(hero, SsdObjects.RetractNeedle);
            Damager = CloneTurned(hero, SsdObjects.Damager);

            SuperjumpLoop = Clone(hero, SsdObjects.SuperjumpLoop);
            NailArtReady = Clone(hero, SsdObjects.NailArtReady);

            hero.gameObject.AddComponent<Keeper>();
        }

        // The stuck needle spends the dash unparented, so it would outlive a hero that dies mid soar.
        private static void Release()
        {
            foreach (var clone in Created.Where(clone => clone != null))
            {
                Object.Destroy(clone.gameObject);
            }

            Created.Clear();
            _owner = null;
        }

        private static Transform ClonePointed(HeroController hero, string path)
        {
            var clone = Clone(hero, path);
            if (clone != null)
            {
                clone.PointForward();
            }

            return clone;
        }

        private static Transform CloneTurned(HeroController hero, string path)
        {
            var clone = Clone(hero, path);
            if (clone != null)
            {
                clone.TurnForward();
            }

            return clone;
        }

        private static Transform Clone(HeroController hero, string path)
        {
            var original = hero.transform.Find(path);
            if (original == null)
            {
                SsdLog.Warning("clone source missing path={Path}", path);
                return null;
            }

            var clone = Object.Instantiate(original.gameObject, original.parent);
            clone.name = ClonePrefix + original.name;
            clone.SetActive(false);

            Unfreeze(clone);
            Created.Add(clone.transform);

            return clone.transform;
        }

        // The needles are frozen on X because vanilla only ever throws them straight up.
        private static void Unfreeze(GameObject clone)
        {
            var body = clone.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                return;
            }

            body.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        }

        private sealed class Keeper : MonoBehaviour
        {
            private void OnDestroy()
            {
                Release();
            }
        }
    }
}
