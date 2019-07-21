using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System.Threading.Tasks;
using System;
using System.Linq;
using CitizenFX.Core.Native;
using FlatbedFiveM.Net.Class;

namespace FlatbedFiveM.Net.Class
{
    static class Helper
    {
        // Config
        public static IniConfig config = new IniConfig(GetCurrentResourceName(), "config.ini");
        public static string vehiclesxml = LoadResourceFile(GetCurrentResourceName(), "vehicles.xml");
        public static IniConfig lang = new IniConfig(GetCurrentResourceName(), "lang.ini");
        public static List<FlatbedData> fbVehs = new List<FlatbedData>();
        public static VehicleData vehData = new VehicleData(vehiclesxml).Instance;
        public static bool marker = true;
        public static Control hookKey = Control.VehicleBikeWings;
        public static Control liftKey = Control.VehicleSubAscend;
        public static Control lowerKey = Control.VehicleSubDescend;
        public static bool manualControl = false;

        // Decor
        public static string modDecor = "inm_flatbed_installed";
        public static string towVehDecor = "inm_flatbed_vehicle";
        public static string lastFbVehDecor = "inm_flatbed_last";
        public static string helpDecor = "inm_flatbed_help";
        public static string gHeightDecor = "inm_flatbed_groundheight";
        public static string scoopDecor = "inm_flatbed_scoop_pos";

        // Declare
        public static List<VehicleClass> AC = new List<VehicleClass>() { VehicleClass.Commercial, VehicleClass.Compacts, VehicleClass.Coupes, VehicleClass.Cycles, VehicleClass.Emergency, VehicleClass.Industrial, VehicleClass.Military, VehicleClass.Motorcycles, VehicleClass.Muscle, VehicleClass.OffRoad, VehicleClass.Sedans, VehicleClass.Service, VehicleClass.Sports, VehicleClass.SportsClassics, VehicleClass.Super, VehicleClass.SUVs, VehicleClass.Utility, VehicleClass.Vans };
        public static List<Vehicle> LFList = new List<Vehicle>() { Game.Player.Character.LastFlatbed() };

        public static Vehicle GetNearestFlatbed(this Vector3 pos)
        {
            return LFList.OrderBy(x => Math.Abs(x.Position.DistanceTo(pos))).First();
        }

        public static Vector3 AttachCoords(this Vehicle veh)
        {
            return new Vector3(0f, 1f, 0.1f + DecorGetFloat(veh.Handle, gHeightDecor));
        }

        //public static bool IsAnyPedInVehicleNearBed(this Vehicle veh, float radius)
        //{
        //    Vector3 pos = veh.AttachDummyPos();
        //    if (IsAnyPedNearPoint(pos.X, pos.Y, pos.Z, radius))
        //    {
        //        if (Game.Player.Character.IsInVehicle())
        //            return true;
        //        else
        //            return false;
        //    }
        //    return false;
        //}

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
            //return veh.Model == "flatbed3";
            return fbVehs.Contains(fbVehs.Find(x => veh.Model == x.Model));
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

        //public static float GroundHeight(this Entity ent)
        //{
        //    return ent.HeightAboveGround;
        //}

        public static bool IsFlatbedDropped(this Vehicle veh)
        {
            bool result = false;
            switch ((int)veh.GetBoneCoord("engine").DistanceTo(veh.AttachDummyPos()))
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
            return veh.AttachDummyPos() - (veh.ForwardVector * 7);
        }

        //public static Vector3 DetachPosition(this Vehicle veh)
        //{
        //    return veh.AttachDummyPos() - (veh.ForwardVector * 10);
        //}

        public static void DrawMarkerTick(this Vehicle veh)
        {
            if (veh.IsFlatbedDropped() && veh.CurrentTowingVehicle().Handle == 0)
            {
                Vector3 pos = new Vector3(veh.AttachPosition().X, veh.AttachPosition().Y, veh.AttachPosition().Z - 1.0F);
                World.DrawMarker(MarkerType.VerticalCylinder, pos, Vector3.Zero, Vector3.Zero, new Vector3(2.0F, 2.0F, 2.0F), Color.FromArgb(100, 128, 255, 0));
            }
            if (veh.IsControlOutside() && !Game.Player.Character.IsInVehicle(veh))
            {
                if (veh.HasBone(veh.ControlDummyBone()))
                {
                    Vector3 pos = new Vector3(veh.ControlDummyPos().X, veh.ControlDummyPos().Y, veh.ControlDummyPos().Z - 1f);
                    World.DrawMarker(MarkerType.VerticalCylinder, pos, Vector3.Zero, Vector3.Zero, new Vector3(1f, 1f, 0.5f), Color.FromArgb(100, 255, 255, 255));
                }
                if (veh.HasBone(veh.ControlDummy2Bone()))
                {
                    Vector3 pos = new Vector3(veh.ControlDummy2Pos().X, veh.ControlDummy2Pos().Y, veh.ControlDummy2Pos().Z - 1f);
                    World.DrawMarker(MarkerType.VerticalCylinder, pos, Vector3.Zero, Vector3.Zero, new Vector3(1f, 1f, 0.5f), Color.FromArgb(100, 255, 255, 255));
                }
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

        public async static Task DetachToFix(this Vehicle carToDetach, bool facingBackwards)
        {
            await Task.FromResult(0);
            Vehicle attachedCar = (Vehicle)carToDetach.GetEntityAttachedTo();
            Vector3 p2 = new Vector3(attachedCar.AttachCoords().X, attachedCar.AttachCoords().Y, attachedCar.AttachCoords().Z + 0.3f);
            DetachEntity(carToDetach.Handle, true, true);
            await BaseScript.Delay(10);
            if (facingBackwards) { carToDetach.AttachToFix(attachedCar, attachedCar.AttachDummyIndex(), p2, new Vector3(0f, 0f, 180f)); } else { carToDetach.AttachToFix(attachedCar, attachedCar.AttachDummyIndex(), p2, Vector3.Zero); }
            await BaseScript.Delay(10);
            DetachEntity(carToDetach.Handle, true, true);
            await BaseScript.Delay(10);
            carToDetach.AttachToPhysically(attachedCar, attachedCar.AttachDummyIndex(), 0, p2, Vector3.Zero);
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

        //public static bool Cheating(string Cheat)
        //{
        //    return HasCheatStringJustBeenEntered((uint)Game.GenerateHash(Cheat));
        //}

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
            ApplyForceToEntity(veh.Handle, 3, 0f, -0.3F, 0f, 0.0f, 0f, 0f, 0, true, true, true, true, true);
        }

        public static void PushVehicleForward(this Vehicle veh)
        {
            ApplyForceToEntity(veh.Handle, 3, 0f, 0.3F, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);
        }

        public static void LoadSettings()
        {
            marker = config.GetBoolValue("SETTING", "MARKER", true);
            manualControl = config.GetBoolValue("SETTING", "MANUALCONTROL", true);
            hookKey = (Control)config.GetIntValue("CONTROL", "HOOKKEY", 354);
            liftKey = (Control)config.GetIntValue("CONTROL", "LIFTKEY", 131);
            lowerKey = (Control)config.GetIntValue("CONTROL", "LOWERKEY", 132);
        }

        public static String GetButtonIcon(this Control control)
        {
            return string.Format("~{0}~", Enum.GetName(typeof(ControlButtonIcon), control));
        }

        enum ControlButtonIcon
        {
            INPUT_NEXT_CAMERA,
            INPUT_LOOK_LR,
            INPUT_LOOK_UD,
            INPUT_LOOK_UP_ONLY,
            INPUT_LOOK_DOWN_ONLY,
            INPUT_LOOK_LEFT_ONLY,
            INPUT_LOOK_RIGHT_ONLY,
            INPUT_CINEMATIC_SLOWMO,
            INPUT_SCRIPTED_FLY_UD,
            INPUT_SCRIPTED_FLY_LR,
            INPUT_SCRIPTED_FLY_ZUP,
            INPUT_SCRIPTED_FLY_ZDOWN,
            INPUT_WEAPON_WHEEL_UD,
            INPUT_WEAPON_WHEEL_LR,
            INPUT_WEAPON_WHEEL_NEXT,
            INPUT_WEAPON_WHEEL_PREV,
            INPUT_SELECT_NEXT_WEAPON,
            INPUT_SELECT_PREV_WEAPON,
            INPUT_SKIP_CUTSCENE,
            INPUT_CHARACTER_WHEEL,
            INPUT_MULTIPLAYER_INFO,
            INPUT_SPRINT,
            INPUT_JUMP,
            INPUT_ENTER,
            INPUT_ATTACK,
            INPUT_AIM,
            INPUT_LOOK_BEHIND,
            INPUT_PHONE,
            INPUT_SPECIAL_ABILITY,
            INPUT_SPECIAL_ABILITY_SECONDARY,
            INPUT_MOVE_LR,
            INPUT_MOVE_UD,
            INPUT_MOVE_UP_ONLY,
            INPUT_MOVE_DOWN_ONLY,
            INPUT_MOVE_LEFT_ONLY,
            INPUT_MOVE_RIGHT_ONLY,
            INPUT_DUCK,
            INPUT_SELECT_WEAPON,
            INPUT_PICKUP,
            INPUT_SNIPER_ZOOM,
            INPUT_SNIPER_ZOOM_IN_ONLY,
            INPUT_SNIPER_ZOOM_OUT_ONLY,
            INPUT_SNIPER_ZOOM_IN_SECONDARY,
            INPUT_SNIPER_ZOOM_OUT_SECONDARY,
            INPUT_COVER,
            INPUT_RELOAD,
            INPUT_TALK,
            INPUT_DETONATE,
            INPUT_HUD_SPECIAL,
            INPUT_ARREST,
            INPUT_ACCURATE_AIM,
            INPUT_CONTEXT,
            INPUT_CONTEXT_SECONDARY,
            INPUT_WEAPON_SPECIAL,
            INPUT_WEAPON_SPECIAL_TWO,
            INPUT_DIVE,
            INPUT_DROP_WEAPON,
            INPUT_DROP_AMMO,
            INPUT_THROW_GRENADE,
            INPUT_VEH_MOVE_LR,
            INPUT_VEH_MOVE_UD,
            INPUT_VEH_MOVE_UP_ONLY,
            INPUT_VEH_MOVE_DOWN_ONLY,
            INPUT_VEH_MOVE_LEFT_ONLY,
            INPUT_VEH_MOVE_RIGHT_ONLY,
            INPUT_VEH_SPECIAL,
            INPUT_VEH_GUN_LR,
            INPUT_VEH_GUN_UD,
            INPUT_VEH_AIM,
            INPUT_VEH_ATTACK,
            INPUT_VEH_ATTACK2,
            INPUT_VEH_ACCELERATE,
            INPUT_VEH_BRAKE,
            INPUT_VEH_DUCK,
            INPUT_VEH_HEADLIGHT,
            INPUT_VEH_EXIT,
            INPUT_VEH_HANDBRAKE,
            INPUT_VEH_HOTWIRE_LEFT,
            INPUT_VEH_HOTWIRE_RIGHT,
            INPUT_VEH_LOOK_BEHIND,
            INPUT_VEH_CIN_CAM,
            INPUT_VEH_NEXT_RADIO,
            INPUT_VEH_PREV_RADIO,
            INPUT_VEH_NEXT_RADIO_TRACK,
            INPUT_VEH_PREV_RADIO_TRACK,
            INPUT_VEH_RADIO_WHEEL,
            INPUT_VEH_HORN,
            INPUT_VEH_FLY_THROTTLE_UP,
            INPUT_VEH_FLY_THROTTLE_DOWN,
            INPUT_VEH_FLY_YAW_LEFT,
            INPUT_VEH_FLY_YAW_RIGHT,
            INPUT_VEH_PASSENGER_AIM,
            INPUT_VEH_PASSENGER_ATTACK,
            INPUT_VEH_SPECIAL_ABILITY_FRANKLIN,
            INPUT_VEH_STUNT_UD,
            INPUT_VEH_CINEMATIC_UD,
            INPUT_VEH_CINEMATIC_UP_ONLY,
            INPUT_VEH_CINEMATIC_DOWN_ONLY,
            INPUT_VEH_CINEMATIC_LR,
            INPUT_VEH_SELECT_NEXT_WEAPON,
            INPUT_VEH_SELECT_PREV_WEAPON,
            INPUT_VEH_ROOF,
            INPUT_VEH_JUMP,
            INPUT_VEH_GRAPPLING_HOOK,
            INPUT_VEH_SHUFFLE,
            INPUT_VEH_DROP_PROJECTILE,
            INPUT_VEH_MOUSE_CONTROL_OVERRIDE,
            INPUT_VEH_FLY_ROLL_LR,
            INPUT_VEH_FLY_ROLL_LEFT_ONLY,
            INPUT_VEH_FLY_ROLL_RIGHT_ONLY,
            INPUT_VEH_FLY_PITCH_UD,
            INPUT_VEH_FLY_PITCH_UP_ONLY,
            INPUT_VEH_FLY_PITCH_DOWN_ONLY,
            INPUT_VEH_FLY_UNDERCARRIAGE,
            INPUT_VEH_FLY_ATTACK,
            INPUT_VEH_FLY_SELECT_NEXT_WEAPON,
            INPUT_VEH_FLY_SELECT_PREV_WEAPON,
            INPUT_VEH_FLY_SELECT_TARGET_LEFT,
            INPUT_VEH_FLY_SELECT_TARGET_RIGHT,
            INPUT_VEH_FLY_VERTICAL_FLIGHT_MODE,
            INPUT_VEH_FLY_DUCK,
            INPUT_VEH_FLY_ATTACK_CAMERA,
            INPUT_VEH_FLY_MOUSE_CONTROL_OVERRIDE,
            INPUT_VEH_SUB_TURN_LR,
            INPUT_VEH_SUB_TURN_LEFT_ONLY,
            INPUT_VEH_SUB_TURN_RIGHT_ONLY,
            INPUT_VEH_SUB_PITCH_UD,
            INPUT_VEH_SUB_PITCH_UP_ONLY,
            INPUT_VEH_SUB_PITCH_DOWN_ONLY,
            INPUT_VEH_SUB_THROTTLE_UP,
            INPUT_VEH_SUB_THROTTLE_DOWN,
            INPUT_VEH_SUB_ASCEND,
            INPUT_VEH_SUB_DESCEND,
            INPUT_VEH_SUB_TURN_HARD_LEFT,
            INPUT_VEH_SUB_TURN_HARD_RIGHT,
            INPUT_VEH_SUB_MOUSE_CONTROL_OVERRIDE,
            INPUT_VEH_PUSHBIKE_PEDAL,
            INPUT_VEH_PUSHBIKE_SPRINT,
            INPUT_VEH_PUSHBIKE_FRONT_BRAKE,
            INPUT_VEH_PUSHBIKE_REAR_BRAKE,
            INPUT_MELEE_ATTACK_LIGHT,
            INPUT_MELEE_ATTACK_HEAVY,
            INPUT_MELEE_ATTACK_ALTERNATE,
            INPUT_MELEE_BLOCK,
            INPUT_PARACHUTE_DEPLOY,
            INPUT_PARACHUTE_DETACH,
            INPUT_PARACHUTE_TURN_LR,
            INPUT_PARACHUTE_TURN_LEFT_ONLY,
            INPUT_PARACHUTE_TURN_RIGHT_ONLY,
            INPUT_PARACHUTE_PITCH_UD,
            INPUT_PARACHUTE_PITCH_UP_ONLY,
            INPUT_PARACHUTE_PITCH_DOWN_ONLY,
            INPUT_PARACHUTE_BRAKE_LEFT,
            INPUT_PARACHUTE_BRAKE_RIGHT,
            INPUT_PARACHUTE_SMOKE,
            INPUT_PARACHUTE_PRECISION_LANDING,
            INPUT_MAP,
            INPUT_SELECT_WEAPON_UNARMED,
            INPUT_SELECT_WEAPON_MELEE,
            INPUT_SELECT_WEAPON_HANDGUN,
            INPUT_SELECT_WEAPON_SHOTGUN,
            INPUT_SELECT_WEAPON_SMG,
            INPUT_SELECT_WEAPON_AUTO_RIFLE,
            INPUT_SELECT_WEAPON_SNIPER,
            INPUT_SELECT_WEAPON_HEAVY,
            INPUT_SELECT_WEAPON_SPECIAL,
            INPUT_SELECT_CHARACTER_MICHAEL,
            INPUT_SELECT_CHARACTER_FRANKLIN,
            INPUT_SELECT_CHARACTER_TREVOR,
            INPUT_SELECT_CHARACTER_MULTIPLAYER,
            INPUT_SAVE_REPLAY_CLIP,
            INPUT_SPECIAL_ABILITY_PC,
            INPUT_CELLPHONE_UP,
            INPUT_CELLPHONE_DOWN,
            INPUT_CELLPHONE_LEFT,
            INPUT_CELLPHONE_RIGHT,
            INPUT_CELLPHONE_SELECT,
            INPUT_CELLPHONE_CANCEL,
            INPUT_CELLPHONE_OPTION,
            INPUT_CELLPHONE_EXTRA_OPTION,
            INPUT_CELLPHONE_SCROLL_FORWARD,
            INPUT_CELLPHONE_SCROLL_BACKWARD,
            INPUT_CELLPHONE_CAMERA_FOCUS_LOCK,
            INPUT_CELLPHONE_CAMERA_GRID,
            INPUT_CELLPHONE_CAMERA_SELFIE,
            INPUT_CELLPHONE_CAMERA_DOF,
            INPUT_CELLPHONE_CAMERA_EXPRESSION,
            INPUT_FRONTEND_DOWN,
            INPUT_FRONTEND_UP,
            INPUT_FRONTEND_LEFT,
            INPUT_FRONTEND_RIGHT,
            INPUT_FRONTEND_RDOWN,
            INPUT_FRONTEND_RUP,
            INPUT_FRONTEND_RLEFT,
            INPUT_FRONTEND_RRIGHT,
            INPUT_FRONTEND_AXIS_X,
            INPUT_FRONTEND_AXIS_Y,
            INPUT_FRONTEND_RIGHT_AXIS_X,
            INPUT_FRONTEND_RIGHT_AXIS_Y,
            INPUT_FRONTEND_PAUSE,
            INPUT_FRONTEND_PAUSE_ALTERNATE,
            INPUT_FRONTEND_ACCEPT,
            INPUT_FRONTEND_CANCEL,
            INPUT_FRONTEND_X,
            INPUT_FRONTEND_Y,
            INPUT_FRONTEND_LB,
            INPUT_FRONTEND_RB,
            INPUT_FRONTEND_LT,
            INPUT_FRONTEND_RT,
            INPUT_FRONTEND_LS,
            INPUT_FRONTEND_RS,
            INPUT_FRONTEND_LEADERBOARD,
            INPUT_FRONTEND_SOCIAL_CLUB,
            INPUT_FRONTEND_SOCIAL_CLUB_SECONDARY,
            INPUT_FRONTEND_DELETE,
            INPUT_FRONTEND_ENDSCREEN_ACCEPT,
            INPUT_FRONTEND_ENDSCREEN_EXPAND,
            INPUT_FRONTEND_SELECT,
            INPUT_SCRIPT_LEFT_AXIS_X,
            INPUT_SCRIPT_LEFT_AXIS_Y,
            INPUT_SCRIPT_RIGHT_AXIS_X,
            INPUT_SCRIPT_RIGHT_AXIS_Y,
            INPUT_SCRIPT_RUP,
            INPUT_SCRIPT_RDOWN,
            INPUT_SCRIPT_RLEFT,
            INPUT_SCRIPT_RRIGHT,
            INPUT_SCRIPT_LB,
            INPUT_SCRIPT_RB,
            INPUT_SCRIPT_LT,
            INPUT_SCRIPT_RT,
            INPUT_SCRIPT_LS,
            INPUT_SCRIPT_RS,
            INPUT_SCRIPT_PAD_UP,
            INPUT_SCRIPT_PAD_DOWN,
            INPUT_SCRIPT_PAD_LEFT,
            INPUT_SCRIPT_PAD_RIGHT,
            INPUT_SCRIPT_SELECT,
            INPUT_CURSOR_ACCEPT,
            INPUT_CURSOR_CANCEL,
            INPUT_CURSOR_X,
            INPUT_CURSOR_Y,
            INPUT_CURSOR_SCROLL_UP,
            INPUT_CURSOR_SCROLL_DOWN,
            INPUT_ENTER_CHEAT_CODE,
            INPUT_INTERACTION_MENU,
            INPUT_MP_TEXT_CHAT_ALL,
            INPUT_MP_TEXT_CHAT_TEAM,
            INPUT_MP_TEXT_CHAT_FRIENDS,
            INPUT_MP_TEXT_CHAT_CREW,
            INPUT_PUSH_TO_TALK,
            INPUT_CREATOR_LS,
            INPUT_CREATOR_RS,
            INPUT_CREATOR_LT,
            INPUT_CREATOR_RT,
            INPUT_CREATOR_MENU_TOGGLE,
            INPUT_CREATOR_ACCEPT,
            INPUT_CREATOR_DELETE,
            INPUT_ATTACK2,
            INPUT_RAPPEL_JUMP,
            INPUT_RAPPEL_LONG_JUMP,
            INPUT_RAPPEL_SMASH_WINDOW,
            INPUT_PREV_WEAPON,
            INPUT_NEXT_WEAPON,
            INPUT_MELEE_ATTACK1,
            INPUT_MELEE_ATTACK2,
            INPUT_WHISTLE,
            INPUT_MOVE_LEFT,
            INPUT_MOVE_RIGHT,
            INPUT_MOVE_UP,
            INPUT_MOVE_DOWN,
            INPUT_LOOK_LEFT,
            INPUT_LOOK_RIGHT,
            INPUT_LOOK_UP,
            INPUT_LOOK_DOWN,
            INPUT_SNIPER_ZOOM_IN,
            INPUT_SNIPER_ZOOM_OUT,
            INPUT_SNIPER_ZOOM_IN_ALTERNATE,
            INPUT_SNIPER_ZOOM_OUT_ALTERNATE,
            INPUT_VEH_MOVE_LEFT,
            INPUT_VEH_MOVE_RIGHT,
            INPUT_VEH_MOVE_UP,
            INPUT_VEH_MOVE_DOWN,
            INPUT_VEH_GUN_LEFT,
            INPUT_VEH_GUN_RIGHT,
            INPUT_VEH_GUN_UP,
            INPUT_VEH_GUN_DOWN,
            INPUT_VEH_LOOK_LEFT,
            INPUT_VEH_LOOK_RIGHT,
            INPUT_REPLAY_START_STOP_RECORDING,
            INPUT_REPLAY_START_STOP_RECORDING_SECONDARY,
            INPUT_SCALED_LOOK_LR,
            INPUT_SCALED_LOOK_UD,
            INPUT_SCALED_LOOK_UP_ONLY,
            INPUT_SCALED_LOOK_DOWN_ONLY,
            INPUT_SCALED_LOOK_LEFT_ONLY,
            INPUT_SCALED_LOOK_RIGHT_ONLY,
            INPUT_REPLAY_MARKER_DELETE,
            INPUT_REPLAY_CLIP_DELETE,
            INPUT_REPLAY_PAUSE,
            INPUT_REPLAY_REWIND,
            INPUT_REPLAY_FFWD,
            INPUT_REPLAY_NEWMARKER,
            INPUT_REPLAY_RECORD,
            INPUT_REPLAY_SCREENSHOT,
            INPUT_REPLAY_HIDEHUD,
            INPUT_REPLAY_STARTPOINT,
            INPUT_REPLAY_ENDPOINT,
            INPUT_REPLAY_ADVANCE,
            INPUT_REPLAY_BACK,
            INPUT_REPLAY_TOOLS,
            INPUT_REPLAY_RESTART,
            INPUT_REPLAY_SHOWHOTKEY,
            INPUT_REPLAY_CYCLEMARKERLEFT,
            INPUT_REPLAY_CYCLEMARKERRIGHT,
            INPUT_REPLAY_FOVINCREASE,
            INPUT_REPLAY_FOVDECREASE,
            INPUT_REPLAY_CAMERAUP,
            INPUT_REPLAY_CAMERADOWN,
            INPUT_REPLAY_SAVE,
            INPUT_REPLAY_TOGGLETIME,
            INPUT_REPLAY_TOGGLETIPS,
            INPUT_REPLAY_PREVIEW,
            INPUT_REPLAY_TOGGLE_TIMELINE,
            INPUT_REPLAY_TIMELINE_PICKUP_CLIP,
            INPUT_REPLAY_TIMELINE_DUPLICATE_CLIP,
            INPUT_REPLAY_TIMELINE_PLACE_CLIP,
            INPUT_REPLAY_CTRL,
            INPUT_REPLAY_TIMELINE_SAVE,
            INPUT_REPLAY_PREVIEW_AUDIO,
            INPUT_VEH_DRIVE_LOOK,
            INPUT_VEH_DRIVE_LOOK2,
            INPUT_VEH_FLY_ATTACK2,
            INPUT_RADIO_WHEEL_UD,
            INPUT_RADIO_WHEEL_LR,
            INPUT_VEH_SLOWMO_UD,
            INPUT_VEH_SLOWMO_UP_ONLY,
            INPUT_VEH_SLOWMO_DOWN_ONLY,
            INPUT_MAP_POI,
            INPUT_REPLAY_SNAPMATIC_PHOTO,
            INPUT_VEH_CAR_JUMP,
            INPUT_VEH_ROCKET_BOOST,
            INPUT_VEH_PARACHUTE,
            INPUT_VEH_BIKE_WINGS
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

        public static bool IsVehicleFacingFlatbed(this Vehicle veh, Vehicle fb)
        {
            float angle = 90f;
            return SHVDN.Angle(veh.ForwardVector, fb.Position - veh.Position) < angle;
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

                switch ((int)veh.GetBoneCoord("engine").DistanceTo(veh.AttachDummyPos()))
                {
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        {
                            float initPos = closeFloat;
                            do
                            {
                                ActivatePhysics(veh.Handle);
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
                                ActivatePhysics(veh.Handle);
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

        public static void DropBedManually(this Vehicle veh, bool isLift)
        {
            if (veh.IsAlive)
            {
                if (!veh.IsEngineRunning)
                    veh.IsEngineRunning = true;
                veh.TurnOnIndicators();
                float closeFloat = 0.03F;
                float openFloat = 0.26F;
                float scoopFloat = DecorGetFloat(veh.Handle, scoopDecor);
                switch (isLift)
                {
                    case true:
                        {
                            ActivatePhysics(veh.Handle);
                            scoopFloat -= 0.0003F;
                            if (scoopFloat <= closeFloat)
                                scoopFloat = closeFloat;
                            N_0xf8ebccc96adb9fb7(veh.Handle, scoopFloat, false);
                            DecorSetFloat(veh.Handle, scoopDecor, scoopFloat);
                            break;
                        }

                    case false:
                        {
                            ActivatePhysics(veh.Handle);
                            scoopFloat += 0.0003F;
                            if (scoopFloat >= openFloat)
                                scoopFloat = openFloat;
                            N_0xf8ebccc96adb9fb7(veh.Handle, scoopFloat, false);
                            DecorSetFloat(veh.Handle, scoopDecor, scoopFloat);
                            break;
                        }
                }
            }
        }

        public static string GetLangEntry(string langstr, string fallback)
        {
            return lang.GetStringValue(Game.Language.ToString().Trim().ToUpper(), langstr, fallback);
        }

        public static bool IsAnyPedBlockingVehicle(this Vehicle veh, Vehicle fb)
        {
            Vector3 pos = veh.GetRopeHookRear();
            if (veh.IsVehicleFacingFlatbed(fb))
                pos = veh.GetRopeHook();
            return IsAnyPedNearPoint(pos.X, pos.Y, pos.Z, 2f);
        }

        public static void LoadVehicles()
        {
            fbVehs.Clear();

            foreach (FlatbedData fd in vehData.Flatbeds)
            {
                if (!fbVehs.Contains(fd))
                    fbVehs.Add(fd);
            }
        }

        public static bool IsDriveable2(this Vehicle veh)
        {
            bool result = false;
            if (veh.IsDriveable)
                result = true;
            if (veh.LockStatus == VehicleLockStatus.Unlocked)
                result = true;
            if (veh.LockStatus == VehicleLockStatus.Locked)
                result = false;
            if (veh.LockStatus == VehicleLockStatus.LockedForPlayer)
                result = false;
            return result;
        }

        public static Vector3 AttachDummyPos(this Vehicle veh)
        {
            return veh.GetBoneCoord(fbVehs.Find(x => x.Model == veh.Model).AttachDummy);
        }

        public static Vector3 WinchDummyPos(this Vehicle veh)
        {
            return veh.GetBoneCoord(fbVehs.Find(x => x.Model == veh.Model).WinchDummy);
        }

        public static Vector3 ControlDummyPos(this Vehicle veh)
        {
            return veh.GetBoneCoord(fbVehs.Find(x => x.Model == veh.Model).ControlDummy);
        }

        public static Vector3 ControlDummy2Pos(this Vehicle veh)
        {
            return veh.GetBoneCoord(fbVehs.Find(x => x.Model == veh.Model).ControlDummy2);
        }

        public static int AttachDummyIndex(this Vehicle veh)
        {
            return veh.GetBoneIndex(fbVehs.Find(x => x.Model == veh.Model).AttachDummy);
        }

        public static string ControlDummyBone(this Vehicle veh)
        {
            return fbVehs.Find(x => x.Model == veh.Model).ControlDummy;
        }

        public static string ControlDummy2Bone(this Vehicle veh)
        {
            return fbVehs.Find(x => x.Model == veh.Model).ControlDummy2;
        }

        public static bool IsControlOutside(this Vehicle veh)
        {
            return fbVehs.Find(x => x.Model == veh.Model).ControlIsOutside;
        }

    }
}