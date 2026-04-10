#if FFS_ECS_PHYSICS
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace FFS.Libraries.StaticEcs.Unity {

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, Const.IL2CPPNullChecks)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, Const.IL2CPPArrayBoundsChecks)]
    #endif
    public abstract class ContactEventListener<TWorld> : MonoBehaviour
        where TWorld : struct, IWorldType {

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;
        }

        [MethodImpl(AggressiveInlining)]
        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            if (World<TWorld>.Status != WorldStatus.Initialized) return;

            for (var h = 0; h < headers.Length; h++) {
                var header = headers[h];
                for (var p = 0; p < header.pairCount; p++) {
                    var pair = header.GetContactPair(p);

                    if (pair.isCollisionEnter && pair.contactCount > 0) {
                        var cp = pair.GetContactPoint(0);
                        World<TWorld>.SendEvent(new ContactEnter3DEvent {
                            ColliderA = pair.collider,
                            ColliderB = pair.otherCollider,
                            Point = cp.position,
                            Normal = cp.normal,
                            Impulse = cp.impulse,
                        });
                    }

                    if (pair.isCollisionExit) {
                        World<TWorld>.SendEvent(new ContactExit3DEvent {
                            ColliderA = pair.collider,
                            ColliderB = pair.otherCollider,
                        });
                    }
                }
            }
        }
    }
}
#endif