// ProjectInstaller.cs
using GCNBC.Controllers;
using GCNBC.Services;
using GCNBC.Signals;
using UnityEngine;
using Zenject;

namespace GCNBC.SOs
{
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Audio")]
        [Tooltip("Prefab with AudioController + AudioSource(s) on it.")]
        [SerializeField] private GameObject _audioControllerPrefab;

        public override void InstallBindings()
        {
            InstallServices();
            InstallControllerManager();
        }

        private void InstallServices()
        {
            // AudioController: MonoBehaviour from prefab. Bound as both its concrete type
            // (so others can call PlaySfx/PlayMusic) and IInitializableService (so the
            // ControllerManager boots it on startup). Single shared instance.
            Container.Bind(typeof(AudioController), typeof(IInitializableService))
                     .To<AudioController>()
                     .FromComponentInNewPrefab(_audioControllerPrefab)
                     .AsSingle()
                     .NonLazy();

            // Plain C# startup services (no Unity dependency).
            Container.Bind<IInitializableService>().To<AdsController>().AsSingle();
            Container.Bind<IInitializableService>().To<AnalyticsController>().AsSingle();
            Container.Bind<SceneTransitionController>().AsSingle();
        }

        private void InstallControllerManager()
        {
            // IInitializable -> Zenject calls Initialize() once when ProjectContext is built.
            // It receives List<IInitializableService> and boots each one.
            Container.BindInterfacesAndSelfTo<ControllerManager>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}