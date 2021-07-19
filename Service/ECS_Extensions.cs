using Unity.Entities;
using Unity.Transforms;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Migration;
using Unity.Mathematics;

namespace QuizCanners.ECS
{
    
    public static class ECS_Extensions
    {
        private static EntityManager Manager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private static int _inspectedComponent = -1;

        public static ECS_ComponentCfg TryGetConfig(this ComponentType cmpType) => ECS_ComponentCfg.GetConfigFor(cmpType);
        
        public static bool Inspect(this Entity e)
        {
            bool changed = pegi.ChangeTrackStart();

            if (e.Has<PhisicsArrayDynamic_Component>())
            {
                PhisicsArrayDynamic_Component cmps = Manager.GetComponentData<PhisicsArrayDynamic_Component>(e);
                int dummy = -1;
                cmps.InspectInList(ref dummy, 0);
            }

            if (e.Has<Translation>())
            {
                var pos = e.Get<Translation>();
                "Position".write(60);
                "X".edit(20, ref pos.Value.x);
                "Y".edit(20, ref pos.Value.y);
                "Z".edit(20, ref pos.Value.z);
                if (changed)
                    e.Set(pos);
                pegi.nl();
            }

            if (e.Has<Rotation>())
            {
                var rot = e.Get<Rotation>();

                var q = rot.Value.value;

                Vector3 eul = (new Quaternion(q.x, q.y, q.z, q.w)).eulerAngles;

                "Rotation".edit(ref eul).nl();

                if (changed)
                {
                    var qt = Quaternion.Euler(eul);

                    rot.Value.value = new float4(qt.x, qt.y, qt.z, qt.w);
                    e.Set(rot);
                }

                pegi.nl();
            }

            var components = e.GetComponentTypes();

            if (_inspectedComponent >= 0 && _inspectedComponent < components.Length)
            {
                var cmpType = components[_inspectedComponent];

                var cmp = cmpType.TryGetConfig();

                if (icon.Exit.Click() || cmpType.GetNameForInspector().ClickLabel().nl())
                    _inspectedComponent = -1;

                if (cmp != null)
                    cmp.Inspect(e);
            }
            else
            {

               // Edit_Array(ref components, ref _inspectedComponent);

                for (int i=0; i<components.Length; i++)
                {
                    var cmpType = components[i];

                    var cmp = cmpType.TryGetConfig();

                    IPEGI_ListInspect pgi = cmp as IPEGI_ListInspect;

                    if (pgi != null)
                    {
                        pgi.InspectInList(ref _inspectedComponent, i);
                        pegi.nl();
                    }
                    else
                    {
                        string name = cmp == null ? cmpType.ToString() : cmp.GetNameForInspector();

                        if (icon.Enter.Click() || name.ClickLabel().nl())
                            _inspectedComponent = i;
                    }
                }
            }

            return changed;
        }
        
        static bool ExitOrDrawPEGI<T>(NativeArray<T> array, ref int index, CollectionMetaData ld = null) where T : struct
        {
            bool changed = pegi.ChangeTrackStart();

            if (index >= 0)
            {
                if (array == null || index >= array.Length || icon.List.ClickUnFocus("Return to {0} array".F(pegi.CurrentListLabel<T>(ld))).nl())
                    index = -1;
                else
                    pegi.Try_Nested_Inspect(array[index]);
            }

            return changed;
        }
        
        private static T Edit_Array<T>(ref NativeArray<T> array, ref int inspected, CollectionMetaData metaDatas = null) where T : struct
        {
            T added = default;

            if (array == null)
            {
                if ("init array".ClickUnFocus().nl())
                    array = new NativeArray<T>();
            }
            else
            {

                ExitOrDrawPEGI(array, ref inspected);

                if (inspected == -1)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        var val = array[i];
                        if (pegi.InspectValueInCollection(ref val, i, ref inspected, metaDatas).nl() &&
                            typeof(T).IsValueType)
                            array[i] = val;
                    }
                }
            }

            return added;
        }
        
        public static bool Edit_Array<T>(this string label, ref NativeArray<T> array, ref int inspected) where T : struct
        {
            label.write(PEGI_Styles.ListLabel);

            bool changed = pegi.ChangeTrackStart();
            Edit_Array(ref array, ref inspected);

            return changed;
        }

        #region Encode & Decode
        public static CfgEncoder Encode(this float3 v3) => new CfgEncoder()
            .Add_IfNotEpsilon("x", v3.x)
            .Add_IfNotEpsilon("y", v3.y)
            .Add_IfNotEpsilon("z", v3.z);

        public static CfgEncoder Encode(this quaternion q) => new CfgEncoder()
            .Add_IfNotEpsilon("x", q.value.x)
            .Add_IfNotEpsilon("y", q.value.y)
            .Add_IfNotEpsilon("z", q.value.z)
            .Add_IfNotEpsilon("w", q.value.w);

        public static float3 ToFloat3(this string data)
        {
            float3 tmp = new float3();

            var cody = new CfgDecoder(data);

            foreach (var t in cody)
            {
                var d = cody.GetData();
                switch (t)
                {
                    case "x": tmp.x = d.ToFloat(); break;
                    case "y": tmp.y = d.ToFloat(); break;
                    case "z": tmp.z = d.ToFloat(); break;
                }
            }

            return tmp;
        }

        public static quaternion Toquaternion(this string data)
        {
            quaternion tmp = new quaternion();

            var cody = new CfgDecoder(data);

            foreach (var t in cody)
            {
                var d = cody.GetData();
                switch (t)
                {
                    case "x": tmp.value.x = d.ToFloat(); break;
                    case "y": tmp.value.y = d.ToFloat(); break;
                    case "z": tmp.value.z = d.ToFloat(); break;
                    case "w": tmp.value.w = d.ToFloat(); break;
                }
            }

            return tmp;
        }
        #endregion


        #region Entity Config 
        public static List<ComponentType> GetComponentTypes(this List<ECS_ComponentCfg> cmps)
        {
            var lst = new List<ComponentType>();

            foreach (var c in cmps)
                lst.Add(c.ComponentType);

            return lst;

        }

        public static EntityArchetype ToArchetype(this List<ECS_ComponentCfg> cmps) =>
            Manager.CreateArchetype(cmps.GetComponentTypes().ToArray());

        public static Entity Instantiate(this List<ECS_ComponentCfg> cmps)
        {

            Entity e = Manager.CreateEntity();

            foreach (var c in cmps)
                c.AddComponentSetData(e);

            return e;

        }
        #endregion
        
        public static NativeArray<ComponentType> GetComponentTypes(this Entity ent) => Manager.GetComponentTypes(ent);
        
        public static Entity Instantiate(this EntityArchetype arch)
        {
            Entity e = Manager.CreateEntity(arch);
            return e;
        }

        public static void Destroy(this Entity ent) => Manager.DestroyEntity(ent);

        public static Entity Add(this Entity ent, ComponentType type)
        {
            Manager.AddComponent(ent, type);
            return ent;
        }

        public static Entity Add<T>(this Entity ent) where T : struct, IComponentData
        {
            Manager.AddComponent(ent, typeof(T));
            return ent;
        }

        public static Entity AddShared<T>(this Entity ent) where T : struct, ISharedComponentData
        {
            Manager.AddComponent(ent, typeof(T));
            return ent;
        }

        public static Entity Set<T>(this Entity ent, T dta) where T : struct, IComponentData
        {
            Manager.SetComponentData(ent, dta);
            return ent;
        }

        public static Entity SetShared<T>(this Entity ent, T dta) where T : struct, ISharedComponentData
        {
            Manager.SetSharedComponentData(ent, dta);
            return ent;
        }

        public static T Get<T>(this Entity ent) where T : struct, IComponentData =>
            Manager.GetComponentData<T>(ent);

        public static T GetShared<T>(this Entity ent) where T : struct, ISharedComponentData =>
            Manager.GetSharedComponentData<T>(ent);


        public static bool Has<T>(this Entity ent) where T : struct, IComponentData =>
            Manager.HasComponent<T>(ent);

        public static bool HasShared<T>(this Entity ent) where T : struct, ISharedComponentData =>
            Manager.HasComponent<T>(ent);
    }
    
}
