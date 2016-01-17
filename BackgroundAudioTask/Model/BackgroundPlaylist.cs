using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Media.Playback;

namespace BackgroundAudioTask.Model
{

    public sealed class BackgroundPlaylist
    {
        #region Private members
        static String[] tracks = { };
        int CurrentTrackId = -1;
        private MediaPlayer mediaPlayer;
        private TimeSpan startPosition = TimeSpan.FromSeconds(0);
        internal BackgroundPlaylist()
        {
            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        #endregion

        #region Public properties, events and handlers
        /// <summary>
        /// Get the current track name
        /// </summary>
        public string CurrentTrackName
        {
            get
            {
                if (CurrentTrackId == -1)
                {
                    return String.Empty;
                }
                if (CurrentTrackId < tracks.Length)
                {
                    string fullUrl = tracks[CurrentTrackId];

                    return fullUrl.Split('/')[fullUrl.Split('/').Length - 1];
                }
                else
                    throw new ArgumentOutOfRangeException("Track Id is higher than total number of tracks");
            }
        }
        /// <summary>
        /// Invoked when the media player is ready to move to next track
        /// </summary>
        public event TypedEventHandler<BackgroundPlaylist, object> TrackChanged;
        #endregion

        #region MediaPlayer Handlers
        /// <summary>
        /// Handler for state changed event of Media Player
        /// </summary>
        void mediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {

            if (sender.CurrentState == MediaPlayerState.Playing && startPosition != TimeSpan.FromSeconds(0))
            {
                // if the start position is other than 0, then set it now
                sender.Position = startPosition;
                sender.Volume = 1.0;
                startPosition = TimeSpan.FromSeconds(0);
                sender.PlaybackMediaMarkers.Clear();
            }
        }

        /// <summary>
        /// Fired when MediaPlayer is ready to play the track
        /// </summary>
        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            // wait for media to be ready
            sender.Play();
            Debug.WriteLine("New Track" + this.CurrentTrackName);
            TrackChanged.Invoke(this, CurrentTrackName);
        }

        /// <summary>
        /// Handler for MediaPlayer Media Ended
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            SkipToNext();
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode.ToString());
        }
        #endregion

        #region Playlist command handlers
        /// <summary>
        /// Starts track at given position in the track list
        /// </summary>
        private void StartTrackAt(int id)
        {
            string source = tracks[id];
            CurrentTrackId = id;
            mediaPlayer.AutoPlay = false;
            mediaPlayer.SetUriSource(new Uri(source));
        }

        /// <summary>
        /// Starts a given track by finding its name
        /// </summary>
        public void StartTrackAt(string TrackName)
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i].Contains(TrackName))
                {
                    string source = tracks[i];
                    CurrentTrackId = i;
                    mediaPlayer.AutoPlay = false;
                    mediaPlayer.SetUriSource(new Uri(source));
                }
            }
        }

        /// <summary>
        /// Starts a given track by finding its name and at desired position
        /// </summary>
        public void StartTrackAt(string TrackName, TimeSpan position)
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i].Contains(TrackName))
                {
                    CurrentTrackId = i;
                    break;
                }
            }

            mediaPlayer.AutoPlay = false;

            // Set the start position, we set the position once the state changes to playing, 
            // it can be possible for a fraction of second, playback can start before we are 
            // able to seek to new start position
            mediaPlayer.Volume = 0;
            startPosition = position;
            mediaPlayer.SetUriSource(new Uri(tracks[CurrentTrackId]));
        }

        /// <summary>
        /// Play all tracks in the list starting with 0 
        /// </summary>
        public void PlayAllTracks()
        {
            StartTrackAt(0);
        }

        /// <summary>
        /// Skip to next track
        /// </summary>
        public void SkipToNext()
        {
            StartTrackAt((CurrentTrackId + 1) % tracks.Length);
        }

        /// <summary>
        /// Skip to next track
        /// </summary>
        public void SkipToPrevious()
        {
            if (CurrentTrackId == 0)
            {
                StartTrackAt(CurrentTrackId);
            }
            else
            {
                StartTrackAt(CurrentTrackId - 1);
            }
        }

        #endregion
    }
}