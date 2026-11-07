/* ============================================================================
 * BASE SIMULATION ENTITY
 * Location: src/entities/entity.js
 * ============================================================================ */
import { Vector2D } from '../math/vector2d.js';

export class Entity {
  constructor(x, y, radius, team) {
    this.id = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).slice(2);
    this.pos = new Vector2D(x, y);
    this.vel = new Vector2D(0, 0);
    this.radius = radius;
    this.team = team;
    this.maxHp = 600;
    this.hp = 600;
    this.maxMp = 200;
    this.mp = 200;
    this.atk = 45;
    this.armor = 3;
    this.speed = 220;
    this.dead = false;
    this.facingAngle = 0;
    this.hitStop = 0;
    this.attackTimer = 0;
    this.attackInterval = 1.0;
  }

  takeDamage(amount, isMagic = false) {
    if (this.dead) return 0;
    const reduction = isMagic ? 0.8 : (1 - (this.armor * 0.05) / (1 + this.armor * 0.05));
    const actualDmg = Math.max(1, Math.round(amount * reduction));
    this.hp -= actualDmg;
    if (this.hp <= 0) {
      this.hp = 0;
      this.dead = true;
    }
    return actualDmg;
  }
}
