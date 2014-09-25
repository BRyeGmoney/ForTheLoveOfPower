using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace PenisPotato.Audio
{
    public class AudioManager : Microsoft.Xna.Framework.GameComponent
    {
        // List of all the sound effects that will be loaded into this manager.
        public static string[] soundNames =
        {
            
        };

        public static string[] songNames =
        {
            "Audio/heart",
            "Audio/hex",
        };

        // The listener describes the ear which is hearing 3D sounds.
        // This is usually set to match the camera.
        public AudioListener Listener
        {
            get { return listener; }
        }

        AudioListener listener = new AudioListener();


        // The emitter describes an entity which is making a 3D sound.
        AudioEmitter emitter = new AudioEmitter();


        // Store all the sound effects that are available to be played.
        Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
        Dictionary<string, Song> songs = new Dictionary<string, Song>();

        int currSongIndex;
        bool playPlaylist = false;

        // Keep track of all the 3D sounds that are currently playing.
        List<ActiveSound> activeSounds = new List<ActiveSound>();

        public VisualizationData visData = new VisualizationData();


        public AudioManager(Game game)
            : base(game)
        { }


        /// <summary>
        /// Initializes the audio manager.
        /// </summary>
        public override void Initialize()
        {
            // Set the scale for 3D audio so it matches the scale of our game world.
            // DistanceScale controls how much sounds change volume as you move further away.
            // DopplerScale controls how much sounds change pitch as you move past them.
            SoundEffect.DistanceScale = 2000;
            SoundEffect.DopplerScale = 0.1f;

            MediaPlayer.IsVisualizationEnabled = true;

            // Load all the sound effects.
            foreach (string soundName in soundNames)
            {
                soundEffects.Add(soundName, Game.Content.Load<SoundEffect>(soundName));
            }

            foreach (string songName in songNames)
                songs.Add(songName, Game.Content.Load<Song>(songName));

            base.Initialize();
        }


        /// <summary>
        /// Unloads the sound effect data.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    foreach (SoundEffect soundEffect in soundEffects.Values)
                    {
                        soundEffect.Dispose();
                    }

                    soundEffects.Clear();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        
        /// <summary>
        /// Updates the state of the 3D audio system.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Loop over all the currently playing 3D sounds.
            int index = 0;

            if (playPlaylist && MediaPlayer.State.Equals(MediaState.Stopped))
                NextSongInPlaylist();
            else if (MediaPlayer.State.Equals(MediaState.Playing))
                MediaPlayer.GetVisualizationData(visData);

            while (index < activeSounds.Count)
            {
                ActiveSound activeSound = activeSounds[index];

                if (activeSound.Instance.State == SoundState.Stopped)
                {
                    // If the sound has stopped playing, dispose it.
                    activeSound.Instance.Dispose();

                    // Remove it from the active list.
                    activeSounds.RemoveAt(index);

                    //Checks if the current sound effect is part of the playlit, if it is then it actually throws it off the top of
                    //the list and starts the next song in the playlist. REPLACE THIS WITH SONGS FILE TYPES
                    /*if (playlist.Count > 0 && activeSound.Equals(playlist[0]))
                    {
                        playlist.RemoveAt(0);
                        NextSongInPlaylist();
                    }*/
                }
                else
                {
                    // If the sound is still playing, update its 3D settings.
                    Apply3D(activeSound);

                    index++;
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Triggers a new 3D sound.
        /// </summary>
        public SoundEffectInstance Play3DSound(string soundName, bool isLooped, IAudioEmitter emitter)
        {
            ActiveSound activeSound = new ActiveSound();

            // Fill in the instance and emitter fields.
            activeSound.Instance = soundEffects[soundName].CreateInstance();
            activeSound.Instance.IsLooped = isLooped;

            activeSound.Emitter = emitter;

            // Set the 3D position of this sound, and then play it.
            Apply3D(activeSound);

            activeSound.Instance.Play();

            // Remember that this sound is now active.
            activeSounds.Add(activeSound);

            return activeSound.Instance;
        }

        public void AddToPlaylist(string soundName)
        {
            songs.Add(soundName, songs[soundName]);
            /*ActiveSound activeSound = new ActiveSound();

            // Fill in the instance and emitter fields.
            activeSound.Instance = soundEffects[soundName].CreateInstance();
            activeSound.Instance.IsLooped = false;

            activeSound.Emitter = emitter;

            // Set the 3D position of this sound, and then play it.
            Apply3D(activeSound);

            playlist.Add(activeSound);*/
        }

        private void NextSongInPlaylist()
        {
            MediaPlayer.Play(songs[songNames[currSongIndex]]);
            currSongIndex++;
            /*ActiveSound activeSound = playlist[0];

            activeSound.Instance.Play();

            // Remember that this sound is now active.
            activeSounds.Add(activeSound);*/

        }

        public void StartPlaylist(int index)
        {
            if (songs.Count >= index)
            {
                playPlaylist = true;
                currSongIndex = index;
                NextSongInPlaylist();
            }
        }


        /// <summary>
        /// Updates the position and velocity settings of a 3D sound.
        /// </summary>
        private void Apply3D(ActiveSound activeSound)
        {
            emitter.Position = activeSound.Emitter.Position;
            //emitter.Forward = activeSound.Emitter.Forward;
            //emitter.Up = activeSound.Emitter.Up;
            emitter.Velocity = activeSound.Emitter.Velocity;

            activeSound.Instance.Apply3D(listener, emitter);
        }


        /// <summary>
        /// Internal helper class for keeping track of an active 3D sound,
        /// and remembering which emitter object it is attached to.
        /// </summary>
        private class ActiveSound
        {
            public SoundEffectInstance Instance;
            public IAudioEmitter Emitter;
        }
    }


    /// <summary>
    /// Interface used by the AudioManager to look up the position
    /// and velocity of entities that can emit 3D sounds.
    /// </summary>
    public interface IAudioEmitter
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        Vector3 Up { get; }
        Vector3 Velocity { get; }
    }

    public class SpriteEntity : IAudioEmitter
    {
        /// <summary>
        /// Gets or sets the 3D position of the entity.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        Vector3 position;


        /// <summary>
        /// Gets or sets which way the entity is facing.
        /// </summary>
        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        Vector3 forward;


        /// <summary>
        /// Gets or sets the orientation of this entity.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        Vector3 up;

        
        /// <summary>
        /// Gets or sets how fast this entity is moving.
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            protected set { velocity = value; }
        }

        Vector3 velocity;

        public SpriteEntity(Vector3 pos, Vector3 forward, Vector3 up, Vector3 velocity)
        {
            Position = pos;
            Forward = forward;
            Up = up;
            Velocity = velocity;
        }
    }
}
