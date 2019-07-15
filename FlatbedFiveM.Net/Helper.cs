using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System.Threading.Tasks;
using System;
using System.Linq;
using CitizenFX.Core.Native;

namespace FlatbedFiveM.Net
{
    static class Helper
    {
        // Decor
        public static string modDecor = "inm_flatbed_installed";
        public static string towVehDecor = "inm_flatbed_vehicle";
        public static string lastFbVehDecor = "inm_flatbed_last";
        public static string helpDecor = "inm_flatbed_help";
        public static string gHeightDecor = "inm_flatbed_groundheight";

        // Declare
        //public static Vector3 slotP = new Vector3(0f, 0.8f, 0.1f);
        public static List<VehicleClass> AC = new List<VehicleClass>() { VehicleClass.Commercial, VehicleClass.Compacts, VehicleClass.Coupes, VehicleClass.Cycles, VehicleClass.Emergency, VehicleClass.Industrial, VehicleClass.Military, VehicleClass.Motorcycles, VehicleClass.Muscle, VehicleClass.OffRoad, VehicleClass.Sedans, VehicleClass.Service, VehicleClass.Sports, VehicleClass.SportsClassics, VehicleClass.Super, VehicleClass.SUVs, VehicleClass.Utility, VehicleClass.Vans };

        public static Vector3 AttachCoords(this Vehicle veh)
        {
            return new Vector3(0f, 0.5f, 0.1f + DecorGetFloat(veh.Handle, gHeightDecor));
        }

        public static bool IsAnyPedInVehicleNearBed(this Vehicle veh, float radius)
        {
            Vector3 pos = veh.GetBoneCoord("misc_a");
            if (IsAnyPedNearPoint(pos.X, pos.Y, pos.Z, radius))
            {
                if (Game.Player.Character.IsInVehicle())
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static Vehicle WorldGetClosestVehicle(this Vector3 pos, float distance = 20f)
        {
            List<Vehicle> vehicles = new List<Vehicle>(World.GetAllVehicles());
            return vehicles.OrderBy(x => Math.Abs(x.Position.DistanceTo(pos))).FirstOrDefault();
        }

        public static bool IsAnyVehicleNearAttachPosition(this Vector3 pos, float radius)
        {
            return IsAnyVehicleNearPoint(pos.X, pos.Y, pos.Z, radius);
        }

        public static bool IsThisFlatbed3(this Vehicle veh)
        {
            return veh.Model == "flatbed3";
        }

        public static Vehicle CurrentTowingVehicle(this Vehicle veh)
        {
            return new Vehicle(DecorGetInt(veh.Handle, towVehDecor));
        }

        public static void CurrentTowingVehicle(this Vehicle flatbed, Vehicle veh)
        {
            if (veh == null)
            {
                DecorSetInt(flatbed.Handle, towVehDecor, 0);
            }
            else
            {
                DecorSetInt(flatbed.Handle, towVehDecor, veh.Handle);
            }
        }

        public static void CurrentTowingVehicle(this Vehicle flatbed, int handle)
        {
            DecorSetInt(flatbed.Handle, towVehDecor, handle);
        }

        public static float GroundHeight(this Entity ent)
        {
            return ent.HeightAboveGround;
        }

        public static bool IsFlatbedDropped(this Vehicle veh)
        {
            bool result = false;
            switch ((int)veh.GetBoneCoord("engine").DistanceTo(veh.GetBoneCoord("misc_a")))
            {
                case 6:
                case 7:
                case 8:
                case 9:
                    {
                        result = false;
                        break;
                    }
                case 10:
                case 11:
                case 12:
                case 13:
                    {
                        result = true;
                        break;
                    }
            }
            return result;
        }

        public static Vector3 AttachPosition(this Vehicle veh)
        {
            return veh.GetBoneCoord("misc_a") - (veh.ForwardVector * 7);
        }

        public static Vector3 DetachPosition(this Vehicle veh)
        {
            return veh.GetBoneCoord("misc_a") - (veh.ForwardVector * 10);
        }

        public static void DrawMarkerTick(this Vehicle veh)
        {
            if (veh.IsFlatbedDropped() && veh.CurrentTowingVehicle().Handle == 0)
            {
                Vector3 pos = new Vector3(veh.AttachPosition().X, veh.AttachPosition().Y, veh.AttachPosition().Z - 1.0F);
                World.DrawMarker(MarkerType.VerticalCylinder, pos, Vector3.Zero, Vector3.Zero, new Vector3(2.0F, 2.0F, 3.0F), Color.FromArgb(128, 255, 0));
            }
        }

        public static void TurnOnIndicators(this Vehicle veh)
        {
            SetVehicleIndicatorLights(veh.Handle, 1, true);
            SetVehicleIndicatorLights(veh.Handle, 0, true);
            if (!veh.IsEngineRunning)
                veh.IsEngineRunning = true;
        }

        public static void TurnOffIndicators(this Vehicle veh)
        {
            SetVehicleIndicatorLights(veh.Handle, 1, false);
            SetVehicleIndicatorLights(veh.Handle, 0, false);
        }

        public static void AttachToFix(this Entity entity1, Entity entity2, int boneindex, Vector3 position, Vector3 rotation)
        {
            bool fixedRot = true;
            int vertexIndex = 2;
            bool isPed = false;
            bool col = true;
            bool useSoftPinning = true;
            AttachEntityToEntity(entity1.Handle, entity2.Handle, boneindex, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, false, useSoftPinning, col, isPed, vertexIndex, fixedRot);
        }

        public static void AttachToPhysically(this Entity entity1, Entity entity2, int boneindex1, int boneindex2, Vector3 position1, Vector3 rotation)
        {
            AttachEntityToEntityPhysically(entity1.Handle, entity2.Handle, boneindex1, boneindex2, position1.X, position1.Y, position1.Z, 0f, 0f, 0f, rotation.X, rotation.Y, rotation.Z, 5000f, true, false, true, true, 2);
        }

        public async static Task DetachToFix(this Vehicle carToDetach)
        {
            await Task.FromResult(0);
            Vehicle attachedCar = (Vehicle)carToDetach.GetEntityAttachedTo();
            Vector3 p2 = new Vector3(attachedCar.AttachCoords().X, attachedCar.AttachCoords().Y, attachedCar.AttachCoords().Z + 0.3f);
            DetachEntity(carToDetach.Handle, true, true);
            await BaseScript.Delay(10);
            carToDetach.AttachToFix(attachedCar, attachedCar.GetBoneIndex("misc_a"), p2, Vector3.Zero);
            await BaseScript.Delay(10);
            DetachEntity(carToDetach.Handle, true, true);
            await BaseScript.Delay(10);
            carToDetach.AttachToPhysically(attachedCar, attachedCar.GetBoneIndex("misc_a"), 0, p2, Vector3.Zero);
            await BaseScript.Delay(10);
            DetachEntity(carToDetach.Handle, true, true);
        }

        public static void DisplayHelpTextThisFrame(string helpText, int Shape = -1)
        {
            SetTextComponentFormat("CELL_EMAIL_BCON");
            const int maxStringLength = 99;

            int i = 0;
            while (i < helpText.Length)
            {
                AddTextComponentSubstringPlayerName(helpText.Substring(i, System.Math.Min(maxStringLength, helpText.Length - i)));
                i += maxStringLength;
            }
            DisplayHelpTextFromStringLabel(0, false, true, Shape);
        }

        public static bool Cheating(string Cheat)
        {
            return HasCheatStringJustBeenEntered((uint)Game.GenerateHash(Cheat));
        }

        public static Vehicle LastFlatbed(this Ped ped)
        {
            return new Vehicle(DecorGetInt(ped.Handle, lastFbVehDecor));
        }

        public static void LastFlatbed(this Ped ped, Vehicle veh)
        {
            DecorSetInt(ped.Handle, lastFbVehDecor, veh.Handle);
        }

        public static void PushVehicleBack(this Vehicle veh)
        {
            ApplyForceToEntity(veh.Handle, 3, 0f, -0.2F, 0f, 0.0f, 0f, 0f, 0, true, true, true, true, true);
        }

        public static void PushVehicleForward(this Vehicle veh)
        {
            ApplyForceToEntity(veh.Handle, 3, 0f, 0.1F, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);
        }

        public static void StartWinding(this Rope rope)
        {
            StartRopeWinding(rope.Handle);
        }

        public static void StopWinding(this Rope rope)
        {
            StopRopeWinding(rope.Handle);
        }

        public static Vector3 GetRopeHook(this Vehicle veh)
        {
            if (veh.HasBone("neon_f"))
                return veh.GetBoneCoord("neon_f");
            else if (veh.HasBone("bumper_f"))
                return veh.GetBoneCoord("bumper_f");
            else if (veh.HasBone("engine"))
                return veh.GetBoneCoord("engine");
            else
                return veh.Position + veh.ForwardVector;
        }

        public static Vector3 GetRopeHookRear(this Vehicle veh)
        {
            if (veh.HasBone("neon_b"))
                return veh.GetBoneCoord("neon_b");
            else if (veh.HasBone("bumper_r"))
                return veh.GetBoneCoord("bumper_r");
            else if (veh.HasBone("trunk"))
                return veh.GetBoneCoord("trunk");
            else
                return veh.Position + veh.ForwardVector;
        }

        public async static Task DropBed(this Vehicle veh)
        {
            await Task.FromResult(0);

            if (veh.IsAlive)
            {
                if (!veh.IsEngineRunning)
                    veh.IsEngineRunning = true;
                veh.TurnOnIndicators();
                int soundId = Audio.PlaySoundFromEntity(veh, "Garage_Open", "CAR_STEAL_2_SOUNDSET");
                await BaseScript.Delay(500);
                float closeFloat = 0.03F;
                float openFloat = 0.26F;

                switch ((int)veh.GetBoneCoord("engine").DistanceTo(veh.GetBoneCoord("misc_a")))
                {
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        {
                            float initPos = closeFloat;
                            do
                            {
                                initPos += 0.0006F;
                                N_0xf8ebccc96adb9fb7(veh.Handle, initPos, false);
                                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                await BaseScript.Delay(1);
                            } while (!(initPos >= openFloat));
                            N_0xf8ebccc96adb9fb7(veh.Handle, openFloat, false);
                            break;
                        }

                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        {
                            float initPos = openFloat;
                            do
                            {
                                initPos -= 0.0006F;
                                N_0xf8ebccc96adb9fb7(veh.Handle, initPos, false);
                                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                await BaseScript.Delay(1);
                            } while (!(initPos <= closeFloat));
                            N_0xf8ebccc96adb9fb7(veh.Handle, closeFloat, false);
                            break;
                        }
                }
                Audio.StopSound(soundId);
                veh.TurnOffIndicators();
            }
        }

        public static Vehicle WorldGetNearbyVehicleWhenInsideCar(this Vector3 pos, float radius)
        {
            return new Vehicle(GetClosestVehicle(pos.X, pos.Y, pos.Z, radius, 0, 70));
        }

        public static Vector3 GetBoneCoord(this Entity entity, string boneName)
        {
            return GetWorldPositionOfEntityBone(entity.Handle, GetEntityBoneIndexByName(entity.Handle, boneName));
        }

        public static int GetBoneIndex(this Entity entity, string boneName)
        {
            return GetEntityBoneIndexByName(entity.Handle, boneName);
        }

        public static float DistanceTo(this Vector3 v, Vector3 pos)
        {
            return World.GetDistance(v, pos); //(pos - v).Length();
        }

        public static bool HasBone(this Vehicle veh, string boneName)
        {
            return !(veh.GetBoneIndex(boneName) == -1);
        }
    }
}