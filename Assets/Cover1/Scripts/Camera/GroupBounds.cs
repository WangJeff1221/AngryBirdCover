using System.Collections.Generic;
using UnityEngine;

namespace Cover1
{
    public static class GroupBounds
    {
        // 只合并本次发射的鸟，旧鸟不会拉走镜头。
        public static bool TryGet(List<BirdController> birds, int shotId, out Bounds bounds)
        {
            bounds = new Bounds();
            bool found = false;
            foreach (var bird in birds)
            {
                if (!bird || !bird.launched || bird.shotId != shotId) continue;
                if (!found)
                {
                    bounds = new Bounds(bird.transform.position, Vector3.zero);
                    found = true;
                }
                else bounds.Encapsulate(bird.transform.position);
            }
            return found;
        }
    }
}
