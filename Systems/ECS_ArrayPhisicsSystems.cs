using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace QuizCanners.ECS {
   
    
    [ExecuteInEditMode]
    public class ECS_WeightlessObjects : SystemBase
    {
        [ReadOnly] public NativeArray<int> previousArray;

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            var arr = previousArray;

            Entities.ForEach((ref PhisicsArrayDynamic_Component dta, in Translation pos) =>
                dta.testValue += arr[0] * deltaTime).Schedule();
        }
    }

    [UpdateBefore(typeof(ECS_ObjectsToArray))]
    [UpdateBefore(typeof(ECS_WeightlessObjects))]
    [ExecuteInEditMode]
    public class ECS_ArrayFlipJob : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (ECS_ObjectsToArray.enabled)
            {    
                ECS_ObjectsToArray.currentPositions.CopyTo(ECS_ObjectsToArray.previousPositions);
                ECS_ObjectsToArray.staticPositions.CopyTo(ECS_ObjectsToArray.currentPositions);
            }
            return inputDeps;
        }
    }

    public class ECS_ObjectsToArray : SystemBase {

        protected override void OnUpdate()
        {
            Entities.ForEach((ref PhisicsArrayDynamic_Component dta, in Translation pos) =>
            {
                dta.testValue += 0.1f;
            }).Run();
        }

        
        #region values
        public static bool enabled;
        private bool initialized;
        const int width = 8;
        const int length = 8;
        const int height = 2;
        public const int size = width * length * height;
        public static NativeArray<int> previousPositions;
        public static NativeArray<int> currentPositions;
        public static NativeArray<int> staticPositions;
        #endregion
        
    }
    
}