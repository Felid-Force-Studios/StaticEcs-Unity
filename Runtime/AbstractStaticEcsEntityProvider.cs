namespace FFS.Libraries.StaticEcs.Unity {
    public abstract class AbstractStaticEcsEntityProvider : AbstractStaticEcsProvider {
        public event OnEntityCreated OnEntityCreated;
        
        public abstract EntityGID EntityGid { get; set; }
        
        public abstract bool CreateEntity();

        public abstract void ResolveLinks();

        public void InvokeOnCreate() {
            OnCreate();
            OnEntityCreated?.Invoke(EntityGid);
        }
    }
    
    public delegate void OnEntityCreated(EntityGID gid);
}