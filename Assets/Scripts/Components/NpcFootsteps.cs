// NpcFootsteps.cs
using UnityEngine;
using Zenject;
using GCNBC.Controllers;

namespace GCNBC.Components
{
    public class NpcFootsteps : MonoBehaviour
    {
        private AudioController _audio;

        [Inject]
        private void Construct(AudioController audio)
        {
            _audio = audio;
        }

        // Called by an Animation Event on the run clip. Method name must match the event.
        public void OnFootstep()
        {
            _audio.PlayFootstep(transform.position);
        }
    }
}