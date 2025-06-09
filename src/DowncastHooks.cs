using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace Downcast
{
    internal class DowncastHooks
    {
        private static readonly DowncastOptions Options = DowncastOptions.instance;
        //public static float DragCoefficient { get { return Options.dragCoefficient.Value / 1000f; } }

        public static void Apply()
        {
            On.Player.ctor += (orig, self, abstractCreature, world) =>
            {
                orig(self, abstractCreature, world);

                // Add gliding variables if the can glide feature is present and set to true.
                if (Downcast.CanGlideFeature.TryGet(self, out bool canGlide) && canGlide)
                {
                    Downcast.Gliding.Get(self).Value = true;
                    Downcast.GlidingDir.Get(self).Value = Vector2.right;
                    Downcast.TargetGlidingDir.Get(self).Value = Vector2.right;
                    if (!Downcast.GlidingTurnCoefFeature.TryGet(self, out Downcast.GlidingTurnCoef.Get(self).Value))
                    {
                        Downcast.GlidingTurnCoef.Get(self).Value = 0.1f;
                    }
                    if (!Downcast.GlidingDragCoefFeature.TryGet(self, out Downcast.GlidingDragCoef.Get(self).Value))
                    {
                        Downcast.GlidingDragCoef.Get(self).Value = 0.75f;
                    }
                    if (!Downcast.GlidingLiftCoefFeature.TryGet(self, out Downcast.GlidingLiftCoef.Get(self).Value))
                    {
                        Downcast.GlidingLiftCoef.Get(self).Value = 0.2f;
                    }
                    if (!Downcast.GlidingUpwardCoefXFeature.TryGet(self, out Downcast.GlidingUpwardCoefX.Get(self).Value))
                    {
                        Downcast.GlidingUpwardCoefX.Get(self).Value = 0.04f;
                    }
                    if (!Downcast.GlidingUpwardCoefYFeature.TryGet(self, out Downcast.GlidingUpwardCoefY.Get(self).Value))
                    {
                        Downcast.GlidingUpwardCoefY.Get(self).Value = 0.14f;
                    }
                    if (!Downcast.GlidingAirFrictionFeature.TryGet(self, out Downcast.GlidingAirFriction.Get(self).Value))
                    {
                        Downcast.GlidingAirFriction.Get(self).Value = 0.02f;
                    }
                }
            };

            On.Player.UpdateBodyMode += (orig, self) =>
            {
                if (Downcast.CanGlideFeature.TryGet(self, out bool canGlide) && canGlide)
                {
                    // Handle entering/exiting gliding mode
                    bool lastLowerBodyInAir = self.bodyChunks[1].lastContactPoint.y == 0 && self.bodyChunks[1].lastContactPoint.x == 0;
                    bool lastUpperBodyInAir = self.bodyChunks[0].lastContactPoint.y == 0 && self.bodyChunks[0].lastContactPoint.x == 0;
                    bool lowerBodyInAir = self.bodyChunks[1].ContactPoint.y == 0 && self.bodyChunks[1].ContactPoint.x == 0;
                    bool upperBodyInAir = self.bodyChunks[0].ContactPoint.y == 0 && self.bodyChunks[0].ContactPoint.x == 0;
                    bool notOverGround = !self.IsTileSolid(1, 0, -1) && !self.IsTileSolid(0, 0, -1);
                    bool inAir = lowerBodyInAir && upperBodyInAir && lastLowerBodyInAir && lastUpperBodyInAir && notOverGround;
                    bool bodyModeCanGlide = self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == DowncastEnums.PlayerBodyModeIndex.Gliding;
                    bool jumpPressed = self.input[0].jmp && !self.input[1].jmp;  // Jump was just pressed, but it is not being held
                    bool startGliding = inAir && bodyModeCanGlide && jumpPressed;  // Criteria to start gliding
                    bool stopGliding = !bodyModeCanGlide;  // Criteria to stop gliding
                    if (!Downcast.Gliding.Get(self).Value && startGliding)
                    {
                        Downcast.Gliding.Get(self).Value = true;

                        // Initialize glidingDir and targetGlidingDir
                        if (self.input[0].y != 0 || self.input[0].x != 0)
                        {
                            Downcast.GlidingDir.Get(self).Value = new Vector2(
                                self.input[0].x,
                                self.input[0].y
                            );
                            Downcast.GlidingDir.Get(self).Value.Normalize();
                        } else if (self.mainBodyChunk.vel.x != 0)
                        {
                            Downcast.GlidingDir.Get(self).Value = Vector2.right * Math.Sign(self.mainBodyChunk.vel.x);
                        } else
                        {
                            Downcast.GlidingDir.Get(self).Value = Vector2.up;
                        }
                        Downcast.TargetGlidingDir.Get(self).Value = Downcast.GlidingDir.Get(self).Value;
                    }
                    else if (Downcast.Gliding.Get(self).Value && stopGliding)
                    {
                        Downcast.Gliding.Get(self).Value = false;
                        self.bodyMode = (self.bodyMode == DowncastEnums.PlayerBodyModeIndex.Gliding) ?
                                        Player.BodyModeIndex.Default :  // Set slugcat to default body mode if they are still in gliding mode
                                        self.bodyMode;  // Keep existing body mode if slugcat's body mode has changed
                        self.standing = self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y;  // Set slugcat standing if upper body is above lower body
                        //self.gravity = self.customPlayerGravity;  // Reset slugcat's gravity to normal
                        Downcast.NetForce.Get(null, self).Value = Vector2.zero;  // Reset slugcat's net non-gravity forces to 0
                    }

                    // Handle gliding mode
                    if (Downcast.Gliding.Get(self).Value)
                    {
                        // Hard set some base game variables that interfere with gliding
                        self.bodyMode = DowncastEnums.PlayerBodyModeIndex.Gliding;
                        self.standing = false;
                        self.dynamicRunSpeed[0] = 0f;
                        self.dynamicRunSpeed[1] = 0f;

                        // Set target gliding dir and modify gliding dir
                        bool directionHeld = self.input[0].y != 0 || self.input[0].x != 0;
                        bool againstWall = self.bodyChunks[1].ContactPoint.x != 0 || self.bodyChunks[0].ContactPoint.x != 0;
                        float turningAngle = 0f;
                        if (againstWall)
                        {
                            if (self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y)
                            {
                                Downcast.TargetGlidingDir.Get(self).Value = Vector2.down;
                            } else
                            {
                                Downcast.TargetGlidingDir.Get(self).Value = Vector2.up;
                            }
                        } else if (directionHeld)
                        {
                            Downcast.TargetGlidingDir.Get(self).Value = new Vector2(
                                self.input[0].x,
                                self.input[0].y
                            );
                            Downcast.TargetGlidingDir.Get(self).Value.Normalize();
                        }
                        float angleError = Vector2.SignedAngle(Downcast.TargetGlidingDir.Get(self).Value, Downcast.GlidingDir.Get(self).Value) * Mathf.PI / 180f;
                        turningAngle = -angleError * Downcast.GlidingTurnCoef.Get(self).Value;
                        Downcast.GlidingDir.Get(self).Value = new Vector2(
                            Mathf.Cos(turningAngle) * Downcast.GlidingDir.Get(self).Value.x - Mathf.Sin(turningAngle) * Downcast.GlidingDir.Get(self).Value.y,
                            Mathf.Sin(turningAngle) * Downcast.GlidingDir.Get(self).Value.x + Mathf.Cos(turningAngle) * Downcast.GlidingDir.Get(self).Value.y
                        );

                        // Calculate gliding forces
                        Vector2 averageVel = (self.bodyChunks[0].vel + self.bodyChunks[1].vel) / 2;  // Average velocity of body chunks
                        Vector2 velDir = averageVel.normalized;  // Get unit vector pointing in direction of average velocity
                        float dragCoef = Downcast.GlidingDragCoef.Get(self).Value;
                        float liftCoef = Downcast.GlidingLiftCoef.Get(self).Value;
                        float glidingAirFriction = Downcast.GlidingAirFriction.Get(self).Value;
                        Vector2 glidingDir = Downcast.GlidingDir.Get(self).Value;
                        Vector2 dragForce = Vector2.up * glidingDir.x * glidingDir.x * self.customPlayerGravity * dragCoef;
                        Vector2 liftForce = Vector2.zero;
                        if (averageVel.y < 0)
                        {
                            liftForce += -liftCoef * glidingDir.x * glidingDir.x * averageVel.y * new Vector2(Math.Sign(glidingDir.x), 1f);
                        }
                        if (glidingDir.y > 0)
                        {
                            liftForce += glidingDir.y * averageVel.x * new Vector2(-Downcast.GlidingUpwardCoefX.Get(self).Value, Math.Sign(averageVel.x) * Downcast.GlidingUpwardCoefY.Get(self).Value);
                        }
                        Vector2 frictionForce = glidingAirFriction * -averageVel;
                        Downcast.NetForce.Get(null, self).Value = dragForce + liftForce + frictionForce;

                        // Keep slugcat's body chunks aligned with gliding dir
                        self.bodyChunks[0].vel += Downcast.GlidingDir.Get(self).Value * 4f * self.EffectiveRoomGravity;
                        self.bodyChunks[1].vel -= Downcast.GlidingDir.Get(self).Value * 4f * self.EffectiveRoomGravity;

                        // Handle transitioning from gliding to beams
                        if (
                            self.input[0].y > 0 &&
                            (
                                self.room.GetTile(self.mainBodyChunk.pos).verticalBeam ||
                                self.room.GetTile(self.mainBodyChunk.pos).horizontalBeam
                            )
                        )
                        {
                            // Adapted from MovementUpdate
                            self.bodyMode = Player.BodyModeIndex.ClimbingOnBeam;
                            if (self.room.GetTile(self.mainBodyChunk.pos).verticalBeam)
                            {
                                self.room.PlaySound(SoundID.Slugcat_Grab_Beam, self.mainBodyChunk, false, 0.2f, 1f);
                                self.animation = Player.AnimationIndex.ClimbOnBeam;
                            } else
                            {
                                self.animation = Player.AnimationIndex.HangFromBeam;
                            }
                        }
                    }
                }
                orig(self);
            };

            On.PhysicalObject.ctor += (orig, self, abstractPhysicalObject) =>
            {
                orig(self, abstractPhysicalObject);
                Downcast.NetForce.Get(null, self).Value = Vector2.zero;
            };

            On.BodyChunk.Update += (orig, self) =>
            {
                if (float.IsNaN(self.vel.y))
                {
                    Custom.LogWarning(new string[]
                    {
                        "VELY IS NAN"
                    });
                    self.vel.y = 0f;
                }
                if (float.IsNaN(self.vel.x))
                {
                    Custom.LogWarning(new string[]
                    {
                        "VELX IS NAN"
                    });
                    self.vel.x = 0f;
                }

                ////////////////////////////////////////////// REPLACED SECTION //////////////////////////////////////////////

                // Apply force of gravity, as well as net non-gravity forces.
                self.vel += new Vector2(0, -self.owner.gravity) + Downcast.NetForce.Get(null, self.owner).Value;

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////

                bool flag;
                if (ModManager.DLCShared)
                {
                    flag = self.owner.room.PointSubmerged(new Vector2(self.pos.x, self.pos.y - self.rad));
                }
                else
                {
                    flag = (self.pos.y - self.rad <= self.owner.room.FloatWaterLevel(self.pos));
                }
                if (self.owner.room.water && flag)
                {
                    if (self.vel.x > self.vel.y * 5f && (Mathf.Abs(self.vel.x) > 10f & self.vel.y < 0f) && self.submersion < 0.5f)
                    {
                        self.vel.y = self.vel.y * -0.5f;
                        self.vel.x = self.vel.x * 0.75f;
                    }
                    else
                    {
                        float effectiveRoomGravity = self.owner.EffectiveRoomGravity;
                        self.vel.y = self.vel.y + self.owner.buoyancy * effectiveRoomGravity * self.submersion;
                        self.vel *= Mathf.Lerp(self.owner.airFriction, Mathf.Lerp(self.owner.waterFriction * self.owner.waterRetardationImmunity, self.owner.waterFriction, Mathf.Pow(1f / Mathf.Max(1f, self.vel.magnitude - 10f), 0.5f)), self.submersion);
                    }
                }
                else
                {
                    self.vel *= self.owner.airFriction;
                }
                if (self.burrow || self.buried)
                {
                    float sandSubmersion = self.sandSubmersion;
                    self.buried = ((double)sandSubmersion > 0.5);
                    self.vel.y = self.vel.y + self.owner.gravity * sandSubmersion;
                    self.vel *= Mathf.Lerp(1f, self.owner.burrowFriction, sandSubmersion);
                    if (self.lastBuried != self.buried)
                    {
                        if (self.owner.room.terrain != null)
                        {
                            self.owner.room.AddObject(new SandPuffSpawner.SandPuff(Vector2.Lerp(self.lastPos, self.pos, UnityEngine.Random.value) + UnityEngine.Random.insideUnitCircle * self.rad * 0.5f, self.rad * (0.2f + UnityEngine.Random.value * 0.8f) * 0.5f, Mathf.Lerp(44f, 88f, UnityEngine.Random.value) * (self.lastBuried ? 1.5f : 1f), true));
                        }
                        self.lastBuried = self.buried;
                    }
                }
                self.lastLastPos = self.lastPos;
                self.lastPos = self.pos;
                if (self.setPos != null)
                {
                    self.pos = self.setPos.Value;
                    self.setPos = null;
                }
                else if (self.terrainCurveNormal != default(Vector2))
                {
                    self.pos.x = self.pos.x + Mathf.Abs(self.terrainCurveNormal.y) * self.vel.x;
                    self.pos.y = self.pos.y + self.vel.y;
                }
                else
                {
                    self.pos += self.vel;
                }
                self.onSlope = 0;
                self.slopeRad = self.TerrainRad;
                self.lastContactPoint = self.contactPoint;
                self.terrainCurveNormal = default(Vector2);
                if (self.collideWithTerrain && !self.buried)
                {
                    self.CheckVerticalCollision();
                    if (self.collideWithSlopes)
                    {
                        self.checkAgainstSlopesVertically();
                    }
                    self.CheckHorizontalCollision();
                }
                else
                {
                    self.contactPoint.x = 0;
                    self.contactPoint.y = 0;
                }
                if (self.owner.grabbedBy.Count == 0)
                {
                    if (self.pos.x < -self.restrictInRoomRange)
                    {
                        self.vel.x = 0f;
                        self.pos.x = -self.restrictInRoomRange;
                    }
                    else if (self.pos.x > self.owner.room.PixelWidth + self.restrictInRoomRange)
                    {
                        self.vel.x = 0f;
                        self.pos.x = self.owner.room.PixelWidth + self.restrictInRoomRange;
                    }
                    if (self.pos.y < -self.restrictInRoomRange)
                    {
                        self.vel.y = 0f;
                        self.pos.y = -self.restrictInRoomRange;
                    }
                    else if (self.pos.y > self.owner.room.PixelHeight + self.restrictInRoomRange)
                    {
                        self.vel.y = 0f;
                        self.pos.y = self.owner.room.PixelHeight + self.restrictInRoomRange;
                    }
                }
                if ((self.splashStop == 10 && (self.submersion == 0f || self.submersion == 1f)) || (self.splashStop != 10 && self.splashStop > 0))
                {
                    self.splashStop--;
                }
            };
        }
    }
}
