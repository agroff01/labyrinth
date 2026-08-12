using System.Linq;
using CustomUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Labyrinth
{
    public class MarkerPlacement : MonoBehaviour
    {

        public enum PlacementMode
        {
            Deactivated = 0,
            Placing = 1 << 0,
            Deleting = 1 << 1
        }

        public InputActionReference ReadyUpAction = null;
        public InputActionReference MainAction = null;
        public InputActionReference ChangeModeAction = null;
        public InputActionReference RotateAction = null;
        public Transform Pointer = null;
        public GameObject MarkerPrefab = null;
        public Material ghostMaterial = null;


        public bool CameraFacingPlacement = false;
        public float RaycastDistance = 2f;
        public float DeleteRange = .2f;
        public LayerMask PlacableSurfaces = new();

        PlacementMode _currentStage = PlacementMode.Deactivated;

        public UnityEvent<PlacementMode> OnStageChange = new();
        GameObject _previewGhostObj = null;
        float _currentRotation = 0;
        bool _surfaceInRange = false;

        // Lambdas
        public PlacementMode CurrentStage => _currentStage;



        void Update()
        {
            // Should Checks be active?
            CheckReadyAction();
            if (CurrentStage == PlacementMode.Deactivated) return;
            
            // Update location
            UpdatePreviewGhostRaycast();

            // Rotate ghost
            CheckRotateAction();

            // Placement / Delete
            CheckMainAction();

            // Change Modes?
            CheckDeleteModeAction();

        } 

        void CheckReadyAction()
        {
            if (ReadyUpAction != null && ReadyUpAction.action.WasPressedThisFrame())
            {
                switch(_currentStage)
                {
                    case PlacementMode.Deactivated:
                        // Activate Placement

                        if (!_previewGhostObj)
                        {
                            CreatePreviewGhost();
                        }

                        _previewGhostObj.SetActive(true);
                        _currentRotation = 0;


                        SetMode(PlacementMode.Placing);

                        break;
                    default:
                        // Deactivate
                        _previewGhostObj.SetActive(false);

                        SetMode(PlacementMode.Deactivated);
                        break;
                }
            }
            
        }

        void UpdatePreviewGhostRaycast()
        {
            if (CurrentStage == PlacementMode.Placing)
            {
                if (!_previewGhostObj)
                {
                    CreatePreviewGhost();
                }


                if (Physics.Raycast(Pointer.ToRay(), out var hitInfo, RaycastDistance, PlacableSurfaces, QueryTriggerInteraction.Ignore))
                {
                    _previewGhostObj.SetActive(true);
                    _surfaceInRange = true;
                    var facingDirection = CameraFacingPlacement ? Pointer.forward : -hitInfo.normal;
                    _previewGhostObj.transform.SetPositionAndRotation(hitInfo.point, Quaternion.FromToRotation(Vector3.forward, facingDirection));
                    Debug.Log(_previewGhostObj.transform.forward);
                    _previewGhostObj.transform.Rotate(_previewGhostObj.transform.forward, _currentRotation, Space.World);
                }
                else
                {
                    _previewGhostObj.SetActive(false);
                    _surfaceInRange = false;
                }
            }
        }

        void CreatePreviewGhost()
        {
            _previewGhostObj = Instantiate(MarkerPrefab);

            if (ghostMaterial)
            {
                // Setup Materials
                var renderer = _previewGhostObj.GetComponent<Renderer>();
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = ghostMaterial;
                }
                renderer.materials = materials;
                
            }
            Debug.Log("Created Ghost");
            _previewGhostObj.SetActive(false);
        }

        void CheckMainAction()
        {
            if (MainAction == null) return;

            if (CurrentStage == PlacementMode.Placing 
                && MainAction.action.WasPressedThisFrame() 
                && _previewGhostObj
                && _surfaceInRange
                && MarkerPrefab)
            {
                Instantiate(MarkerPrefab).SetPose(_previewGhostObj.GetPose());
            }
            else if (CurrentStage == PlacementMode.Deleting && Physics.Raycast(Pointer.ToRay(), out var hitInfo, RaycastDistance, PlacableSurfaces, QueryTriggerInteraction.Ignore))
            {
                var nearbyDecals = FindObjectsByType<DecalProjector>(FindObjectsSortMode.None)
                                        .Where(d => Mathf.Abs((d.transform.position - hitInfo.point).sqrMagnitude) < (DeleteRange * DeleteRange))
                                        .ToList();

                if (nearbyDecals.Count > 0)
                {
                    var closest = nearbyDecals.OrderBy(d => (d.transform.position - hitInfo.point).sqrMagnitude).FirstOrDefault();
                    if (closest != default)
                    {
                        Destroy(closest.gameObject);
                    }
                }
            }
        }

        void CheckDeleteModeAction()
        {
            if (ChangeModeAction != null && ChangeModeAction.action.WasPressedThisFrame())
            switch(_currentStage)
            {
                case PlacementMode.Placing:
                    SetMode(PlacementMode.Deleting);
                    break;
                case PlacementMode.Deleting:
                    SetMode(PlacementMode.Placing);
                    break;
            }
            
        }

        void CheckRotateAction()
        {
            
            if (_currentStage == PlacementMode.Placing && RotateAction)
            {
                var delta = RotateAction.action.ReadValue<Vector2>().y;
                _currentRotation += delta;
            }
        }


        void SetMode(PlacementMode mode)
        {
            _currentStage = mode;
            OnStageChange.Invoke(mode);
        }
    }
}
