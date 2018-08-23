using UnityEditor;

namespace Mirror
{
    [CustomEditor(typeof(NetworkTransformVisualizer), true)]
    [CanEditMultipleObjects]
    public class NetworkTransformVisualizerEditor : NetworkBehaviourInspector
    {
        internal override bool hideScriptField
        {
            get
            {
                return true;
            }
        }
    }
}
