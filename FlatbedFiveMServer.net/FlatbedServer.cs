using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace FlatbedFiveMServer.net
{
    public class FlatbedServer : BaseScript
    {
        public FlatbedServer()
        {
            EventHandlers.Add("flatbed:AddRope", new Action<int, int, int>(WorldAddRope));
            EventHandlers.Add("flatbed:SetTowingVehicle", new Action<int, int, int>(SetTowingVehicle));
        }

        private void WorldAddRope(int Ply, int FB, int Veh)
        {
            TriggerClientEvent("flatbed:AddRope",Ply, FB, Veh);
        }

        private void SetTowingVehicle(int Ply, int FB, int Veh)
        {
            TriggerClientEvent("flatbed:SetTowingVehicle", Ply, FB, Veh);
        }
    }
}
