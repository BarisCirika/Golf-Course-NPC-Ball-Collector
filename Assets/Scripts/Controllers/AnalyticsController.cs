// AnalyticsController.cs
using UnityEngine;
using GCNBC.Services;

namespace GCNBC.Controllers
{
    // Placeholder. Later: real analytics SDK init.
    public class AnalyticsController : IInitializableService
    {
        public void Initialize()
        {
            Debug.Log("[AnalyticsController] Initialized (dummy).");
            // TODO: real analytics SDK startup
        }
    }
}