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

    // Create slugbase player features
    public static readonly PlayerFeature<bool> CanGlideFeature = PlayerBool("downcast/can_glide");
    public static readonly PlayerFeature<float> GlidingTurnCoefFeature = PlayerFloat("downcast/gliding_turn_coef");
    public static readonly PlayerFeature<float> GlidingDragCoefFeature = PlayerFloat("downcast/gliding_drag_coef");
    public static readonly PlayerFeature<float> GlidingLiftCoefFeature = PlayerFloat("downcast/gliding_lift_coef");
    public static readonly PlayerFeature<float> GlidingUpwardCoefXFeature = PlayerFloat("downcast/gliding_upward_coef_x");
    public static readonly PlayerFeature<float> GlidingUpwardCoefYFeature = PlayerFloat("downcast/gliding_upward_coef_y");
    public static readonly PlayerFeature<float> GlidingAirFrictionFeature = PlayerFloat("downcast/gliding_air_friction");

    // Create player instance data using slugbase
    public static readonly PlayerData<bool> Gliding = new PlayerData<bool>(CanGlideFeature);
    public static readonly PlayerData<Vector2> GlidingDir = new PlayerData<Vector2>(CanGlideFeature);
    public static readonly PlayerData<Vector2> TargetGlidingDir = new PlayerData<Vector2>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingTurnCoef = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingDragCoef = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingLiftCoef = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingUpwardCoefX = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingUpwardCoefY = new PlayerData<float>(CanGlideFeature);
    public static readonly PlayerData<float> GlidingAirFriction = new PlayerData<float>(CanGlideFeature);

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
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
