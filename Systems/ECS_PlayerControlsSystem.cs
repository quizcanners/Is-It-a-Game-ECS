using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace QuizCanners.ECS
{

    public class ECS_PlayerControlsSystem : SystemBase
    {
        public float3 position;

        protected override void OnUpdate()
        {
            var possn = position;
            Entities.ForEach((ref Translation pos, in PlayerControls_Component dta) => { pos.Value.xyz = possn; })
                .Schedule();
        }
    }
}
