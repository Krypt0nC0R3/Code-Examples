using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.DXGI;
//using SharpDX.Windows;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;

namespace DXOverlay
{
    public class DXOverlay
    {
        #region [Public vars]
        public int XOffset { get { return _xOffset; } set { _xOffset = value; } }
        public int YOffset { get { return _yOffset; } set { _yOffset = value; } }
        #endregion

        #region [Private vars]
        private int _xOffset, _yOffset;
        private IntPtr _tragetWindowHandler;
        #endregion

        #region [Constructors]
        public DXOverlay()
        {

        }

        #endregion

        public void SetTargetWindow(string WindowName)
        {

        }


    }
}
