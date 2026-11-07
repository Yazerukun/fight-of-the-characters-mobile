/* ============================================================================
 * CAMERA SYSTEM & TRAUMA SCREEN SHAKE
 * Location: src/core/camera.js
 * ============================================================================ */
import { Vector2D } from '../math/vector2d.js';

export class Camera {
  constructor(initialX = 0, initialY = 0) {
    this.pos = new Vector2D(initialX, initialY);
    this.trauma = 0;
  }

  addTrauma(amt) {
    this.trauma = Math.min(1.0, this.trauma + amt);
  }

  update(targetX, targetY, viewportWidth, viewportHeight, dt) {
    // Smooth lerp to center on player
    const desiredX = targetX - (viewportWidth / 2);
    const desiredY = targetY - (viewportHeight / 2);
    this.pos.x += (desiredX - this.pos.x) * 0.12;
    this.pos.y += (desiredY - this.pos.y) * 0.12;

    // Decay trauma over time
    this.trauma = Math.max(0, this.trauma - dt * 1.2);
  }

  applyScreenShake(ctx) {
    if (this.trauma > 0.01) {
      const shakeFactor = this.trauma * this.trauma * 16;
      const offsetX = (Math.random() - 0.5) * shakeFactor;
      const offsetY = (Math.random() - 0.5) * shakeFactor;
      ctx.translate(offsetX, offsetY);
    }
  }
}
