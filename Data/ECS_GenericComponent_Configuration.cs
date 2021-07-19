using QuizCanners.Migration;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace QuizCanners.ECS {

#pragma warning disable IDE0019 // Simplify 'default' expression
      
    
    public abstract class ECS_ComponentCfg : ICfg, IGotClassTag
    {
        #region Tagged Types
        public abstract string ClassTag { get; }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(ECS_ComponentCfg));
        private static Dictionary<ComponentType, ECS_ComponentCfg> _cfgCache;

        public static ECS_ComponentCfg GetConfigFor(ComponentType type)
        {
            if (_cfgCache == null)
            {
               _cfgCache = new Dictionary<ComponentType, ECS_ComponentCfg>();

               foreach (var cfg in all)
               {
                    var newc = Activator.CreateInstance(cfg) as ECS_ComponentCfg;
                    _cfgCache.Add(newc.ComponentType, newc);
               }
            }

            return _cfgCache.TryGet(type);
        }

        #endregion

        #region Component MGMT
        public virtual void AddComponentSetData(Entity e)
        {
            e.Add(ComponentType);
            SetData(e);
        }

        public abstract ComponentType ComponentType { get; }

        public abstract void SetData(Entity e);

        public abstract bool HasComponent(Entity e);
        public virtual CfgEncoder Encode() => new CfgEncoder();
        public virtual void Decode(string key, CfgData data) { }
        #endregion

        #region Inspector
        public virtual void Inspect(Entity entity)
        {

        }

        #endregion
    }

    
    public abstract class EcsComponentCfgGeneric<T> : ECS_ComponentCfg where T : struct, IComponentData {

        public static EntityManager Manager => ECS_Service.Manager;

        static ComponentType type;

        public override ComponentType ComponentType
        {
            get {

                if (type == null)
                {

                    try
                    {
                        type = ComponentType.ReadWrite<T>();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Couldn't get read write type for {0}".F(typeof(T)));
                        Debug.LogException(ex);
                    }
                }

                return type;
            }
        }

        public virtual T GetData(Entity e) => e.Get<T>();

        public virtual void SetData(Entity e, T cmp) => e.Set(cmp);

        public override void SetData(Entity e) => SetData(e, new T());

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);

        #region Inspector
        protected virtual void InspectActiveData(ref T dta) {
            var ipl = dta as IPEGI;
            if (ipl != null)
                ipl.Inspect();
        }

        public override void Inspect(Entity entity) {

            if (entity != null && HasComponent(entity)) {

                var cmp = GetData(entity);
                var change = pegi.ChangeTrackStart();

                InspectActiveData(ref cmp);

                if (change)
                    SetData(entity, cmp);
                
            }
        }
        #endregion

    }

    public abstract class EcsComponentSharedCfgGeneric<T> : ECS_ComponentCfg where T : struct, ISharedComponentData
    {
        public static EntityManager Manager => ECS_Service.Manager;
        
        static ComponentType type;

        public override ComponentType ComponentType
        {
            get
            {

                try
                {
                    type = ComponentType.ReadWrite<T>();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Couldn't get read write type for {0}".F(typeof(T)));
                    Debug.LogException(ex);
                }

                return type;
            }
        }

        public virtual T GetData(Entity e) => e.GetShared<T>();

        public override void SetData(Entity e) { }
        
        public virtual void SetData(Entity e, T cmp) => e.SetShared(cmp);

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);

        #region Inspector
        protected virtual void InspectActiveData(ref T dta)
        {
            var ipl = dta as IPEGI;
            if (ipl != null)
                 ipl.Inspect();
        }

        public override void Inspect(Entity entity) {

            if (entity != null && HasComponent(entity)) {

                var cmp = GetData(entity);

                var change = pegi.ChangeTrackStart();

                InspectActiveData(ref cmp);

                if (change)
                {
                    SetData(entity, cmp);
                }
            }

        }
        #endregion
    }
    
    
}
