using UnityEngine;
using Zenject;
using System;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] private HudView _hudPrefab;
    [SerializeField] private TileView _tilePrefab;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private Transform _hudCanvas;

    [Header("Level Progression")]
    [SerializeField] private string _startingLevelName = "Level_01_Tutorial";

    public override void InstallBindings()
    {
        Container.Bind<PlayerModel>().AsSingle();

        Container.BindFactory<int, int, int, LevelModel, LevelModel.Factory>();

        Container.Bind<TurnController>().AsSingle();
        Container.Bind<LuckController>().AsSingle();
        Container.Bind<LevelGenerationController>().AsSingle();
        Container.Bind<LevelProgressionController>().AsSingle().WithArguments(_startingLevelName);

        Container.Bind<IInitializable>().To<TurnController>().FromResolve();
        Container.Bind<IInitializable>().To<LuckController>().FromResolve();
        Container.Bind<IInitializable>().To<LevelProgressionController>().FromResolve();

        Container.Bind<IDisposable>().To<TurnController>().FromResolve();
        Container.Bind<IDisposable>().To<LuckController>().FromResolve();
        Container.Bind<IDisposable>().To<LevelGenerationController>().FromResolve();
        Container.Bind<IDisposable>().To<LevelProgressionController>().FromResolve();

        Container.BindFactory<int, int, TileData, TileView, TileView.Factory>()
            .FromComponentInNewPrefab(_tilePrefab)
            .UnderTransform(_gridParent);

        Container.Bind<HudView>().FromComponentInNewPrefab(_hudPrefab).UnderTransform(_hudCanvas).AsSingle().NonLazy();
    }
}
