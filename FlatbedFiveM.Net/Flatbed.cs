using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlatbedFiveM.Net.Class.Helper;
using static CitizenFX.Core.Native.API;
using FlatbedFiveM.Net.Class;

namespace FlatbedFiveM.Net
{
    public class Flatbed : BaseScript
    {

        public static Ped PP;
        public static Vehicle LV, LF;
        public static bool ES = false;

        public Flatbed()
        {
            PP = Game.PlayerPed;
            LV = Game.PlayerPed.LastVehicle;

            DecorRegister(modDecor, 2);
            DecorRegister(towVehDecor, 3);
            DecorRegister(lastFbVehDecor, 3);
            DecorRegister(helpDecor, 2);
            DecorRegister(gHeightDecor, 1);

            EventHandlers.Add("flatbed:AddRope", new Action<int, int, int>(WorldAddRope));
            EventHandlers.Add("flatbed:SetTowingVehicle", new Action<int, int, int>(SetTowingVehicle));

            RegisterCommand("stopwinch", new Action<int, List<object>, string>((src, args, raw) =>
            {
                ES = true;
            }), false);
            RegisterCommand("unfreezeflatbed", new Action<int, List<object>, string>((src, args, raw) =>
            {
                FreezeEntityPosition(LF.Handle, false);
            }), false);

            Tick += OnTick;
            Tick += OnTick2;
        }

        public async Task OnTick2()
        {
            await Task.FromResult(0);

            try
            {
                PP = Game.PlayerPed;
                LV = Game.PlayerPed.LastVehicle;
                LF = Game.PlayerPed.LastFlatbed();                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}{ex.StackTrace}");
            }
        }

        public async Task OnTick()
        {
            await Task.FromResult(0);

            try
            {
                if (PP.IsInVehicle())
                {
                    if (PP.CurrentVehicle.IsOnAllWheels)
                    {
                        if (DecorGetFloat(PP.CurrentVehicle.Handle, gHeightDecor) == 0f) { DecorSetFloat(PP.CurrentVehicle.Handle, gHeightDecor, PP.CurrentVehicle.HeightAboveGround); }
                    }

                    if (PP.CurrentVehicle.IsThisFlatbed3()) { PP.LastFlatbed(PP.CurrentVehicle); }
                }

                if (PP.CurrentVehicle == LF)
                {
                    if (!DecorGetBool(LF.Handle, helpDecor) && LV.Model == "flatbed3")
                    {
                        DisplayHelpTextThisFrame("Press ~INPUT_VEH_BIKE_WINGS~ to lift/lower the bed.");
                        DecorSetBool(LF.Handle, helpDecor, true);
                    }

                    LF.DrawMarkerTick();
                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                    if (Game.IsControlJustPressed(0, Control.VehicleBikeWings) && !LF.AttachPosition().IsAnyVehicleNearAttachPosition(2.0F)) { await LF.DropBed(); }
                    FreezeEntityPosition(LF.Handle, false);
                    LF.IsPersistent = false;
                    if (LF.CurrentTowingVehicle().Handle != 0)
                    {
                        if (Game.IsControlPressed(2, Control.VehicleAccelerate)) { ApplyForceToEntity(Game.Player.LastVehicle.Handle, 3, 0F, -0.04F, 0F, 0F, 0F, 0F, 0, true, true, true, true, true); }
                    }
                }
                else { LF.IsPersistent = true; }

                if (LF.Exists()) //Handle != 0
                {
                    LF.DrawMarkerTick();
                    if (LF.IsFlatbedDropped()) { LF.TurnOnIndicators(); } else { LF.TurnOffIndicators(); }
                    if (LF.CurrentTowingVehicle().Handle != 0) { if (!LF.CurrentTowingVehicle().IsAttachedTo(LF))
                        {
                            LF.CurrentTowingVehicle(null);
                            TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                        } }

                    if (DoesEntityExist(GetEntityAttachedTo(LF.Handle)) && LF.CurrentTowingVehicle().Handle == 0)
                    {
                        LF.CurrentTowingVehicle(GetEntityAttachedTo(LF.Handle));
                        TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, GetEntityAttachedTo(LF.Handle));
                    }

                    if (PP.IsInVehicle())
                    {
                        if (PP.CurrentVehicle == LF.CurrentTowingVehicle())
                        {
                            await LF.CurrentTowingVehicle().DetachToFix();
                            LF.CurrentTowingVehicle().IsPersistent = false;
                            LF.CurrentTowingVehicle(null);
                            TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                        }

                        if (LV.IsAlive && LF.IsAnyPedInVehicleNearBed(2f))
                        {
                            if (!LV.IsThisFlatbed3() && LF.CurrentTowingVehicle().Handle == 0 && AC.Contains(LV.ClassType))
                            {
                                DisplayHelpTextThisFrame($"Press ~INPUT_VEH_BIKE_WINGS~ to load {PP.CurrentVehicle.LocalizedName}.");
                                if (Game.IsControlJustPressed(0, Control.VehicleBikeWings))
                                {
                                    LF.CurrentTowingVehicle(PP.CurrentVehicle);
                                    TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, PP.CurrentVehicle.Handle);
                                    PP.CurrentVehicle.IsEngineRunning = false;
                                    PP.CurrentVehicle.IsPersistent = true;
                                    if (PP.CurrentVehicle == LF.CurrentTowingVehicle()) { PP.Task.LeaveVehicle(LF.CurrentTowingVehicle(), true); }
                                    await Delay(1000);
                                    LV.AttachToFix(LF, LF.GetBoneIndex("misc_a"), LV.AttachCoords(), Vector3.Zero);
                                }
                            }
                        }

                        if (LF.AttachPosition().IsAnyVehicleNearAttachPosition(2f))
                        {
                            if (!LV.IsThisFlatbed3() && LF.CurrentTowingVehicle().Handle == 0 && LF.IsFlatbedDropped() && AC.Contains(LV.ClassType))
                            {
                                if (LV.Model != LF.Model)
                                {
                                    DisplayHelpTextThisFrame($"Press ~INPUT_VEH_BIKE_WINGS~ to load {LV.LocalizedName}.");
                                    if (Game.IsControlJustPressed(0, Control.VehicleBikeWings))
                                    {
                                        LF.CurrentTowingVehicle(LV);
                                        TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, LV.Handle);
                                        LV.IsPersistent = true;
                                        if (DecorGetFloat(LF.CurrentTowingVehicle().Handle, gHeightDecor) == 0f) { DecorSetFloat(LF.CurrentTowingVehicle().Handle, gHeightDecor, LF.CurrentTowingVehicle().HeightAboveGround); }
                                        FreezeEntityPosition(LF.Handle, true);
                                        if (PP.CurrentVehicle == LV) { PP.Task.LeaveVehicle(); }
                                        TriggerServerEvent("flatbed:AddRope",PP.Handle, LF.Handle, LV.Handle);
                                        ES = false;
                                        Rope rope = World.AddRope((RopeType)6, LF.GetBoneCoord("misc_b"), Vector3.Zero, LF.GetBoneCoord("misc_b").DistanceTo(LV.GetRopeHook()), 0.1f, false);
                                        rope.AttachEntities(LF, LF.GetBoneCoord("misc_b"), LV, LV.GetRopeHook(), LF.GetBoneCoord("misc_b").DistanceTo(LV.GetRopeHook()));
                                        rope.ActivatePhysics();
                                        do
                                        {
                                            if (ES == true)
                                            {
                                                ES = false;
                                                rope.StopWinding();
                                                rope.DetachEntity(LF);
                                                rope.DetachEntity(LV);
                                                rope.Delete();
                                                return;
                                            }
                                            rope.StartWinding();
                                            Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                            LV.PushVehicleForward();
                                            await Delay(5);
                                        } while (!(rope.Length <= 1.9f));
                                        rope.StopWinding();
                                        rope.DetachEntity(LF);
                                        rope.DetachEntity(LV);
                                        rope.Delete();
                                        FreezeEntityPosition(LF.Handle, false);
                                        LV.AttachToFix(LF, LF.GetBoneIndex("misc_a"), LV.AttachCoords(), Vector3.Zero);
                                    }
                                }
                            }


                        }
                    }

                    if (!PP.IsInVehicle() && PP.Position.DistanceTo(LF.GetBoneCoord("misc_a")) <= 3f)
                    {
                        if (World.GetDistance(LF.CurrentTowingVehicle().Position, PP.Position) <= 3f)
                        {
                            DisplayHelpTextThisFrame($"Press ~INPUT_VEH_BIKE_WINGS~ to unload {LF.CurrentTowingVehicle().LocalizedName}.");
                            if (Game.IsControlJustPressed(0, Control.VehicleBikeWings))
                            {
                                await LF.CurrentTowingVehicle().DetachToFix();
                                FreezeEntityPosition(LF.Handle, true);
                                do
                                {
                                    LF.CurrentTowingVehicle().PushVehicleBack();
                                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                    await Delay(5);
                                } while (!(LF.CurrentTowingVehicle().Position.DistanceTo(LF.AttachPosition()) <= 2f));
                                FreezeEntityPosition(LF.Handle, false);
                                LF.CurrentTowingVehicle().IsPersistent = false;
                                LF.CurrentTowingVehicle(null);
                                TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                            }
                        }
                    }

                    if (!PP.IsInVehicle() && LF.CurrentTowingVehicle().Handle == 0 && LF.IsFlatbedDropped() && LF.AttachPosition().IsAnyVehicleNearAttachPosition(2f))
                    {
                        Vehicle TV = PP.Position.WorldGetClosestVehicle();
                        if (TV.Model != LF.Model && AC.Contains(TV.ClassType) && PP.Position.DistanceTo(TV.GetRopeHook()) <= 1.5f)
                        {
                            DisplayHelpTextThisFrame($"Press ~INPUT_VEH_BIKE_WINGS~ to load {TV.LocalizedName}.");
                            if (Game.IsControlJustPressed(0, Control.VehicleBikeWings))
                            {
                                LF.CurrentTowingVehicle(TV);
                                TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, TV.Handle);
                                TV.IsPersistent = true;
                                if (TV.IsOnAllWheels)
                                {
                                    if (DecorGetFloat(TV.Handle, gHeightDecor) == 0f) { DecorSetFloat(TV.Handle, gHeightDecor, TV.HeightAboveGround); }
                                }
                                FreezeEntityPosition(LF.Handle, true);
                                TriggerServerEvent("flatbed:AddRope", PP.Handle, LF.Handle, LV.Handle);
                                ES = false;
                                Rope rope = World.AddRope((RopeType)6, LF.GetBoneCoord("misc_b"), Vector3.Zero, LF.GetBoneCoord("misc_b").DistanceTo(TV.GetRopeHook()), 0.1f, false);
                                rope.AttachEntities(LF, LF.GetBoneCoord("misc_b"), TV, TV.GetRopeHook(), LF.GetBoneCoord("misc_b").DistanceTo(TV.GetRopeHook()));
                                rope.ActivatePhysics();
                                do
                                {
                                    if (ES == true)
                                    {
                                        ES = false;
                                        rope.StopWinding();
                                        rope.DetachEntity(LF);
                                        rope.DetachEntity(TV);
                                        rope.Delete();
                                        return;
                                    }
                                    rope.StartWinding();
                                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                    TV.PushVehicleForward();
                                    await Delay(5);
                                } while (!(rope.Length <= 1.9f));
                                rope.StopWinding();
                                rope.DetachEntity(LF);
                                rope.DetachEntity(TV);
                                rope.Delete();
                                FreezeEntityPosition(LF.Handle, false);
                                TV.AttachToFix(LF, LF.GetBoneIndex("misc_a"), TV.AttachCoords(), Vector3.Zero);
                            }
                        }
                    }

                    if (World.GetDistance(LF.CurrentTowingVehicle().Position, LF.Position) >= 20f)
                    {
                        LF.CurrentTowingVehicle().IsPersistent = false;
                        LF.CurrentTowingVehicle(null);
                        TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}{ex.StackTrace}");
            }
        }

        private async void WorldAddRope(int Ply, int FB, int Veh)
        {
            if (PP.Handle != Ply)
            {
                List<Vehicle> vehicles = new List<Vehicle>(World.GetAllVehicles());
                Vehicle _fb = vehicles.Find(x => x.Handle == FB);
                Vehicle _veh = vehicles.Find(x => x.Handle == Veh);

                if (_fb.Position.DistanceTo(PP.Position) <= 50f)
                {
                    Rope rope = World.AddRope((RopeType)6, _fb.GetBoneCoord("misc_b"), Vector3.Zero, _fb.GetBoneCoord("misc_b").DistanceTo(_veh.GetRopeHook()), 0.1f, false);
                    rope.AttachEntities(_fb, _fb.GetBoneCoord("misc_b"), LV, _veh.GetRopeHook(), _fb.GetBoneCoord("misc_b").DistanceTo(_veh.GetRopeHook()));
                    rope.ActivatePhysics();
                    do
                    {
                        rope.StartWinding();
                        await Delay(5);
                    } while (!(rope.Length <= 1.8f));
                    rope.StopWinding();
                    rope.DetachEntity(_fb);
                    rope.DetachEntity(_veh);
                    rope.Delete();
                }            
            }       
        }

        private void SetTowingVehicle(int Ply, int FB, int Veh)
        {
            if (PP.Handle != Ply)
            {
                List<Vehicle> vehicles = new List<Vehicle>(World.GetAllVehicles());
                Vehicle _fb = vehicles.Find(x => x.Handle == FB);
                Vehicle _veh = vehicles.Find(x => x.Handle == Veh);
                DecorSetInt(_fb.Handle, towVehDecor, _veh.Handle);
            }
        }
    }
}
