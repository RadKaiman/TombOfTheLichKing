using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] private HudView _hudPrefab;
    [SerializeField] private TileView _tilePrefab;
    [SerializeField] private Transform _gridParent;

    public override void InstallBindings()
    {
        Container.Bind<PlayerModel>().AsSingle();

        Container.BindFactory<int, int, LevelModel, LevelModel.Factory>();

        Container.Bind<TurnController>().AsSingle();
        Container.Bind<LuckController>().AsSingle();
        Container.Bind<LevelGenerationController>().AsSingle();

        Container.BindFactory<int, int, TileData, TileView, TileView.Factory>()
            .FromComponentInNewPrefab(_tilePrefab)
            .UnderTransform(_gridParent);

        Container.Bind<HudView>().FromComponentInNewPrefab(_hudPrefab).AsSingle().NonLazy();
    }
}
