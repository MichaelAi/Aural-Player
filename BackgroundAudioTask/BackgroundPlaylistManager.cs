/*
 * (c) Copyright Microsoft Corporation.
This source is subject to the Microsoft Public License (Ms-PL).
All other rights reserved.
*/

using BackgroundAudioTask.Model;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Media.Playback;

/*
 * This file implements a sample implementation of playlist management.
 * Make sure to note that objects of this class are to be initialized and 
 * run in background context. Using these in foreground context will lead 
 * music to stop or MediaPlayer to throw exception once the foreground app 
 * is suspended. 
 */
namespace BackgroundAudioTask
{
    /// <summary>
    /// Manage playlist information. For simplicity of this sample, we allow only one playlist
    /// </summary>
    public sealed class BackgroundPlaylistManager
    {
        #region Private members
        private static BackgroundPlaylist instance;
        #endregion

        #region Playlist management methods/properties
        public BackgroundPlaylist Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new BackgroundPlaylist();
                }
                return instance;
            }
        }

        /// <summary>
        /// Clears playlist for re-initialization
        /// </summary>
        public void ClearPlaylist()
        {
            instance = null;
        }
        #endregion
    }

    /// <summary>
    /// Implement a playlist of tracks. 
    /// If instantiated in background task, it will keep on playing once app is suspended
    /// </summary>
}

