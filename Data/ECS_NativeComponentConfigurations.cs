using QuizCanners.Migration;
using QuizCanners.Inspect;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace QuizCanners.ECS {

 

    [TaggedType(CLASS_KEY, "Local To World")]
    public class LocalToWorldCfg : EcsComponentCfgGeneric<LocalToWorld>, IPEGI_ListInspect
    {
        #region Tagged Class
        const string CLASS_KEY = "locToW";
        public override string ClassTag => CLASS_KEY;
        #endregion

        #region Inspector

        public void InspectInList(ref int edited, int ind)
        {
            "Local To World shows mesh".write();
        }

        #endregion

    }

    [TaggedType(CLASS_KEY, "Position")]
    public class PositionCfg : EcsComponentCfgGeneric<Translation>, IPEGI_ListInspect
    {
        #region Tagged Class
        const string CLASS_KEY = "pos";
        public override string ClassTag => CLASS_KEY;
        #endregion

        Vector3 startPosition;

        public override void SetData(Entity e) => e.Set_Position(startPosition);

        #region Inspector

        protected override void InspectActiveData(ref Translation dta)
        {
            float3 val = dta.Value;

            Vector3 v3 = new Vector3(val.x, val.y, val.z);

            if ("Current Position".edit(ref v3))
            {
                dta.Value = new float3(v3.x, v3.y, v3.z);
            }
        }

        public void InspectInList(ref int edited, int ind) {

           "Position".edit(ref startPosition);

            if (icon.Enter.Click())
                edited = ind;
        }
        
        #endregion

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotZero("pos", startPosition);

        public override void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "pos": startPosition = data.ToVector3(); break;
            }
        }
        #endregion

    }

    [TaggedType(CLASS_KEY, "Rotation")]
    public class RotationCfg : EcsComponentCfgGeneric<Rotation>, IPEGI_ListInspect
    {

        #region Tagged class
        const string CLASS_KEY = "rot";
        public override string ClassTag => CLASS_KEY;
        #endregion

        Quaternion qt;

        public override void SetData(Entity e) => e.Set_Rotation(qt);

        #region Inspector

        public void InspectInList(ref int edited, int ind) {

            "Rotation".edit(ref qt);

            if (icon.Enter.Click())
                edited = ind;
        }

        #endregion

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add(CLASS_KEY, qt);

        public override void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case CLASS_KEY: qt = data.ToQuaternion(); break;
            }
        }
        #endregion

    }

    public static class ECS_STD_Extensions {

        public static Entity Set_Position(this Entity e, Vector3 pos) => e.Set(new Translation { Value = new float3(pos.x, pos.y, pos.z) });

        public static Entity Set_Rotation(this Entity e, Quaternion qt) => e.Set(new Rotation { Value = new quaternion { value = new float4(qt.x, qt.y, qt.z, qt.w) } });
    }
 
    
}