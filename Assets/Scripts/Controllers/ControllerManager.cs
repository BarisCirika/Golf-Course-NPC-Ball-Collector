// ControllerManager.cs
using GCNBC.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace GCNBC.Controllers
{
    // Bootstrap orchestrator: initializes all registered services on app startup.
    // Lives in ProjectContext, so it runs once for the whole app.
    public class ControllerManager : IInitializable
    {
        private readonly List<IInitializableService> _services;

        // Zenject injects ALL bound IInitializableService implementations as a list.
        public ControllerManager(List<IInitializableService> services)
        {
            _services = services;
        }

        // Called once by Zenject when ProjectContext is built.
        public void Initialize()
        {
            Debug.Log($"[ControllerManager] Booting {_services.Count} services...");
            foreach (var service in _services)
            {
                service.Initialize();
            }
            Debug.Log("[ControllerManager] All services initialized.");
            Random.InitState((int)System.DateTime.Now.Ticks);
            ActivateMouseAndKeyboard();
        }

        private void ActivateMouseAndKeyboard()
        {

#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_TVOS

            var mouse = Mouse.current;
            if (mouse != null)
            {
                InputSystem.EnableDevice(Mouse.current);
                Debug.Log("Mouse başarıyla etkinleştirildi.");
            }
            else
            {
                Debug.LogError("Sistemde aktif bir mouse bulunamadı!");
            }

            var keyboard = Keyboard.current;

            if (keyboard != null)
            {
                InputSystem.EnableDevice(keyboard);
                Debug.Log("Klavye başarıyla etkinleştirildi.");
            }
            else
            {
                Debug.LogError("Sistemde aktif bir klavye bulunamadı!");
            }
#endif
        }

    }
}