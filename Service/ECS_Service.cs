//using Unity.Entities;
using QuizCanners.Inspect;
using Unity.Collections;
using UnityEngine;
using QuizCanners.Utils;

namespace QuizCanners.ECS {


    [ExecuteAlways]
    public class ECS_Service : Service.BehaniourBase
    {
        public static EntityManager Manager => World.DefaultGameObjectInjectionWorld.EntityManager;
        
        public static Entity Instantiate() => Manager.CreateEntity();
        
        #region Inspector

        static int exploredEntity = -1;
        public override void Inspect()   {
            
            base.Inspect();

            pegi.nl();

            if (!Application.isPlaying)
            {
                "No ECS in Edit Mode".writeHint();

                return;
            }

            if (Manager == default)
            {

                if (World.DefaultGameObjectInjectionWorld == null)
                {
                    "Active World is null".writeWarning();

                    foreach (var world in World.All)
                    {
                        if (world.Name.Click().nl())
                            World.DefaultGameObjectInjectionWorld = world;
                    }

                    if ("Create".Click())
                        World.DefaultGameObjectInjectionWorld = new World("Node Notes");
                }

            }
            else
            {
                NativeArray<Entity> all = Manager.GetAllEntities();

                if ("Add".Click().nl())
                    Instantiate();

                if (exploredEntity != -1 && exploredEntity < all.Length)
                {
                    var ent = all[exploredEntity];

                    if (icon.Exit.Click() || ent.ToString().ClickLabel().nl())
                        exploredEntity = -1;
                    else
                        ent.Inspect();
                    
                    pegi.nl();
                }
                else
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        var e = all[i];

                        if (icon.Enter.Click())
                            exploredEntity = i;

                        e.ToString().nl();
                    }
                }
            }
        }

        #endregion

    }


    [PEGI_Inspector_Override(typeof(ECS_Service))]
    public class ECS_ServiceDrawer : PEGI_Inspector_Override { }


}