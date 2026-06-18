using GCNBC.Components;
using GCNBC.Services;
using GCNBC.Signals;
using UnityEngine;
using Zenject;

namespace GCNBC.SOs
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // SignalBus
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<ScoreChangedSignal>();
            Container.DeclareSignal<BallCollectedSignal>();
            Container.DeclareSignal<NpcDiedSignal>();
            Container.DeclareSignal<NpcSpawnedSignal>();
            Container.DeclareSignal<BallPickedUpSignal>();
            Container.DeclareSignal<NpcHealthChangedSignal>();

            // Managers
            Container.Bind<IScoreService>().To<ScoreManager>().AsSingle();

            // Scene objects
            Container.Bind<NpcSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TopDownCameraFollow>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IBallProvider>().To<BallSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ICartService>().To<GolfCartComponent>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<SceneAudioBinder>().AsSingle().NonLazy();

            // NPC factory — custom factory takes a prefab parameter, no fixed prefab binding.
            Container.Bind<NpcFactory>().AsSingle();
        }
    }
}