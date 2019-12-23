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
        //Config
        public static string vehiclesxml = GetResourcePath(GetCurrentResourceName());
        public static List<FlatbedData> fbVehs = new List<FlatbedData>();
        public static VehicleData vehData = new VehicleData(vehiclesxml).Instance;

        public FlatbedServer()
        {
            //Load Vehicles
            LoadVehicles();

            EventHandlers.Add("flatbed:AddRope", new Action<int, int, int>(WorldAddRope));
            EventHandlers.Add("flatbed:SetTowingVehicle", new Action<int, int, int>(SetTowingVehicle));
            EventHandlers.Add("flatbed:SendFlatbedVehicles", new Action<List<FlatbedData>>(SendFlatbedVehicles));
        }

        private void WorldAddRope(int Ply, int FB, int Veh)
        {
            TriggerClientEvent("flatbed:AddRope",Ply, FB, Veh);
        }

        private void SetTowingVehicle(int Ply, int FB, int Veh)
        {
            TriggerClientEvent("flatbed:SetTowingVehicle", Ply, FB, Veh);
        }

        private void SendFlatbedVehicles(List<FlatbedData> Vehs)
        {
            TriggerClientEvent("flatbed:SendFlatbedVehicles", Vehs);
        }

        public void LoadVehicles()
        {
            fbVehs.Clear();

            foreach (FlatbedData fd in vehData.Flatbeds)
            {
                if (!fbVehs.Contains(fd))
                    fbVehs.Add(fd);
            }
        }
    }
}
