using QuizCanners.Migration;
using QuizCanners.Inspect;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace QuizCanners.ECS {
    
    
    #region Player Controls
    public struct PlayerControls_Component : IComponentData { }

    [TaggedType(CLASS_TAG, "Player Controls")]
    public class PlayerControlsCfg : EcsComponentCfgGeneric<PlayerControls_Component> {
        
        #region Tagged Class
        const string CLASS_TAG = "plCnt";
        public override string ClassTag => CLASS_TAG;
        #endregion

    }
    #endregion

    #region Phisics Array

    [Serializable]
    public struct PhisicsArrayDynamic_Component : IComponentData, IGotReadOnlyName, IPEGI_ListInspect, ICfg
    {
        public uint phisixIndex;
        public float testValue;

        #region Inspector

        public string GetNameForInspector() => "Phisics Array index: "+phisixIndex;

        public void InspectInList(ref int edited, int ind)
        {
            "PhisX Index".edit(80, ref phisixIndex);

            "Test value".edit(60, ref testValue);

            if (icon.Refresh.Click("Reset test value"))
                testValue = 0;
        }

        #endregion

        #region Encode & Decode

        public CfgEncoder Encode() => new CfgEncoder()
            .Add("i", phisixIndex)
            .Add("tv", testValue);

        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "i": phisixIndex = data.ToUInt(); break;
                case "tv": testValue = data.ToFloat(); break;
            }
        }

        public void Decode(CfgData data) => new CfgDecoder(data).DecodeTagsFor(ref this);

        #endregion

    }

    [TaggedType(CLASS_KEY, "Phisics Array Index")]
    public class PhisicsArrayDynamicCfg : EcsComponentCfgGeneric<PhisicsArrayDynamic_Component> {

        #region Tagged Class

        const string CLASS_KEY = "phArr";
        public override string ClassTag => CLASS_KEY;

        #endregion

        #region Set Data

        public override void SetData(Entity e) => e.Set(new PhisicsArrayDynamic_Component {phisixIndex = 0});

        #endregion
        
    }

    #endregion
    
    
}
