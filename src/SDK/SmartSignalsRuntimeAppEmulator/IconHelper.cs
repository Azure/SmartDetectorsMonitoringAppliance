//-----------------------------------------------------------------------
// <copyright file="IconHelper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Helper class allowing to remove the icon from the window's title bar
    /// </summary>
    public static class IconHelper
    {
        private const int GwlExstyle = -20;
        private const int WsExDlgmodalFrame = 0x0001;
        private const int SwpNosize = 0x0001;
        private const int SwpNomove = 0x0002;
        private const int SwpNozorder = 0x0004;
        private const int SwpFrameChanged = 0x0020;
        private const uint WmSeticon = 0x0080;

        /// <summary>
        /// Gets a window.
        /// </summary>
        /// <param name="hwnd">The window's handle</param>
        /// <param name="index">The window's index</param>
        /// <returns>A pointer to the window</returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        /// <summary>
        /// Sets a window.
        /// </summary>
        /// <param name="hwnd">The window's handle</param>
        /// <param name="index">The window's index</param>
        /// <param name="newStyle">The window's style</param>
        /// <returns>A pointer to the window</returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        /// <summary>
        /// Sets a window position.
        /// </summary>
        /// <param name="hwnd">The window's handle</param>
        /// <param name="hwndInsertAfter">The handle of the window to insert after</param>
        /// <param name="x">The window's x position</param>
        /// <param name="y">The window's y position</param>
        /// <param name="width">The window's width</param>
        /// <param name="height">The window's height</param>
        /// <param name="flags">The window's flags</param>
        /// <returns>A boolean flag indicates whether the window's position was updated successfully</returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        /// <summary>
        /// Removes the icon from a given window's title bar.
        /// </summary>
        /// <param name="window">a window</param>
        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GwlExstyle);
            SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExDlgmodalFrame);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNozorder | SwpFrameChanged);
        }
    }
}
