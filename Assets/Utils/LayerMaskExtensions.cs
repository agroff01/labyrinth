
using UnityEngine;

namespace CustomUtils
{
    public static class LayerMaskExtensions {

        public static bool CheckForLayer(this LayerMask layerMask, int layer) {
            return ( layerMask & (1 << layer)) != 0;
        }

        public static bool LayerInLayermask(this int layer, LayerMask layerMask) {
            return ( layerMask & (1 << layer)) != 0;
        }
    }
}