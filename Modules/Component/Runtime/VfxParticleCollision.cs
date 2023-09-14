using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEngine;

namespace Pancake.Component
{
    public class VfxParticleCollision : GameComponent
    {
        [SerializeField] private ScriptableEventInt updateCoinWithValueEvent;
        [SerializeField] private ScriptableEventNoParam updateCoinEvent;
        [SerializeField] private ScriptableListGameObject vfxMagnetCollection;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;
        [field: SerializeField] public ParticleSystem PS { get; private set; }
        [SerializeField] private int numberParticle;
        [SerializeField] private bool enabledSound;
        [SerializeField, ShowIf(nameof(enabledSound))] private Audio audioCollision;
        [SerializeField, ShowIf(nameof(enabledSound))] private ScriptableEventAudio audioPlayEvent;

        private int _segmentValue;
        private bool _flag;

        public void Init(int value)
        {
            _flag = false;
            _segmentValue = value / numberParticle;
        }

        private void OnParticleCollision(GameObject particle)
        {
            updateCoinWithValueEvent.Raise(_segmentValue);
            if (enabledSound) audioPlayEvent.Raise(audioCollision);
        }

        protected override void Tick()
        {
            if (PS.particleCount > 0) return;

            if (!_flag)
            {
                _flag = true;
                returnPoolEvent.Raise(gameObject);
                if (vfxMagnetCollection.Count == 0) updateCoinEvent.Raise();
            }
        }
    }
}