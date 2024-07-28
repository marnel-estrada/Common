using CommonEcs.Math;
using Unity.Mathematics;

namespace CommonEcs {
    public static class QuaternionExtensions {
        public static quaternion ToQuaternion(float3 euler) {
            // Note that these are still in degrees
            float yaw = euler.y;
            float pitch = euler.x;
            float roll = euler.z;
            
            yaw = math.radians(yaw);
            pitch = math.radians(pitch);
            roll = math.radians(roll);
            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = (float)math.sin((double)rollOver2);
            float cosRollOver2 = (float)math.cos((double)rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = (float)math.sin((double)pitchOver2);
            float cosPitchOver2 = (float)math.cos((double)pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = (float)math.sin((double)yawOver2);
            float cosYawOver2 = (float)math.cos((double)yawOver2);
            
            quaternion result;
            result.value.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.value.x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.value.y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            result.value.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

            return result;
        }

        public static float3 ToEuler(this quaternion quat) {
            ref float4 quatValue = ref quat.value;
            
            float sqw = quatValue.w * quatValue.w;
            float sqx = quatValue.x * quatValue.x;
            float sqy = quatValue.y * quatValue.y;
            float sqz = quatValue.z * quatValue.z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = quatValue.x * quatValue.w - quatValue.y * quatValue.z;
            
            float3 v;

            if (test > 0.4995f*unit) { // singularity at north pole
                v.y = 2f * math.atan2(quatValue.y, quatValue.x);
                v.x = math.PI / 2;
                v.z = 0;
                return NormalizeAngles(math.degrees(v));
            }
            
            if (test<-0.4995f*unit) { // singularity at south pole
                v.y = -2f * math.atan2(quatValue.y, quatValue.x);
                v.x = -math.PI / 2;
                v.z = 0;
                return NormalizeAngles(math.degrees(v));
            }
            
            quaternion q = new quaternion(quatValue.w, quatValue.z, quatValue.x, quatValue.y);
            ref float4 qValue = ref q.value;
            v.y = (float)math.atan2(2f * qValue.x * qValue.w + 2f * qValue.y * qValue.z, 1 - 2f * (qValue.z * qValue.z + qValue.w * qValue.w));     // Yaw
            v.x = (float)math.asin(2f * (qValue.x * qValue.z - qValue.w * qValue.y));                             // Pitch
            v.z = (float)math.atan2(2f * qValue.x * qValue.y + 2f * qValue.z * qValue.w, 1 - 2f * (qValue.y * qValue.y + qValue.z * qValue.z));      // Roll
            return NormalizeAngles(math.degrees(v));
        }
        
        private static float3 NormalizeAngles(float3 angles) {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            
            return angles;
        }

        private static float NormalizeAngle(float angle) {
            while (angle > 360) {
                angle -= 360;
            }

            while (angle < 0) {
                angle += 360;
            }

            return angle;
        }

        public static bool IsIdentity(this quaternion q) {
            float4 value = q.value;
            return value.xyz.IsZero() && value.w.TolerantEquals(1.0f);
        }
    }
}