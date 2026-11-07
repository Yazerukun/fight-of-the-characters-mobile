/* ============================================================================
 * MODULAR CORE GAME ENGINE
 * Location: src/core/engine.js
 * ============================================================================ */
import { CONFIG, TEAMS } from '../config/constants.js';
import { Vector2D } from '../math/vector2d.js';
import { SpatialGrid } from './spatial_grid.js';
import { Camera } from './camera.js';
import { ProceduralAudioEngine } from '../audio/sound_synth.js';
import { ParticlePool, FloatingTextPool } from '../systems/pools.js';
import { Entity } from '../entities/entity.js';
import { Hero } from '../entities/hero.js';
import { Projectile } from '../entities/projectile.js';
import { DuelArena } from '../systems/duel_arena.js';
import { AntiCheatNumber } from '../security/anti_cheat.js';
import { HERO_ROSTER } from '../heroes/roster.js';
import { CharacterRenderer } from '../render/character_renderer.js';

export class GameEngine {
  constructor() {
    this.canvas = document.getElementById('gameCanvas');
    this.ctx = this.canvas.getContext('2d', { alpha: false });
    this.camera = new Camera(CONFIG.WORLD_WIDTH / 2, CONFIG.WORLD_HEIGHT / 2);
    
    this.grid = new SpatialGrid(CONFIG.WORLD_WIDTH, CONFIG.WORLD_HEIGHT, CONFIG.GRID_CELL_SIZE);
    this.audio = new ProceduralAudioEngine();
    this.particles = new ParticlePool(300);
    this.floatingTexts = new FloatingTextPool(100);
    this.charRenderer = new CharacterRenderer();

    // Initial default hero
    this.selectedHeroDef = HERO_ROSTER.ichigo;
    this.player = new Hero(CONFIG.WORLD_WIDTH / 2 - 100, CONFIG.WORLD_HEIGHT / 2, TEAMS.SENTINEL, 'Kurosaki');
    this.enemyHero = new Hero(CONFIG.WORLD_WIDTH / 2 + 300, CONFIG.WORLD_HEIGHT / 2, TEAMS.SCOURGE, 'Byakuya');
    this.enemyHero.speed = 210;
    
    this.creeps = [];
    this.projectiles = new Array(60).fill(null).map(() => new Projectile());
    this.duelArena = new DuelArena();

    this.gold = new AntiCheatNumber(250);
    this.keys = {};
    this.touchMoveDir = new Vector2D();
    this.isMoving = false;
    this.creepTimer = 0;

    this.initCanvasResize();
    this.spawnCreepCamp();
    this.selectHero('ichigo');
  }

  selectHero(heroId) {
    const def = HERO_ROSTER[heroId];
    if (!def) return;
    this.selectedHeroDef = def;
    this.player.name = def.name;
    this.player.str = def.stats.str;
    this.player.agi = def.stats.agi;
    this.player.int = def.stats.int;
    this.player.calcStats();
    this.player.hp = this.player.maxHp;
    this.player.mp = this.player.maxMp;

    // Update Skill UI Labels
    const btnQ = document.querySelector('#btn-skill-q .skill-name');
    const btnW = document.querySelector('#btn-skill-w .skill-name');
    const btnE = document.querySelector('#btn-skill-e .skill-name');
    const btnR = document.querySelector('#btn-skill-r .skill-name');
    if (btnQ) btnQ.textContent = def.skills.q.name.split(' ')[0];
    if (btnW) btnW.textContent = def.skills.w.name.split(' ')[0];
    if (btnE) btnE.textContent = def.skills.e.name.split(' ')[0];
    if (btnR) btnR.textContent = def.skills.r.name.split(' ')[0];

    this.showToast(`HERO SELECTED: ${def.name} (${def.origin})!`);
    this.audio.playBlink();
    
    const modal = document.getElementById('hero-select-modal');
    if (modal) modal.style.display = 'none';
  }

  initCanvasResize() {
    const resize = () => {
      const dpr = window.devicePixelRatio || 1;
      this.canvas.width = window.innerWidth * dpr;
      this.canvas.height = window.innerHeight * dpr;
      this.ctx.scale(dpr, dpr);
      this.viewportWidth = window.innerWidth;
      this.viewportHeight = window.innerHeight;
    };
    window.addEventListener('resize', resize);
    resize();
  }

  showToast(text) {
    const c = document.getElementById('toast-container');
    if (!c) return;
    const msg = document.createElement('div');
    msg.className = 'toast-msg';
    msg.textContent = text;
    c.appendChild(msg);
    setTimeout(() => msg.remove(), 2500);
  }

  spawnCreepCamp() {
    for (let i = 0; i < 4; i++) {
      const c = new Entity(
        CONFIG.WORLD_WIDTH / 2 - 250 + (Math.random() * 80 - 40),
        CONFIG.WORLD_HEIGHT / 2 + (i * 40 - 60),
        16,
        TEAMS.SENTINEL
      );
      c.maxHp = 320;
      c.hp = 320;
      c.atk = 22;
      c.speed = 150;
      this.creeps.push(c);
    }
    for (let i = 0; i < 4; i++) {
      const c = new Entity(
        CONFIG.WORLD_WIDTH / 2 + 250 + (Math.random() * 80 - 40),
        CONFIG.WORLD_HEIGHT / 2 + (i * 40 - 60),
        16,
        TEAMS.SCOURGE
      );
      c.maxHp = 320;
      c.hp = 320;
      c.atk = 22;
      c.speed = 150;
      this.creeps.push(c);
    }
  }

  castSkillQ() {
    const p = this.player;
    if (p.cd.q > 0 || p.mp < 35 || p.dead) return;
    p.mp -= 35;
    p.cd.q = p.maxCd.q;
    this.audio.playSlash();
    this.camera.addTrauma(0.25);

    const forward = new Vector2D(Math.cos(p.facingAngle), Math.sin(p.facingAngle));
    const coneCenter = new Vector2D(p.pos.x + forward.x * 60, p.pos.y + forward.y * 60);

    for (let i = 0; i < 18; i++) {
      const ang = p.facingAngle + (Math.random() - 0.5) * 1.6;
      const spd = 200 + Math.random() * 200;
      this.particles.spawn(
        p.pos.x, p.pos.y,
        Math.cos(ang) * spd, Math.sin(ang) * spd,
        p.buffActive ? '#ef4444' : '#38bdf8',
        4, 0.25
      );
    }

    const hits = this.grid.queryRange(coneCenter, 85, e => e.team !== p.team);
    hits.forEach(target => {
      const dmg = target.takeDamage(p.atk * 1.8 + (p.buffActive ? 80 : 0));
      this.floatingTexts.spawn(target.pos.x, target.pos.y - 20, `-${dmg}`, '#ef4444');
      target.pos.x += forward.x * 35;
      target.pos.y += forward.y * 35;
      if (target.dead) this.onEntityKilled(target, p);
    });
  }

  castSkillW() {
    const p = this.player;
    if (p.cd.w > 0 || p.mp < 50 || p.dead) return;
    p.mp -= 50;
    p.cd.w = p.maxCd.w;
    this.audio.playBlink();

    const blinkDist = 220;
    const targetX = p.pos.x + Math.cos(p.facingAngle) * blinkDist;
    const targetY = p.pos.y + Math.sin(p.facingAngle) * blinkDist;

    for (let i = 0; i < 12; i++) {
      this.particles.spawn(p.pos.x, p.pos.y, (Math.random() - 0.5) * 80, (Math.random() - 0.5) * 80, '#9333ea', 5, 0.4);
    }

    p.pos.x = Math.max(60, Math.min(CONFIG.WORLD_WIDTH - 60, targetX));
    p.pos.y = Math.max(60, Math.min(CONFIG.WORLD_HEIGHT - 60, targetY));

    for (let i = 0; i < 12; i++) {
      this.particles.spawn(p.pos.x, p.pos.y, (Math.random() - 0.5) * 80, (Math.random() - 0.5) * 80, '#a855f7', 5, 0.4);
    }
  }

  castSkillE() {
    const p = this.player;
    if (p.cd.e > 0 || p.mp < 80 || p.dead) return;
    p.mp -= 80;
    p.cd.e = p.maxCd.e;
    p.buffActive = true;
    p.buffDuration = 10.0;
    p.buffName = 'Bankai';
    this.audio.playExplosion();
    this.camera.addTrauma(0.45);
    this.showToast("TRANSFORMATION ACTIVATED! (+50% SPEED & DAMAGE)");

    for (let i = 0; i < 35; i++) {
      const ang = Math.random() * Math.PI * 2;
      const spd = 150 + Math.random() * 250;
      this.particles.spawn(p.pos.x, p.pos.y, Math.cos(ang) * spd, Math.sin(ang) * spd, '#dc2626', 6, 0.5);
    }
  }

  castSkillR() {
    const p = this.player;
    if (p.cd.r > 0 || p.mp < 120 || p.dead) return;
    p.mp -= 120;
    p.cd.r = p.maxCd.r;
    this.audio.playGetsugaBeam();
    this.camera.addTrauma(0.65);

    const proj = this.projectiles.find(item => !item.active);
    if (proj) {
      const targetX = p.pos.x + Math.cos(p.facingAngle) * 500;
      const targetY = p.pos.y + Math.sin(p.facingAngle) * 500;
      const dmg = (p.atk * 3.5) + (p.buffActive ? 250 : 100);
      proj.init(p.pos.x, p.pos.y, targetX, targetY, 680, dmg, p.team, 'getsuga');
    }
  }

  buyItem(itemId) {
    const curGold = this.gold.get();
    if (itemId === 'potion' && curGold >= 50) {
      this.gold.add(-50);
      this.player.hp = Math.min(this.player.maxHp, this.player.hp + 300);
      this.floatingTexts.spawn(this.player.pos.x, this.player.pos.y - 30, '+300 HP', '#22c55e');
      this.audio.playBlink();
      this.showToast("Used Healing Potion (+300 HP)");
    } else if (itemId === 'claws' && curGold >= 250) {
      this.gold.add(-250);
      this.player.str += 8;
      this.player.calcStats();
      this.showToast("Equipped Claws of Attack (+30 ATK)");
    } else if (itemId === 'boots' && curGold >= 300) {
      this.gold.add(-300);
      this.player.agi += 15;
      this.player.calcStats();
      this.showToast("Equipped Boots of Speed (+45 MS)");
    } else if (itemId === 'relic' && curGold >= 800) {
      this.gold.add(-800);
      this.player.str += 25;
      this.player.calcStats();
      this.showToast("Equipped Sacred Relic (+80 ATK)");
    } else {
      this.showToast("Not enough Gold!");
    }
    const modal = document.getElementById('shop-modal');
    if (modal) modal.style.display = 'none';
  }

  onEntityKilled(victim, killer) {
    if (victim.team !== TEAMS.SENTINEL) {
      const reward = (victim instanceof Hero) ? 350 : 45;
      const expReward = (victim instanceof Hero) ? 240 : 35;
      this.gold.add(reward);
      const lvUp = this.player.gainExp(expReward);
      this.floatingTexts.spawn(victim.pos.x, victim.pos.y - 40, `+${reward} G`, '#facc15');
      if (lvUp) {
        this.showToast(`LEVEL UP! Hero is now Level ${this.player.level}!`);
        this.audio.playBlink();
      }

      if (this.duelArena.isActive && victim === this.enemyHero) {
        this.duelArena.isActive = false;
        this.gold.add(500);
        this.player.gainExp(300);
        this.showToast("VICTORY! You won the Duel! +500 Gold & Bonus EXP!");
      }
    } else if (victim === this.player && this.duelArena.isActive) {
      this.duelArena.isActive = false;
      this.showToast("DEFEAT! Knocked down in the Duel.");
    }
  }

  fixedUpdate(dt) {
    const p = this.player;
    if (!p.dead) {
      p.hp = Math.min(p.maxHp, p.hp + (p.str * 0.08) * dt);
      p.mp = Math.min(p.maxMp, p.mp + (p.int * 0.12) * dt);
    }

    if (p.buffActive) {
      p.buffDuration -= dt;
      if (p.buffDuration <= 0) {
        p.buffActive = false;
        this.showToast("Transformation ended.");
      }
    }

    for (const k in p.cd) {
      if (p.cd[k] > 0) p.cd[k] = Math.max(0, p.cd[k] - dt);
    }

    // Movement
    let moveX = 0;
    let moveY = 0;
    if (this.keys['KeyW'] || this.keys['ArrowUp']) moveY -= 1;
    if (this.keys['KeyS'] || this.keys['ArrowDown']) moveY += 1;
    if (this.keys['KeyA'] || this.keys['ArrowLeft']) moveX -= 1;
    if (this.keys['KeyD'] || this.keys['ArrowRight']) moveX += 1;

    if (this.isMoving) {
      moveX = this.touchMoveDir.x;
      moveY = this.touchMoveDir.y;
    }

    if ((moveX !== 0 || moveY !== 0) && !p.dead) {
      p.isMoving = true;
      const moveVec = new Vector2D(moveX, moveY).normalize();
      const speedMultiplier = p.buffActive ? 1.4 : 1.0;
      p.pos.x += moveVec.x * p.speed * speedMultiplier * dt;
      p.pos.y += moveVec.y * p.speed * speedMultiplier * dt;
      p.facingAngle = Math.atan2(moveVec.y, moveVec.x);
    } else {
      p.isMoving = false;
    }

    // Duel Arena clamping or map bounds
    if (this.duelArena.isActive) {
      this.duelArena.clampEntity(p);
      this.duelArena.clampEntity(this.enemyHero);
    } else {
      p.pos.x = Math.max(p.radius, Math.min(CONFIG.WORLD_WIDTH - p.radius, p.pos.x));
      p.pos.y = Math.max(p.radius, Math.min(CONFIG.WORLD_HEIGHT - p.radius, p.pos.y));
    }

    // Rebuild Spatial Grid
    this.grid.clear();
    this.grid.insert(p);
    if (!this.enemyHero.dead) this.grid.insert(this.enemyHero);
    for (let i = 0; i < this.creeps.length; i++) {
      if (!this.creeps[i].dead) this.grid.insert(this.creeps[i]);
    }

    // Enemy AI
    if (!this.enemyHero.dead) {
      const e = this.enemyHero;
      const dist = e.pos.dist(p.pos);
      if (dist < 450 && !p.dead) {
        const dir = new Vector2D(p.pos.x - e.pos.x, p.pos.y - e.pos.y).normalize();
        e.pos.x += dir.x * e.speed * dt;
        e.pos.y += dir.y * e.speed * dt;
        e.facingAngle = Math.atan2(dir.y, dir.x);

        e.attackTimer += dt;
        if (e.attackTimer >= e.attackInterval && dist <= 50) {
          e.attackTimer = 0;
          const dmg = p.takeDamage(e.atk);
          this.floatingTexts.spawn(p.pos.x, p.pos.y - 20, `-${dmg}`, '#f87171');
          this.camera.addTrauma(0.2);
          this.audio.playSlash();
          if (p.dead) this.onEntityKilled(p, e);
        }
      }
    }

    // Creep AI
    for (let i = 0; i < this.creeps.length; i++) {
      const creep = this.creeps[i];
      if (creep.dead) continue;
      const foes = this.grid.queryRange(creep.pos, 220, other => other.team !== creep.team);
      if (foes.length > 0) {
        const target = foes[0];
        const dist = creep.pos.dist(target.pos);
        if (dist > 38) {
          const dir = new Vector2D(target.pos.x - creep.pos.x, target.pos.y - creep.pos.y).normalize();
          creep.pos.x += dir.x * creep.speed * dt;
          creep.pos.y += dir.y * creep.speed * dt;
        } else {
          creep.attackTimer += dt;
          if (creep.attackTimer >= 1.0) {
            creep.attackTimer = 0;
            const dmg = target.takeDamage(creep.atk);
            this.floatingTexts.spawn(target.pos.x, target.pos.y - 15, `-${dmg}`, '#e2e8f0');
            if (target.dead) this.onEntityKilled(target, creep);
          }
        }
      }
    }

    // Projectiles
    for (let i = 0; i < this.projectiles.length; i++) {
      const proj = this.projectiles[i];
      if (!proj.active) continue;
      proj.update(dt);

      this.particles.spawn(
        proj.pos.x, proj.pos.y,
        (Math.random() - 0.5) * 50, (Math.random() - 0.5) * 50,
        '#ef4444', 5, 0.2
      );

      const hits = this.grid.queryRange(proj.pos, proj.radius, ent => ent.team !== proj.ownerTeam);
      if (hits.length > 0) {
        hits.forEach(ent => {
          const dmg = ent.takeDamage(proj.damage, true);
          this.floatingTexts.spawn(ent.pos.x, ent.pos.y - 30, `CRIT -${dmg}!`, '#facc15');
          if (ent.dead) this.onEntityKilled(ent, p);
        });
        proj.active = false;
        this.audio.playExplosion();
        this.camera.addTrauma(0.35);
      }
    }

    // Duel State Machine Update
    this.duelArena.update(dt, () => {
      this.showToast("⚔️ DUEL ARENA TELEPORT! 1v1 BATTLE TO THE DEATH!");
      this.audio.playExplosion();
      p.pos.set(this.duelArena.center.x - 160, this.duelArena.center.y);
      p.hp = Math.max(p.hp, p.maxHp * 0.75);
      this.enemyHero.pos.set(this.duelArena.center.x + 160, this.duelArena.center.y);
      this.enemyHero.hp = Math.max(this.enemyHero.hp, this.enemyHero.maxHp * 0.75);
      this.enemyHero.dead = false;
      this.camera.addTrauma(0.6);
    }, (playerWon) => {
      this.showToast("Duel ended. Returning to battleground.");
      p.pos.set(CONFIG.WORLD_WIDTH / 2 - 120, CONFIG.WORLD_HEIGHT / 2);
      this.enemyHero.pos.set(CONFIG.WORLD_WIDTH / 2 + 120, CONFIG.WORLD_HEIGHT / 2);
    });

    // Respawn Creeps
    this.creepTimer += dt;
    if (this.creepTimer >= 20.0) {
      this.creepTimer = 0;
      this.creeps = this.creeps.filter(c => !c.dead);
      if (this.creeps.length < 12) this.spawnCreepCamp();
    }

    this.particles.update(dt);
    this.floatingTexts.update(dt);
    this.charRenderer.update(dt);
    this.camera.update(p.pos.x, p.pos.y, this.viewportWidth, this.viewportHeight, dt);
  }

  render() {
    const ctx = this.ctx;
    ctx.fillStyle = '#080c14';
    ctx.fillRect(0, 0, this.viewportWidth, this.viewportHeight);

    ctx.save();
    this.camera.applyScreenShake(ctx);

    // 1. Grid
    this.drawWorldGrid(ctx);

    // 2. Arena
    this.duelArena.draw(ctx, this.camera);

    // 3. Creeps
    for (let i = 0; i < this.creeps.length; i++) {
      const c = this.creeps[i];
      if (!c.dead) this.drawEntity(ctx, c, c.team === TEAMS.SENTINEL ? '#3b82f6' : '#ef4444');
    }

    // 4. Enemy Hero (Full-Body Animated Rig)
    if (!this.enemyHero.dead) {
      const sx = this.enemyHero.pos.x - this.camera.pos.x;
      const sy = this.enemyHero.pos.y - this.camera.pos.y;
      const enemyDef = HERO_ROSTER.byakuya || { id: 'byakuya', name: 'Byakuya', color: '#ec4899' };
      this.charRenderer.drawHero(ctx, this.enemyHero, sx, sy, enemyDef);
    }

    // 5. Player Hero (Full-Body Animated Rig with Costumes, Hair & Weapons)
    if (!this.player.dead) {
      const sx = this.player.pos.x - this.camera.pos.x;
      const sy = this.player.pos.y - this.camera.pos.y;
      this.charRenderer.drawHero(ctx, this.player, sx, sy, this.selectedHeroDef);
    }

    // 6. Projectiles
    for (let i = 0; i < this.projectiles.length; i++) {
      const proj = this.projectiles[i];
      if (!proj.active) continue;
      ctx.save();
      ctx.fillStyle = '#ef4444';
      ctx.shadowColor = '#f87171';
      ctx.shadowBlur = 15;
      ctx.beginPath();
      ctx.arc(proj.pos.x - this.camera.pos.x, proj.pos.y - this.camera.pos.y, proj.radius, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    }

    // 7. Particles & Combat Text
    this.particles.draw(ctx, this.camera.pos);
    this.floatingTexts.draw(ctx, this.camera.pos);

    ctx.restore();
    this.updateHUD();
  }

  drawWorldGrid(ctx) {
    const step = 80;
    const startX = Math.floor(this.camera.pos.x / step) * step;
    const startY = Math.floor(this.camera.pos.y / step) * step;
    const endX = startX + this.viewportWidth + step;
    const endY = startY + this.viewportHeight + step;

    ctx.strokeStyle = '#1e293b';
    ctx.lineWidth = 1;
    ctx.beginPath();
    for (let x = startX; x <= endX; x += step) {
      ctx.moveTo(x - this.camera.pos.x, 0);
      ctx.lineTo(x - this.camera.pos.x, this.viewportHeight);
    }
    for (let y = startY; y <= endY; y += step) {
      ctx.moveTo(0, y - this.camera.pos.y);
      ctx.lineTo(this.viewportWidth, y - this.camera.pos.y);
    }
    ctx.stroke();

    ctx.strokeStyle = '#38bdf8';
    ctx.lineWidth = 4;
    ctx.strokeRect(-this.camera.pos.x, -this.camera.pos.y, CONFIG.WORLD_WIDTH, CONFIG.WORLD_HEIGHT);
  }

  drawEntity(ctx, entity, color, label = '', isPlayer = false) {
    const sx = entity.pos.x - this.camera.pos.x;
    const sy = entity.pos.y - this.camera.pos.y;

    if (sx < -60 || sx > this.viewportWidth + 60 || sy < -60 || sy > this.viewportHeight + 60) return;

    ctx.save();

    if (isPlayer && entity.buffActive) {
      ctx.fillStyle = 'rgba(220, 38, 38, 0.25)';
      ctx.beginPath();
      ctx.arc(sx, sy, entity.radius * 2.2, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.arc(sx, sy, entity.radius, 0, Math.PI * 2);
    ctx.fill();
    ctx.lineWidth = 2;
    ctx.strokeStyle = '#ffffff';
    ctx.stroke();

    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.moveTo(sx, sy);
    ctx.lineTo(sx + Math.cos(entity.facingAngle) * (entity.radius + 12), sy + Math.sin(entity.facingAngle) * (entity.radius + 12));
    ctx.stroke();

    const barW = 46;
    const barH = 5;
    const hpPercent = Math.max(0, entity.hp / entity.maxHp);
    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(sx - barW / 2, sy - entity.radius - 14, barW, barH);
    ctx.fillStyle = (entity.team === TEAMS.SENTINEL) ? '#22c55e' : '#ef4444';
    ctx.fillRect(sx - barW / 2, sy - entity.radius - 14, barW * hpPercent, barH);

    if (entity instanceof Hero) {
      const mpPercent = Math.max(0, entity.mp / entity.maxMp);
      ctx.fillStyle = '#0284c7';
      ctx.fillRect(sx - barW / 2, sy - entity.radius - 8, barW * mpPercent, 3);
    }

    if (label) {
      ctx.font = 'bold 11px -apple-system, sans-serif';
      ctx.fillStyle = '#f8fafc';
      ctx.textAlign = 'center';
      ctx.fillText(label, sx, sy - entity.radius - 18);
    }

    ctx.restore();
  }

  updateHUD() {
    const p = this.player;
    const heroName = document.getElementById('hud-hero-name');
    const goldDisplay = document.getElementById('hud-gold');
    const duelDisplay = document.getElementById('hud-duel-timer');

    if (heroName) heroName.textContent = `${p.name} (Lv. ${p.level})`;
    if (goldDisplay) goldDisplay.textContent = `${this.gold.get()} G`;

    if (duelDisplay) {
      if (this.duelArena.isActive) {
        duelDisplay.textContent = `DUEL: ${Math.ceil(this.duelArena.duration)}s`;
      } else {
        const min = Math.floor(this.duelArena.timer / 60);
        const sec = Math.floor(this.duelArena.timer % 60);
        duelDisplay.textContent = `${String(min).padStart(2, '0')}:${String(sec).padStart(2, '0')}`;
      }
    }

    const updateOverlay = (key) => {
      const overlay = document.getElementById(`overlay-${key}`);
      const cdText = document.getElementById(`cd-text-${key}`);
      if (!overlay || !cdText) return;
      const curCd = p.cd[key];
      const maxCd = p.maxCd[key];
      if (curCd > 0) {
        const pct = (curCd / maxCd) * 100;
        overlay.style.height = `${pct}%`;
        cdText.style.display = 'block';
        cdText.textContent = curCd.toFixed(1);
      } else {
        overlay.style.height = '0%';
        cdText.style.display = 'none';
      }
    };

    updateOverlay('q');
    updateOverlay('w');
    updateOverlay('e');
    updateOverlay('r');
  }
}
