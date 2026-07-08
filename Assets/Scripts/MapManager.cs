using System.Collections.Generic;
using CustomInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Labyrinth
{
    public class MapManager : MonoBehaviour
    {
        [ForceFill] public Grid mazeGrid = null;
        public Map map = null;

        [Foldout, AssetsOnly] public MapCreationLayer MapCreationSettings = null;
        [Foldout] public List<ObjectPlacementLayer> ObjectPlacementLayers = new();
        [Foldout] public List<PathCarvingLayer> PathCarvingLayers = new();
        
        public bool UseRandomSeed = false;
        [ShowIfNot(nameof(UseRandomSeed))] public int randomizationSeed = 0;

        [HorizontalLine("Runtime", 5f, FixedColor.CherryRed)]
        [HideField] public UnityEvent<Map> OnNewMapCreated = new();
        [HideField] public UnityEvent<Map> OnObjectsPlaced = new();
        [HideField] public UnityEvent<Map> OnPathsCarved = new();
        public UnityEvent<Map> OnGenerationFinished = new();
        public bool PrintToLogOnCompletion = false;

        [SerializeField, HideField, Button(nameof(GenMap))] private bool _;
        void Start()
        {
            if (!mazeGrid) return;

            // GenMap();

            // if provided a base map, create valid grid from that map
            /// TODO: Map should check for extra edge trimming internally when baked!!

            // Place Rooms within the valid grid into valid positions (find random positions with enough spacing for a in/out path)
            /// TODO: Check Later if rooms match with correct grid scaling
            
            // Carve random paths between room openings

            // Carve random paths without knoledge of old paths

            // Translate generated map to tileset pieces

            // Fill gaps of tile logic (corners/large rooms/ceilings)

            // Placing game elements into maze

            // Occlusion Culling (Hard to do on runtime objs)

        }

        public void GenMap()
        {
            if (MapCreationSettings == null)
            {
                Debug.LogError("Tried to generate a new maze without creation setting.", this);

                throw new System.ArgumentNullException(nameof(MapCreationSettings));
            }

            // Setup Seeded Generation
            if (UseRandomSeed) randomizationSeed = Random.Range(int.MinValue, int.MaxValue);
            var rand = new System.Random(randomizationSeed);

            // Initalize Map
            map = MapCreationSettings.GetMap(ref rand);


            // Place Objects into Map
            foreach (var layer in ObjectPlacementLayers)
            {
                var newMap = layer.Execute(map, ref rand);

                /// if we need any extra processing per layer before or after applying
                /// FIXME: If this step is unneccesary then this could probably change to a ref param?
                map = newMap;
            }
            OnObjectsPlaced.Invoke(map);

            foreach (var layer in PathCarvingLayers)
            {
                map = layer.Execute(map, ref rand);
            }
            OnPathsCarved.Invoke(map);


            if (PrintToLogOnCompletion) Debug.Log(map.ToString());
            
            OnGenerationFinished.Invoke(map);
        }
    }
}
