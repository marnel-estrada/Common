using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Common {
    public class ImageAnimation : MonoBehaviour {

        [SerializeField]
        private Image image;

        [SerializeField]
        private float framesPerSecond = 15;

        [SerializeField]
        private Sprite[] sprites; // The sequence of sprites comprising the animation

        private float polledTime;

        private int currentFrameIndex;

        private void Awake() {
            Assertion.NotNull(this.image);
            Assertion.Assert(this.framesPerSecond > 0);

            this.polledTime = 0;

            // Reset
            this.currentFrameIndex = 0;
            SetSprite(this.currentFrameIndex);
        }

        private void Update() {
            this.polledTime += UnityEngine.Time.deltaTime;

            // We didn't cache this so we can see the effect of framesPerSecond on the fly like tweaking it in editor
            float timePerFrame = 1.0f / this.framesPerSecond;

            while(this.polledTime > timePerFrame) {
                this.polledTime -= timePerFrame;

                // Show next frame
                this.currentFrameIndex = (this.currentFrameIndex + 1) % this.sprites.Length;
                SetSprite(this.currentFrameIndex);
            }
        }

        private void SetSprite(int index) {
            // Changed to overrideSprite since Unity2019 has this weird bug #1406
            this.image.overrideSprite = this.sprites[index];
        }

    }
}
