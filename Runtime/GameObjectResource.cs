using System;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    [Serializable]
    public struct GameObjectResource : IResource {
        public GameObject Value;

        public GameObjectResource(GameObject value) {
            Value = value;
        }
    }
}
