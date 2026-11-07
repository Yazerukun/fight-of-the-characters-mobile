/* ============================================================================
 * OBJECT POOLS (ZERO-GC PARTICLES & DAMAGE COMBAT TEXT)
 * Location: src/systems/pools.js
 * ============================================================================ */
import { Vector2D } from '../math/vector2d.js';

export class ParticlePool {
  constructor(maxSize = 300) {
    this.pool = [];
    for (let i = 0; i < maxSize; i++) {
      this.pool.push({
        active: false,
        pos: new Vector2D(),
        vel: new Vector2D(),
        color: '#fff',
        size: 4,
        life: 0,
        maxLife: 1
      });
    }
  }

  spawn(x, y, vx, vy, color, size, life) {
    const p = this.pool.find(item => !item.active);
    if (!p) return;
    p.active = true;
    p.pos.set(x, y);
    p.vel.set(vx, vy);
    p.color = color;
    p.size = size;
    p.life = life;
    p.maxLife = life;
  }

  update(dt) {
    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active) continue;
      p.pos.x += p.vel.x * dt;
      p.pos.y += p.vel.y * dt;
      p.vel.x *= 0.95;
      p.vel.y *= 0.95;
      p.life -= dt;
      if (p.life <= 0) p.active = false;
    }
  }

  draw(ctx, camera) {
    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active) continue;
      const sx = p.pos.x - camera.x;
      const sy = p.pos.y - camera.y;
      const alpha = Math.max(0, p.life / p.maxLife);
      ctx.save();
      ctx.globalAlpha = alpha;
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.arc(sx, sy, p.size, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    }
  }
}

export class FloatingTextPool {
  constructor(maxSize = 100) {
    this.pool = [];
    for (let i = 0; i < maxSize; i++) {
      this.pool.push({
        active: false,
        text: '',
        x: 0,
        y: 0,
        vy: -40,
        color: '#fff',
        life: 0,
        maxLife: 0.8
      });
    }
  }

  spawn(x, y, text, color = '#fff') {
    const item = this.pool.find(t => !t.active);
    if (!item) return;
    item.active = true;
    item.x = x;
    item.y = y;
    item.text = text;
    item.color = color;
    item.life = 0.8;
    item.maxLife = 0.8;
    item.vy = -50 - Math.random() * 20;
  }

  update(dt) {
    for (let i = 0; i < this.pool.length; i++) {
      const t = this.pool[i];
      if (!t.active) continue;
      t.y += t.vy * dt;
      t.life -= dt;
      if (t.life <= 0) t.active = false;
    }
  }

  draw(ctx, camera) {
    for (let i = 0; i < this.pool.length; i++) {
      const t = this.pool[i];
      if (!t.active) continue;
      const alpha = Math.max(0, t.life / t.maxLife);
      ctx.save();
      ctx.globalAlpha = alpha;
      ctx.fillStyle = t.color;
      ctx.font = 'bold 15px -apple-system, sans-serif';
      ctx.textAlign = 'center';
      ctx.shadowColor = '#000';
      ctx.shadowBlur = 4;
      ctx.fillText(t.text, t.x - camera.x, t.y - camera.y);
      ctx.restore();
    }
  }
}
