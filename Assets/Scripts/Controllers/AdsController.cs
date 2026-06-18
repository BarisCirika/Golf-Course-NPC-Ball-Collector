// AdsController.cs
using UnityEngine;
using GCNBC.Services;

namespace GCNBC.Controllers
{
    // Placeholder. Later: real ad SDK init (AdMob, etc.).
    public class AdsController : IInitializableService
    {
        public void Initialize()
        {
            Debug.Log("[AdsController] Initialized (dummy).");
            // TODO: real ads SDK startup
        }
    }
}