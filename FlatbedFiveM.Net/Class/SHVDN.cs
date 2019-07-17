using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FlatbedFiveM.Net.Class
{
    static class SHVDN
    {
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

        public static float Angle(Vector3 from, Vector3 to)
        {
            double dot = Dot(from.Normalized(), to.Normalized());
            return (float)(System.Math.Acos((dot)) * (180.0 / System.Math.PI));
        }

        public static float Dot(Vector3 left, Vector3 right) => (left.X * right.X + left.Y * right.Y + left.Z * right.Z);

        public static Vector3 Normalized(this Vector3 vector3)
        {
            return Normalize(new Vector3(vector3.X, vector3.Y, vector3.Z));
        }

        public static float Length(this Vector3 vector3)
        {
            return (float)(System.Math.Sqrt((vector3.X * vector3.X) + (vector3.Y * vector3.Y) + (vector3.Z * vector3.Z)));
        }

        public static Vector3 Normalize(this Vector3 vector3)
        {
            float length = vector3.Length();
            if (length == 0)
                return vector3;

            float num = 1 / length;
            vector3.X *= num;
            vector3.Y *= num;
            vector3.Z *= num;

            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}
