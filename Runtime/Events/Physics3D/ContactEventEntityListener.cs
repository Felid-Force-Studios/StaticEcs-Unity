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
    public abstract class ContactEventEntityListener<TWorld> : MonoBehaviour
        where TWorld : struct, IWorldType {

        [SerializeField] private bool sendNonEntityEvents;
        [SerializeField] private bool manageComponents;

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;
        }

        [MethodImpl(AggressiveInlining)]
        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            if (World<TWorld>.Status != WorldStatus.Initialized) return;
            if (!World<TWorld>.HasResource<ContactColliderEntityMap>()) return;
            ref var map = ref World<TWorld>.GetResource<ContactColliderEntityMap>();

            for (var h = 0; h < headers.Length; h++) {
                var header = headers[h];
                for (var p = 0; p < header.pairCount; p++) {
                    var pair = header.GetContactPair(p);
                    var colliderA = pair.collider;
                    var colliderB = pair.otherCollider;

                    if (pair.isCollisionEnter && pair.contactCount > 0) {
                        ProcessEnter(ref map, pair, colliderA, colliderB);
                    }

                    if (pair.isCollisionExit) {
                        ProcessExit(ref map, colliderA, colliderB);
                    }
                }
            }
        }

        [MethodImpl(AggressiveInlining)]
        private void ProcessEnter(ref ContactColliderEntityMap map, ContactPair pair, Collider colliderA, Collider colliderB) {
            var cp = pair.GetContactPoint(0);

            if (sendNonEntityEvents) {
                World<TWorld>.SendEvent(new ContactEnter3DEvent {
                    ColliderA = colliderA,
                    ColliderB = colliderB,
                    Point = cp.position,
                    Normal = cp.normal,
                    Impulse = cp.impulse,
                });
            }

            map.Map.TryGetValue(colliderA.GetInstanceID(), out var gidA);
            map.Map.TryGetValue(colliderB.GetInstanceID(), out var gidB);

            World<TWorld>.SendEvent(new ContactEnter3DEntityEvent {
                EntityA = gidA,
                EntityB = gidB,
                ColliderA = colliderA,
                ColliderB = colliderB,
                Point = cp.position,
                Normal = cp.normal,
                Impulse = cp.impulse,
            });

            if (manageComponents) {
                if (gidA.TryUnpack<TWorld>(out var entityA)) {
                    World<TWorld>.Components<ContactCollision3DState>.Instance.Set(entityA, new ContactCollision3DState {
                        OtherCollider = colliderB,
                        Point = cp.position,
                        Normal = cp.normal,
                        Impulse = cp.impulse,
                    });
                }

                if (gidB.TryUnpack<TWorld>(out var entityB)) {
                    World<TWorld>.Components<ContactCollision3DState>.Instance.Set(entityB, new ContactCollision3DState {
                        OtherCollider = colliderA,
                        Point = cp.position,
                        Normal = cp.normal,
                        Impulse = cp.impulse,
                    });
                }
            }
        }

        [MethodImpl(AggressiveInlining)]
        private void ProcessExit(ref ContactColliderEntityMap map, Collider colliderA, Collider colliderB) {
            if (sendNonEntityEvents) {
                World<TWorld>.SendEvent(new ContactExit3DEvent {
                    ColliderA = colliderA,
                    ColliderB = colliderB,
                });
            }

            map.Map.TryGetValue(colliderA.GetInstanceID(), out var gidA);
            map.Map.TryGetValue(colliderB.GetInstanceID(), out var gidB);

            World<TWorld>.SendEvent(new ContactExit3DEntityEvent {
                EntityA = gidA,
                EntityB = gidB,
                ColliderA = colliderA,
                ColliderB = colliderB,
            });

            if (manageComponents) {
                if (gidA.TryUnpack<TWorld>(out var entityA)) {
                    World<TWorld>.Components<ContactCollision3DState>.Instance.Delete(entityA);
                }

                if (gidB.TryUnpack<TWorld>(out var entityB)) {
                    World<TWorld>.Components<ContactCollision3DState>.Instance.Delete(entityB);
                }
            }
        }
    }
}
#endif
