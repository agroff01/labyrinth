using QFSW.QC;
using UnityEngine;

namespace CustomUtils
{
    public class GlobalCameraController : MonoBehaviour
    {
        public enum GameCameraType
        {
            None = 0,
            PlayerCam = 1,
            Freecam = 1 << 1
        }

        public static bool PlayerCamActive = true;
        public static bool FreecamActive = false;


        [Command]
        public static void SetCameraType(GameCameraType type)
        {
            PlayerCamActive = type == GameCameraType.PlayerCam;
            FreecamActive = type == GameCameraType.Freecam;
        }



    }
}