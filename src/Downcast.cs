using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using Debug = UnityEngine.Debug;
using System.Data.SqlClient;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Downcast;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public partial class Downcast : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "eclogite.downcast";
    public const string PLUGIN_NAME = "The Downcast";
    public const string PLUGIN_VERSION = "1.0.0";
    private static readonly DowncastOptions Options = DowncastOptions.instance;

    // Create slugbase player features
    public static readonly PlayerFeature<bool> CanGlideFeature = PlayerBool("downcast/can_glide");
    public static readonly PlayerFeature<float> TurningCoefficientFeature = PlayerFloat("downcast/turning_coefficient");
    public static readonly PlayerFeature<float> DragCoefficientFeature = PlayerFloat("downcast/drag_coefficient");
    public static readonly PlayerFeature<float> LiftCoefficientFeature = PlayerFloat("downcast/lift_coefficient");

    // Create player instance data using slugbase
    public static readonly PlayerData<bool> Gliding = new PlayerData<bool>(CanGlideFeature);
    public static readonly PlayerData<Vector2> GlidingDir = new PlayerData<Vector2>(CanGlideFeature);
    public static readonly PlayerData<Vector2> TargetGlidingDir = new PlayerData<Vector2>(CanGlideFeature);
    public static readonly PlayerData<float> TurningCoefficient = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> DragCoefficient = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> LiftCoefficient = new PlayerData<float>(CanGlideFeature);

    // Create physical object instance data using slugbase
    public static readonly Data<PhysicalObject, Vector2> NetForce = new Data<PhysicalObject, Vector2>(null);

    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (IsInit) return;

        try
        {
            IsInit = true;

            //Register new enum values
            DowncastEnums.RegisterAll();

            //Your hooks go here
            DowncastHooks.Apply();

            //Add options interface
            MachineConnector.SetRegisteredOI(PLUGIN_GUID, Options);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
