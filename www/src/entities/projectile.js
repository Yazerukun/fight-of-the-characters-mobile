/* ============================================================================
 * SWEPT PROJECTILE SYSTEM
 * Location: src/entities/projectile.js
 * ============================================================================ */
import { Vector2D } from '../math/vector2d.js';
import { TEAMS } from '../config/constants.js';

export class Projectile {
  constructor() {
    this.active = false;
    this.pos = new Vector2D();
    this.vel = new Vector2D();
    this.radius = 16;
    this.damage = 100;
    this.ownerTeam = TEAMS.SENTINEL;
    this.rangeLeft = 600;
    this.type = 'getsuga'; // 'getsuga', 'rasenshuriken', etc.
  }

  init(x, y, targetX, targetY, speed, damage, ownerTeam, type = 'getsuga') {
    this.active = true;
    this.pos.set(x, y);
    this.damage = damage;
    this.ownerTeam = ownerTeam;
    this.type = type;
    this.rangeLeft = (type === 'getsuga') ? 850 : 550;
    this.radius = (type === 'getsuga') ? 34 : 18;

    const dir = new Vector2D(targetX - x, targetY - y).normalize();
    this.vel.set(dir.x * speed, dir.y * speed);
  }

  update(dt) {
    if (!this.active) return;
    const step = this.vel.length() * dt;
    this.pos.x += this.vel.x * dt;
    this.pos.y += this.vel.y * dt;
    this.rangeLeft -= step;
    if (this.rangeLeft <= 0) this.active = false;
  }
}
