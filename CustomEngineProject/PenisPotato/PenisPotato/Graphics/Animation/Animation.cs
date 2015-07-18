using System;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Graphics.Animation
{
        /// <summary>
        /// Represents an animated texture.
        /// </summary>
        /// <remarks>
        /// Currently, this class assumes that each frame of animation is
        /// as wide as each animation is tall. The number of frames in the
        /// animation are inferred from this.
        /// </remarks>
        public class Animation
        {
            /// <summary>
            /// All frames in the animation arranged horizontally.
            /// </summary>
            public Texture2D Texture
            {
                get { return texture; }
            }
            Texture2D texture;

            /// <summary>
            /// Duration of time to show each frame.
            /// </summary>
            public float FrameTime
            {
                get { return frameTime; }
            }
            float frameTime;

            /// <summary>
            /// When the end of the animation is reached, should it
            /// continue playing from the beginning?
            /// </summary>
            public bool IsLooping
            {
                get { return isLooping; }
            }
            bool isLooping;

            public int FrameToLoopFrom
            {
                get { return frameToLoopFrom; }
            }
            int frameToLoopFrom = 0;

            /// <summary>
            /// Gets the number of frames in the animation.
            /// </summary>
            public int FrameCount
            {
                get { return Texture.Width / FrameWidth; }
            }

            /// <summary>
            /// Gets the width of a frame in the animation.
            /// </summary>
            public int FrameWidth
            {
                // Assume square frames.
                get { return Texture.Height; }
            }

            /// <summary>
            /// Gets the height of a frame in the animation.
            /// </summary>
            public int FrameHeight
            {
                get { return Texture.Height; }
            }

            public bool IsStopped { get; set; }
            public bool IsDeathAnimation { get; set; }

            /// <summary>
            /// Constructs a new animation
            /// </summary>
            /// <param name="texture">The entire spritesheet for parsing</param>
            /// <param name="frameTime">the duration each frame spends on screen</param>
            /// <param name="isLooping">Whether the animation actually loops</param>
            /// <param name="frameToLoopFrom">The frame from which you wish to start the animation the second time around.</param>
            public Animation(Texture2D texture, float frameTime, bool isLooping, int frameToLoopFrom)
            {
                this.texture = texture;
                this.frameTime = frameTime;
                this.isLooping = isLooping;
                this.frameToLoopFrom = frameToLoopFrom;
            }

            /// <summary>
            /// Constructs a new animation
            /// </summary>
            /// <param name="texture">The entire spritesheet for parsing</param>
            /// <param name="frameTime">the duration each frame spends on screen</param>
            /// <param name="isLooping">Whether the animation actually loops</param>
            public Animation(Texture2D texture, float frameTime, bool isLooping)
            {
                this.texture = texture;
                this.frameTime = frameTime;
                this.isLooping = isLooping;
            }
        }
}
