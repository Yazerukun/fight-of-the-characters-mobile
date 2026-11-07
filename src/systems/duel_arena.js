/* ============================================================================
 * DUEL ARENA STATE MACHINE (3-MINUTE GLADIATOR RING)
 * Location: src/systems/duel_arena.js
 * ============================================================================ */
import { Vector2D } from '../math/vector2d.js';
import { CONFIG } from '../config/constants.js';

export class DuelArena {
  constructor(centerX = CONFIG.WORLD_WIDTH / 2, centerY = 350, radius = 380) {
    this.center = new Vector2D(centerX, centerY);
    this.radius = radius;
    this.timer = CONFIG.DUEL_INTERVAL_SEC;
    this.isActive = false;
    this.duration = 0;
  }

  update(dt, onStartDuel, onEndDuel) {
    if (!this.isActive) {
      this.timer -= dt;
      if (this.timer <= 0) {
        this.isActive = true;
        this.duration = CONFIG.DUEL_DURATION_SEC;
        if (onStartDuel) onStartDuel();
      }
    } else {
      this.duration -= dt;
      if (this.duration <= 0) {
        this.isActive = false;
        this.timer = CONFIG.DUEL_INTERVAL_SEC;
        if (onEndDuel) onEndDuel(false);
      }
    }
  }

  clampEntity(entity) {
    if (!this.isActive) return;
    const dist = entity.pos.dist(this.center);
    if (dist > this.radius - entity.radius) {
      const angle = Math.atan2(entity.pos.y - this.center.y, entity.pos.x - this.center.x);
      entity.pos.x = this.center.x + Math.cos(angle) * (this.radius - entity.radius);
      entity.pos.y = this.center.y + Math.sin(angle) * (this.radius - entity.radius);
    }
  }

  draw(ctx, camera) {
    const sx = this.center.x - camera.pos.x;
    const sy = this.center.y - camera.pos.y;
    ctx.save();
    ctx.strokeStyle = this.isActive ? '#ef4444' : '#475569';
    ctx.lineWidth = 3;
    ctx.setLineDash([12, 8]);
    ctx.beginPath();
    ctx.arc(sx, sy, this.radius, 0, Math.PI * 2);
    ctx.stroke();

    ctx.font = 'bold 16px -apple-system, sans-serif';
    ctx.fillStyle = this.isActive ? '#f87171' : '#64748b';
    ctx.textAlign = 'center';
    ctx.fillText("⚔️ FOTC DUEL ARENA ⚔️", sx, sy - this.radius + 30);
    ctx.restore();
  }
}
