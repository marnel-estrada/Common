using Unity.Mathematics;

namespace Common {
    public static class MathUtils {
        public static float2 InsideUnitCircle(ref Random random) {
            float angle = random.NextFloat(0, math.PI * 2);
            float radius = random.NextFloat(0.0f, 1.0f);

            float x = radius * math.cos(angle);
            float y = radius * math.sin(angle);

            return new float2(x, y);
        }

        public static float2 InsideCircle(ref Random random, float minRadius, float maxRadius) {
            float angle = random.NextFloat(0, math.PI * 2);
            float radius = random.NextFloat(minRadius, maxRadius);
            
            float x = radius * math.cos(angle);
            float y = radius * math.sin(angle);

            return new float2(x, y);
        }
    }
}