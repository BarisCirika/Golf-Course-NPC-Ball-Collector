using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GCNBC.SOs
{
    [CreateAssetMenu(fileName = "GameSettingsInstaller", menuName = "GCNBC/Installers/Game Settings Installer")]
    public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
    {
        [SerializeField] private List<BallLevelConfig> _levelConfigs = new();
        [SerializeField] private NPCPrefabRegistry _prefabRegistry;

        public override void InstallBindings()
        {
            Container.Bind<List<BallLevelConfig>>()
                     .FromInstance(_levelConfigs)
                     .AsSingle();

            Container.Bind<NPCPrefabRegistry>()
                     .FromInstance(_prefabRegistry)
                     .AsSingle();
        }
    }
}