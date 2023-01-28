using ICities;
using UnityEngine;


namespace TestMod.Source
{
    public class Loader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            GameObject go = new GameObject("Test object");
            go.AddComponent<MyBehaviour>();
            base.OnLevelLoaded(mode);
        }
    }
}
