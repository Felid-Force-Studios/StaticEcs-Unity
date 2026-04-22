namespace FFS.Libraries.StaticEcs.Unity {
    public abstract class AbstractStaticEcsEntityProvider : AbstractStaticEcsProvider {
        public abstract EntityGID EntityGid { get; set; }
        
        public abstract bool CreateEntity();

        public abstract void ResolveLinks();

        public void InvokeOnCreate() {
            OnCreate();
        }
    }
}