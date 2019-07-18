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
            DecorRegister(scoopDecor, 1);

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
                PP.LastFlatbed(PP.Position.GetNearestFlatbed());

                if (PP.CurrentVehicle == LF)
                {
                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                    if (!LF.IsControlOutside())
                    {
                        if (manualControl)
                        {
                            if (Game.IsControlPressed(0, liftKey)) { LF.DropBedManually(true); }
                            if (Game.IsControlPressed(0, lowerKey)) { LF.DropBedManually(false); }
                        }
                        else
                        {
                            if (Game.IsControlJustPressed(0, hookKey)) { await LF.DropBed(); }
                        }
                    }
                    FreezeEntityPosition(LF.Handle, false);
                    LF.IsPersistent = false;

                    if (LF.CurrentTowingVehicle().Handle != 0 && PP.IsInVehicle(LF))
                    {
                        if (Game.IsControlPressed(2, Control.VehicleAccelerate)) { ApplyForceToEntity(Game.Player.LastVehicle.Handle, 3, 0F, -0.04F, 0F, 0F, 0F, 0F, 0, true, true, true, true, true); }
                        if (Game.IsControlPressed(2, Control.VehicleBrake)) { ApplyForceToEntity(Game.Player.LastVehicle.Handle, 3, 0F, 0.04F, 0F, 0F, 0F, 0F, 0, true, true, true, true, true); }
                    }
                }
                else { LF.IsPersistent = true; }

                if (PP.IsInVehicle())
                {
                    if (PP.CurrentVehicle.IsOnAllWheels)
                    {
                        if (DecorGetFloat(PP.CurrentVehicle.Handle, gHeightDecor) == 0f) { DecorSetFloat(PP.CurrentVehicle.Handle, gHeightDecor, PP.CurrentVehicle.HeightAboveGround); }
                    }
                    if (PP.CurrentVehicle.IsThisFlatbed3() && !LFList.Contains(PP.CurrentVehicle)) { LFList.Add(PP.CurrentVehicle); }
                }

                if (LF.Exists())
                {
                    if (!PP.IsInVehicle(LF) && LF.IsControlOutside() && (PP.Position.DistanceTo(LF.ControlDummyPos()) <= 2f | PP.Position.DistanceTo(LF.ControlDummy2Pos()) <= 2f))
                    {
                        if (manualControl)
                        {
                            DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HELP"), $"{liftKey.GetButtonIcon()} {lowerKey.GetButtonIcon()}"));
                            if (Game.IsControlPressed(0, liftKey)) { LF.DropBedManually(true); }
                            if (Game.IsControlPressed(0, lowerKey)) { LF.DropBedManually(false); }
                        }
                        else
                        {
                            DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HELP"), hookKey.GetButtonIcon()));
                            if (Game.IsControlJustPressed(0, hookKey)) { await LF.DropBed(); }
                        }
                    }

                    if (!DecorGetBool(LF.Handle, helpDecor) && LV.IsThisFlatbed3())
                    {
                        if (manualControl)
                        {
                            DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HELP"), $"{liftKey.GetButtonIcon()} {lowerKey.GetButtonIcon()}"));
                        }
                        else
                        {
                            DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HELP"), hookKey.GetButtonIcon()));
                        }
                        DecorSetBool(LF.Handle, helpDecor, true);
                    }

                    if (marker) { LF.DrawMarkerTick(); }
                    if (LF.IsFlatbedDropped()) { LF.TurnOnIndicators(); } else { LF.TurnOffIndicators(); }

                    if (LF.CurrentTowingVehicle().Handle != 0)
                    {
                        if (!LF.CurrentTowingVehicle().IsAttachedTo(LF))
                        {
                            LF.CurrentTowingVehicle(null);
                            TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                        }
                    }

                    if (DoesEntityExist(GetEntityAttachedTo(LF.Handle)) && LF.CurrentTowingVehicle().Handle == 0)
                    {
                        LF.CurrentTowingVehicle(GetEntityAttachedTo(LF.Handle));
                        TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, GetEntityAttachedTo(LF.Handle));
                    }

                    if (PP.IsInVehicle()) //Player is in vehicle
                    {
                        //Detach towing vehicle if player is inside towing vehicle
                        if (PP.CurrentVehicle == LF.CurrentTowingVehicle())
                        {
                            if (LV.IsVehicleFacingFlatbed(LF)) { await LF.CurrentTowingVehicle().DetachToFix(false); } else { await LF.CurrentTowingVehicle().DetachToFix(true); }
                            LF.CurrentTowingVehicle().IsPersistent = false;
                            LF.CurrentTowingVehicle(null);
                            TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                        }

                        //Load vehicle while player is pressing hookkey on bed
                        if (LV.IsAlive && LV.Position.DistanceTo(LF.AttachDummyPos()) <= 2f)
                        {
                            if (!LV.IsThisFlatbed3() && LF.CurrentTowingVehicle().Handle == 0 && AC.Contains(LV.ClassType))
                            {
                                DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HOOK"), hookKey.GetButtonIcon(), PP.CurrentVehicle.LocalizedName));
                                if (Game.IsControlJustPressed(0, hookKey))
                                {
                                    LF.CurrentTowingVehicle(PP.CurrentVehicle);
                                    TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, PP.CurrentVehicle.Handle);
                                    PP.CurrentVehicle.IsEngineRunning = false;
                                    PP.CurrentVehicle.IsPersistent = true;
                                    if (PP.CurrentVehicle == LF.CurrentTowingVehicle()) { PP.Task.LeaveVehicle(LF.CurrentTowingVehicle(), true); }
                                    await Delay(3000);
                                    if (LV.IsVehicleFacingFlatbed(LF))
                                    {
                                        LV.AttachToFix(LF, LF.AttachDummyIndex(), LV.AttachCoords(), Vector3.Zero);
                                    }
                                    else
                                    {
                                        LV.AttachToFix(LF, LF.AttachDummyIndex(), LV.AttachCoords(), new Vector3(0f, 0f, 180f));
                                    }
                                }
                            }
                        }

                        //Load vehicle while player is pressing hookkey on attach marker
                        if (LF.AttachPosition().IsAnyVehicleNearAttachPosition(2f))
                        {
                            if (!LV.IsThisFlatbed3() && LF.CurrentTowingVehicle().Handle == 0 && LF.IsFlatbedDropped() && AC.Contains(LV.ClassType))
                            {
                                if (LV.Model != LF.Model)
                                {
                                    DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HOOK"), hookKey.GetButtonIcon(), LV.LocalizedName));
                                    if (Game.IsControlJustPressed(0, hookKey))
                                    {
                                        LF.CurrentTowingVehicle(LV);
                                        TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, LV.Handle);
                                        LV.IsPersistent = true;
                                        if (DecorGetFloat(LF.CurrentTowingVehicle().Handle, gHeightDecor) == 0f && LV.IsOnAllWheels) { DecorSetFloat(LF.CurrentTowingVehicle().Handle, gHeightDecor, LF.CurrentTowingVehicle().HeightAboveGround); }
                                        FreezeEntityPosition(LF.Handle, true);
                                        if (PP.CurrentVehicle == LV) { PP.Task.LeaveVehicle(); }

                                        if (LV.IsVehicleFacingFlatbed(LF))
                                        {
                                            do
                                            {
                                                PP.Task.GoTo(LV.GetRopeHook());
                                                await Delay(100);
                                            } while (!(PP.Position.DistanceTo(LV.GetRopeHook()) <= 1.5f));
                                        }
                                        else
                                        {
                                            do
                                            {
                                                PP.Task.GoTo(LV.GetRopeHookRear());
                                                await Delay(100);
                                            } while (!(PP.Position.DistanceTo(LV.GetRopeHookRear()) <= 1.5f));
                                        }

                                        PP.Task.ClearAll();

                                        //Vehicle heading is almost same as Flatbed
                                        if (LV.IsVehicleFacingFlatbed(LF))
                                        {
                                            TriggerServerEvent("flatbed:AddRope", PP.Handle, LF.Handle, LV.Handle);
                                            ES = false;
                                            Rope rope = World.AddRope((RopeType)6, LF.WinchDummyPos(), Vector3.Zero, LF.WinchDummyPos().DistanceTo(LV.GetRopeHook()), 0.1f, false);
                                            rope.AttachEntities(LF, LF.WinchDummyPos(), LV, LV.GetRopeHook(), LF.WinchDummyPos().DistanceTo(LV.GetRopeHook()));
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
                                                if (!LV.IsAnyPedBlockingVehicle(LF))
                                                {
                                                    rope.StartWinding();
                                                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                                }                                              
                                                await Delay(5);
                                            } while (!(rope.Length <= 1.9f));
                                            rope.StopWinding();
                                            rope.DetachEntity(LF);
                                            rope.DetachEntity(LV);
                                            rope.Delete();
                                            FreezeEntityPosition(LF.Handle, false);
                                            LV.AttachToFix(LF, LF.AttachDummyIndex(), LV.AttachCoords(), Vector3.Zero);
                                        }
                                        //Vehicle heading is the opposite of Flatbed
                                        else
                                        {
                                            TriggerServerEvent("flatbed:AddRope", PP.Handle, LF.Handle, LV.Handle);
                                            ES = false;
                                            Rope rope = World.AddRope((RopeType)6, LF.WinchDummyPos(), Vector3.Zero, LF.WinchDummyPos().DistanceTo(LV.GetRopeHookRear()), 0.1f, false);
                                            rope.AttachEntities(LF, LF.WinchDummyPos(), LV, LV.GetRopeHookRear(), LF.WinchDummyPos().DistanceTo(LV.GetRopeHookRear()));
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
                                                if (!LV.IsAnyPedBlockingVehicle(LF))
                                                {
                                                    rope.StartWinding();
                                                    Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                                }
                                                await Delay(5);
                                            } while (!(rope.Length <= 1.9f));
                                            rope.StopWinding();
                                            rope.DetachEntity(LF);
                                            rope.DetachEntity(LV);
                                            rope.Delete();
                                            FreezeEntityPosition(LF.Handle, false);
                                            LV.AttachToFix(LF, LF.AttachDummyIndex(), LV.AttachCoords(), Vector3.Zero);
                                        }                                 
                                    }
                                }
                            }
                        }
                    }
                    else //Player is not in vehicle
                    {
                        //Unload vehicle while flatbed is lowered
                        if (LF.IsFlatbedDropped() && PP.Position.DistanceTo(LF.AttachDummyPos()) <= 3f)
                        {
                            if (World.GetDistance(LF.CurrentTowingVehicle().Position, PP.Position) <= 3f)
                            {
                                DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_UNHOOK"), hookKey.GetButtonIcon(), LF.CurrentTowingVehicle().LocalizedName));
                                if (Game.IsControlJustPressed(0, hookKey))
                                {
                                    Vehicle towVeh = LF.CurrentTowingVehicle();
                                    FreezeEntityPosition(LF.Handle, true);
                                    towVeh.SteeringScale = 0f;
                                    if (towVeh.IsVehicleFacingFlatbed(LF)) { await towVeh.DetachToFix(false); } else { await towVeh.DetachToFix(true); }
                                    await Delay(1000);

                                    if (towVeh.IsDriveable2())
                                    {
                                        PP.Task.EnterVehicle(towVeh, VehicleSeat.Driver, 5000, 1f);
                                        do
                                        {
                                            Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                            await Delay(5);
                                        } while (!(towVeh.Position.DistanceTo(LF.AttachPosition()) <= 2f));
                                    }
                                    else
                                    {
                                        do
                                        {
                                            if (towVeh.IsVehicleFacingFlatbed(LF)) { towVeh.PushVehicleBack(); } else { towVeh.PushVehicleForward(); }
                                            Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                            await Delay(5);
                                        } while (!(towVeh.Position.DistanceTo(LF.AttachPosition()) <= 2f));
                                    }

                                    FreezeEntityPosition(LF.Handle, false);
                                    towVeh.IsPersistent = false;
                                    LF.CurrentTowingVehicle(null);
                                    TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, 0);
                                }
                            }
                        }

                        if (LF.CurrentTowingVehicle().Handle == 0 && LF.IsFlatbedDropped() && LF.AttachPosition().IsAnyVehicleNearAttachPosition(2f))
                        {
                            Vehicle thatVehicle = PP.Position.WorldGetClosestVehicle();
                            //Vehicle heading is almost same as Flatbed
                            if (thatVehicle.Model != LF.Model && AC.Contains(thatVehicle.ClassType) && PP.Position.DistanceTo(thatVehicle.GetRopeHook()) <= 1.5f)
                            {
                                DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HOOK"), hookKey.GetButtonIcon(), thatVehicle.LocalizedName));
                                if (Game.IsControlJustPressed(0, hookKey))
                                {
                                    LF.CurrentTowingVehicle(thatVehicle);
                                    TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, thatVehicle.Handle);
                                    thatVehicle.IsPersistent = true;
                                    if (thatVehicle.IsOnAllWheels)
                                    {
                                        if (DecorGetFloat(thatVehicle.Handle, gHeightDecor) == 0f) { DecorSetFloat(thatVehicle.Handle, gHeightDecor, thatVehicle.HeightAboveGround); }
                                    }
                                    FreezeEntityPosition(LF.Handle, true);
                                    TriggerServerEvent("flatbed:AddRope", PP.Handle, LF.Handle, LV.Handle);
                                    ES = false;
                                    Rope rope = World.AddRope((RopeType)6, LF.WinchDummyPos(), Vector3.Zero, LF.WinchDummyPos().DistanceTo(thatVehicle.GetRopeHook()), 0.1f, false);
                                    rope.AttachEntities(LF, LF.WinchDummyPos(), thatVehicle, thatVehicle.GetRopeHook(), LF.WinchDummyPos().DistanceTo(thatVehicle.GetRopeHook()));
                                    rope.ActivatePhysics();
                                    do
                                    {
                                        if (ES == true)
                                        {
                                            ES = false;
                                            rope.StopWinding();
                                            rope.DetachEntity(LF);
                                            rope.DetachEntity(thatVehicle);
                                            rope.Delete();
                                            return;
                                        }
                                        if (!thatVehicle.IsAnyPedBlockingVehicle(LF))
                                        {
                                            rope.StartWinding();
                                            Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                        }
                                        await Delay(5);
                                    } while (!(rope.Length <= 1.9f));
                                    rope.StopWinding();
                                    rope.DetachEntity(LF);
                                    rope.DetachEntity(thatVehicle);
                                    rope.Delete();
                                    FreezeEntityPosition(LF.Handle, false);
                                    thatVehicle.AttachToFix(LF, LF.GetBoneIndex("misc_a"), thatVehicle.AttachCoords(), Vector3.Zero);
                                }
                            }

                            //Vehicle heading is the opposite of Flatbed
                            if (thatVehicle.Model != LF.Model && AC.Contains(thatVehicle.ClassType) && PP.Position.DistanceTo(thatVehicle.GetRopeHookRear()) <= 1.5f)
                            {
                                DisplayHelpTextThisFrame(String.Format(GetLangEntry("INM_FB_HOOK"), hookKey.GetButtonIcon(), thatVehicle.LocalizedName));
                                if (Game.IsControlJustPressed(0, hookKey))
                                {
                                    LF.CurrentTowingVehicle(thatVehicle);
                                    TriggerServerEvent("flatbed:SetTowingVehicle", PP.Handle, LF.Handle, thatVehicle.Handle);
                                    thatVehicle.IsPersistent = true;
                                    if (thatVehicle.IsOnAllWheels)
                                    {
                                        if (DecorGetFloat(thatVehicle.Handle, gHeightDecor) == 0f) { DecorSetFloat(thatVehicle.Handle, gHeightDecor, thatVehicle.HeightAboveGround); }
                                    }
                                    FreezeEntityPosition(LF.Handle, true);
                                    TriggerServerEvent("flatbed:AddRope", PP.Handle, LF.Handle, LV.Handle);
                                    ES = false;
                                    Rope rope = World.AddRope((RopeType)6, LF.WinchDummyPos(), Vector3.Zero, LF.WinchDummyPos().DistanceTo(thatVehicle.GetRopeHookRear()), 0.1f, false);
                                    rope.AttachEntities(LF, LF.WinchDummyPos(), thatVehicle, thatVehicle.GetRopeHookRear(), LF.WinchDummyPos().DistanceTo(thatVehicle.GetRopeHookRear()));
                                    rope.ActivatePhysics();
                                    do
                                    {
                                        if (ES == true)
                                        {
                                            ES = false;
                                            rope.StopWinding();
                                            rope.DetachEntity(LF);
                                            rope.DetachEntity(thatVehicle);
                                            rope.Delete();
                                            return;
                                        }
                                        if (!thatVehicle.IsAnyPedBlockingVehicle(LF))
                                        {
                                            rope.StartWinding();
                                            Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                                        }
                                        await Delay(5);
                                    } while (!(rope.Length <= 1.9f));
                                    rope.StopWinding();
                                    rope.DetachEntity(LF);
                                    rope.DetachEntity(thatVehicle);
                                    rope.Delete();
                                    FreezeEntityPosition(LF.Handle, false);
                                    thatVehicle.AttachToFix(LF, LF.GetBoneIndex("misc_a"), thatVehicle.AttachCoords(), new Vector3(0f,0f,180f));
                                }
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
                    Rope rope = World.AddRope((RopeType)6, _fb.WinchDummyPos(), Vector3.Zero, _fb.WinchDummyPos().DistanceTo(_veh.GetRopeHook()), 0.1f, false);
                    rope.AttachEntities(_fb, _fb.WinchDummyPos(), LV, _veh.GetRopeHook(), _fb.WinchDummyPos().DistanceTo(_veh.GetRopeHook()));
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
