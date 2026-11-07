/* ============================================================================
 * FOTC HERO BASE CLASS (STR / AGI / INT SPEC)
 * Location: src/entities/hero.js
 * ============================================================================ */
import { Entity } from './entity.js';

export class Hero extends Entity {
  constructor(x, y, team, name = 'Kurosaki') {
    super(x, y, 22, team);
    this.name = name;
    this.level = 1;
    this.exp = 0;
    this.expToNext = 120;
    this.str = 22; // +24 HP per STR
    this.agi = 18; // +1.2 ASPD / armor / speed
    this.int = 16; // +14 MP per INT
    this.calcStats();
    
    // Default cooldowns
    this.cd = { q: 0, w: 0, e: 0, r: 0 };
    this.maxCd = { q: 3.5, w: 6.0, e: 14.0, r: 24.0 };
    
    // Buff / Form state
    this.buffActive = false;
    this.buffDuration = 0;
    this.buffName = 'None';

    this.attackTimer = 0;
    this.attackInterval = 0.85;
  }

  calcStats() {
    this.maxHp = 450 + this.str * 24;
    this.hp = Math.min(this.hp, this.maxHp);
    this.maxMp = 150 + this.int * 14;
    this.mp = Math.min(this.mp, this.maxMp);
    this.atk = 45 + Math.floor(this.str * 1.5);
    this.speed = 230 + Math.floor(this.agi * 1.2);
  }

  gainExp(amt) {
    this.exp += amt;
    if (this.exp >= this.expToNext && this.level < 25) {
      this.level++;
      this.exp -= this.expToNext;
      this.expToNext = Math.floor(this.expToNext * 1.4);
      this.str += 3;
      this.agi += 2;
      this.int += 2;
      this.calcStats();
      this.hp = this.maxHp;
      this.mp = this.maxMp;
      return true; // Leveled up
    }
    return false;
  }
}
