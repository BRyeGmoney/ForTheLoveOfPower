using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Graphics.Animation
{
        /// <summary>
        /// Controls playback of an Animation.
        /// </summary>
        public struct AnimationPlayer
        {
            /// <summary>
            /// Gets the animation which is currently playing.
            /// </summary>
            public Animation Animation
            {
                get { return animation; }
                set { animation = value; }
            }
            Animation animation;

            /// <summary>
            /// Gets the index of the current frame in the animation.
            /// </summary>
            public int FrameIndex
            {
                get { return frameIndex; }
            }
            int frameIndex;

            /// <summary>
            /// The amount of time in seconds that the current frame has been shown for.
            /// </summary>
            private float time;

            /// <summary>
            /// Gets a texture origin at the bottom center of each frame.
            /// </summary>
            public Vector2 Origin
            {
                get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
            }

            /// <summary>
            /// Begins or continues playback of an animation.
            /// </summary>
            public void PlayAnimation(Animation animation)
            {
                // If this animation is already running, do not restart it.
                if (Animation == animation)
                    return;

                // Start the new animation.
                this.animation = animation;
                this.frameIndex = 0;
                this.time = 0.0f;
            }

            /// <summary>
            /// Sets the current animation to null so that we can return to a standard spritebatch.Draw call.
            /// </summary>
            public void KillAnimation()
            {
                Animation = null;

                this.animation = null;
                this.frameIndex = 0;
                this.time = 0.0f;
            }

            /// <summary>
            /// Advances the time position and draws the current frame of the animation.
            /// </summary>
            public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color playerColor)
            {
                if (Animation == null)
                    throw new NotSupportedException("No animation is currently playing.");

                // Process passing time.
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                while (time > Animation.FrameTime)
                {
                    time -= Animation.FrameTime;

                    // Advance the frame index; looping or clamping as appropriate.
                    if (Animation.IsLooping)
                    {
                        frameIndex = (frameIndex + 1) % Animation.FrameCount;
                    }
                    else
                    {
                        frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                    }
                }

                // Calculate the source rectangle of the current frame.
                Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

                // Draw the current frame.
                spriteBatch.Draw(Animation.Texture, position, source, playerColor, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
            }

            public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle pieceRect, SpriteEffects spriteEffects, Color playerColor)
            {
                if (Animation == null)
                    throw new NotSupportedException("No animation is currently playing.");

                // Process passing time.
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                while (time > Animation.FrameTime)
                {
                    time -= Animation.FrameTime;

                    // Advance the frame index; looping or clamping as appropriate.
                    if (Animation.IsLooping)
                    {
                        frameIndex = (frameIndex + 1) % Animation.FrameCount;
                    }
                    else
                    {
                        frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                    }
                }

                // Calculate the source rectangle of the current frame.
                Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

                // Draw the current frame.
                spriteBatch.Draw(Animation.Texture, pieceRect, source, playerColor, 0.0f, Vector2.Zero, spriteEffects, 0.0f);
            }
        }
}
