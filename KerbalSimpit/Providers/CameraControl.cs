using System;
using System.Runtime.InteropServices;
using UnityEngine;

using KerbalSimpit;
using KerbalSimpit.Utilities;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitCameraControl : MonoBehaviour
    {

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct CameraModeStruct
        {
            public byte cameraMode;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct CameraRotationalStruct
        {
            public short pitch;
            public short roll;
            public short yaw;
            public byte mask;
        }
        [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
        public struct CameraTranslationalStruct
        {
            public short X;
            public short Y;
            public short Z;
            public byte mask;
        }

        // Inbound messages
        private EventData<byte, object> CameraModeChannel;

        private EventData<byte, object> CameraRotationChannel, CameraTranslationChannel;

        private volatile byte receivedCameraControlMode, oldCameraModeControl;
        private volatile Boolean needToUpdateCamera = false;

        private CameraRotationalStruct myCameraRotation, newCameraRotation;

        private CameraTranslationalStruct myCameraTranslation, newCameraTranslation;

        private CameraManager cameraManager = CameraManager.Instance;

        public void Start()
        {

            receivedCameraControlMode = 0;
            oldCameraModeControl = 0;
            CameraModeChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived21");
            if (CameraModeChannel != null) CameraModeChannel.Add(cameraModeCallback);

            CameraRotationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived22");
            if (CameraRotationChannel != null) CameraRotationChannel.Add(cameraRotationCallback);
            CameraTranslationChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived23");
            if (CameraTranslationChannel != null) CameraTranslationChannel.Add(cameraTranslationCallback);

        }

        public void OnDestroy()
        {
            if (CameraModeChannel != null) CameraModeChannel.Remove(cameraModeCallback);
            if (CameraRotationChannel != null) CameraRotationChannel.Remove(cameraRotationCallback);
            if (CameraTranslationChannel != null) CameraTranslationChannel.Remove(cameraTranslationCallback);

        }

        public void Update(){
            if(needToUpdateCamera){
                updateCameraMode(receivedCameraControlMode);
            }
        }


        public void cameraModeCallback(byte ID, object Data){
            byte[] payload = (byte[])Data;
            receivedCameraControlMode = payload[0];
            needToUpdateCamera = true;
        }

        private void updateCameraMode(byte controlMode){
            Debug.Log("Camera update called");
            oldCameraModeControl = receivedCameraControlMode;
            needToUpdateCamera = false;

            if(controlMode == CameraControlBits.FlightBit){
                cameraManager.SetCameraFlight();
            }
            if((controlMode & CameraControlBits.MapBit) != 0){
                cameraManager.SetCameraMap();
            }
            if((controlMode & CameraControlBits.ExternalBit) != 0){
                cameraManager.SetCameraMode(CameraManager.CameraMode.External);
            }
            if((controlMode & CameraControlBits.IVABit) != 0){
                cameraManager.SetCameraIVA();
            }
            if((controlMode & CameraControlBits.InternalBit) != 0){
                cameraManager.SetCameraMode(CameraManager.CameraMode.Internal);
            }
            if(controlMode == CameraControlBits.NextBit){
                Debug.Log("Camera Mode Next");
                cameraManager.NextCamera();
            }
            if(controlMode == CameraControlBits.PreviousBit){
                Debug.Log("Camera Mode Previous");
                cameraManager.PreviousCameraMode();
            }
            
        }


        public void cameraRotationCallback(byte ID, object Data)
        {

            newCameraRotation = KerbalSimpitUtils.ByteArrayToStructure<CameraRotationalStruct>((byte[])Data);
            // Bit fields:
            // pitch = 1
            // roll = 2
            // yaw = 4
            if ((newCameraRotation.mask & (byte)1) > 0)
            {
                myCameraRotation.pitch = newCameraRotation.pitch;
            }
            if ((newCameraRotation.mask & (byte)2) > 0)
            {
                myCameraRotation.roll = newCameraRotation.roll;
            }
            if ((newCameraRotation.mask & (byte)4) > 0)
            {
                myCameraRotation.yaw = newCameraRotation.yaw;
            }
        }

        public void cameraTranslationCallback(byte ID, object Data)
        {
            newCameraTranslation = KerbalSimpitUtils.ByteArrayToStructure<CameraTranslationalStruct>((byte[])Data);
            // Bit fields:
            // X = 1
            // Y = 2
            // Z = 4
            if ((newCameraTranslation.mask & (byte)1) > 0)
            {
                myCameraTranslation.X = newCameraTranslation.X;
            }
            if ((newCameraTranslation.mask & (byte)2) > 0)
            {
                myCameraTranslation.Y = newCameraTranslation.Y;
            }
            if ((newCameraTranslation.mask & (byte)4) > 0)
            {
                myCameraTranslation.Z = newCameraTranslation.Z;
            }
        }
    }
}
