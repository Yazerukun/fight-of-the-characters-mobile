/* ============================================================================
 * === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA ===
 * Authentic Warcraft 3 Fight of the Characters (FOTC) 3D Arena Engine
 * WebGL MDX Native Model Pipeline, Real Animations, 3D Combat AI & Duel Arena
 * ============================================================================ */

export const ARENA_CONFIG = Object.freeze({
  FIXED_TIMESTEP: 1 / 60,
  MAX_FRAME_DELTA: 0.25,
  CAMERA_DISTANCE: 700,
  CAMERA_PITCH: Math.PI / 3.8, // Warcraft 3 isometric camera inclination
  CAMERA_HEADING: Math.PI / 2,
  CAMERA_LERP_SPEED: 0.08,
  
  // Arena Boundaries (XY Plane in Warcraft 3 coordinates)
  MAP_MIN_X: -800,
  MAP_MAX_X: 800,
  MAP_MIN_Y: -600,
  MAP_MAX_Y: 600,
  
  // Duel Arena Ring
  DUEL_CENTER: [0, 0, 0],
  DUEL_RADIUS: 220,
  DUEL_INTERVAL_SEC: 180, // 3 minutes
  DUEL_DURATION_SEC: 60,
  
  // Lane Spawner
  CREEP_SPAWN_INTERVAL_SEC: 25,
  MAX_CREEPS_PER_TEAM: 8,
  TEAM_1_SPAWN: [-650, 0, 0],
  TEAM_2_SPAWN: [650, 0, 0]
});

export const HERO_MODELS = Object.freeze({
  "ichigo": {
    file: "ichigo.mdx",
    scale: 1.0,
    anims: { walk: 2, attack: 0, attack2: 1, stand: 5, spell: 6, death: 3 }
  },
  "saber": {
    file: "saber.mdx",
    scale: 0.95,
    anims: { walk: 3, attack: 0, attack2: 5, stand: 4, spell: 9, death: 2 }
  },
  "luffy": {
    file: "luffy.mdx",
    scale: 0.85,
    anims: { walk: 5, attack: 6, attack2: 7, stand: 0, spell: 2, death: 1 }
  },
  "inuyasha": {
    file: "inuyasha.mdx",
    scale: 1.0,
    anims: { walk: 7, attack: 4, attack2: 5, stand: 8, spell: 3, death: 1 }
  },
  "rocklee": {
    file: "rocklee.mdx",
    scale: 0.9,
    anims: { walk: 4, attack: 2, attack2: 3, stand: 1, spell: 5, death: 6 }
  },
  "ace": {
    file: "ace.mdx",
    scale: 0.95,
    anims: { walk: 4, attack: 2, attack2: 3, stand: 0, spell: 5, death: 13 }
  },
  "byakuya": {
    file: "byakuya.mdx",
    scale: 0.95,
    anims: { walk: 2, attack: 0, attack2: 1, stand: 5, spell: 6, death: 3 }
  },
  "gojo": {
    file: "sasuke.mdx",
    scale: 1.05,
    anims: { walk: 2, attack: 0, attack2: 1, stand: 5, spell: 6, death: 3 }
  }
});

/* ============================================================================
 * === 02. SECURITY, CRYPTO & ANTI-CHEAT UTILITIES ===
 * ============================================================================ */

class SafeState {
  static #mask = 0x5a3c9e1f;

  static encode(val) {
    return (Math.floor(val) ^ SafeState.#mask);
  }

  static decode(val) {
    return (val ^ SafeState.#mask);
  }

  static sanitize(str) {
    if (typeof str !== 'string') return '';
    return str.replace(/[&<>"']/g, (m) => {
      switch (m) {
        case '&': return '&amp;';
        case '<': return '&lt;';
        case '>': return '&gt;';
        case '"': return '&quot;';
        case "'": return '&#39;';
        default: return m;
      }
    });
  }
}

/* ============================================================================
 * === 03. PRECISION MATH, VECTORS & COLLISION HELPERS ===
 * ============================================================================ */

class Vec3Util {
  static dist2D(x1, y1, x2, y2) {
    const dx = x2 - x1;
    const dy = y2 - y1;
    return Math.sqrt(dx * dx + dy * dy);
  }

  static clamp(val, min, max) {
    return Math.max(min, Math.min(max, val));
  }

  static lerp(a, b, t) {
    return a + (b - a) * t;
  }

  static setYawQuat(outQuat, angleRad) {
    const half = angleRad * 0.5;
    outQuat[0] = 0;
    outQuat[1] = 0;
    outQuat[2] = Math.sin(half);
    outQuat[3] = Math.cos(half);
    return outQuat;
  }
}

/* ============================================================================
 * === 04. RESILIENT STORAGE, AUDIO SYNTH & ASSET POOL ===
 * ============================================================================ */

class AudioSynthesizer {
  constructor() {
    this.ctx = null;
    this.masterGain = null;
    this.muted = false;
  }

  init() {
    if (this.ctx) return;
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) return;
    this.ctx = new AudioContextClass();
    this.masterGain = this.ctx.createGain();
    this.masterGain.gain.setValueAtTime(0.3, this.ctx.currentTime);
    this.masterGain.connect(this.ctx.destination);
  }

  toggleMute() {
    this.muted = !this.muted;
    if (this.masterGain) {
      this.masterGain.gain.setValueAtTime(this.muted ? 0.0 : 0.3, this.ctx ? this.ctx.currentTime : 0);
    }
    return !this.muted;
  }

  playSlash() {
    if (this.muted || !this.ctx) return;
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'sawtooth';
    osc.frequency.setValueAtTime(520, now);
    osc.frequency.exponentialRampToValueAtTime(110, now + 0.12);
    gain.gain.setValueAtTime(0.4, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.12);
    osc.connect(gain);
    gain.connect(this.masterGain);
    osc.start(now);
    osc.stop(now + 0.12);
  }

  playImpact() {
    if (this.muted || !this.ctx) return;
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'triangle';
    osc.frequency.setValueAtTime(160, now);
    osc.frequency.exponentialRampToValueAtTime(40, now + 0.18);
    gain.gain.setValueAtTime(0.6, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.18);
    osc.connect(gain);
    gain.connect(this.masterGain);
    osc.start(now);
    osc.stop(now + 0.18);
  }

  playSpell() {
    if (this.muted || !this.ctx) return;
    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(320, now);
    osc.frequency.exponentialRampToValueAtTime(880, now + 0.25);
    gain.gain.setValueAtTime(0.5, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.25);
    osc.connect(gain);
    gain.connect(this.masterGain);
    osc.start(now);
    osc.stop(now + 0.25);
  }

  playFanfare() {
    if (this.muted || !this.ctx) return;
    const notes = [261.63, 329.63, 392.00, 523.25];
    const now = this.ctx.currentTime;
    notes.forEach((freq, i) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(freq, now + i * 0.1);
      gain.gain.setValueAtTime(0.35, now + i * 0.1);
      gain.gain.exponentialRampToValueAtTime(0.01, now + i * 0.1 + 0.3);
      osc.connect(gain);
      gain.connect(this.masterGain);
      osc.start(now + i * 0.1);
      osc.stop(now + i * 0.1 + 0.35);
    });
  }
}

/* ============================================================================
 * === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT ===
 * ============================================================================ */

class Entity3D {
  constructor(id, name, teamId, modelKey, x, y, isHero = false) {
    this.id = id;
    this.name = name;
    this.teamId = teamId; // 1 = Radiant (Blue), 2 = Dire (Red)
    this.modelKey = modelKey;
    this.isHero = isHero;

    this.x = x;
    this.y = y;
    this.z = 0;
    this.vx = 0;
    this.vy = 0;
    this.facingAngle = teamId === 1 ? 0 : Math.PI;

    // Attributes (Integer precision)
    this.maxHealth = isHero ? 1400 : 420;
    this.health = this.maxHealth;
    this.maxMana = isHero ? 450 : 0;
    this.mana = this.maxMana;
    this.attackDamage = isHero ? 72 : 26;
    this.armor = isHero ? 5 : 2;
    this.moveSpeed = isHero ? 240 : 160;
    this.attackRange = isHero ? 120 : 90;
    this.attackCooldown = 0.0;
    this.attackInterval = isHero ? 0.95 : 1.35;

    // States
    this.isDead = false;
    this.isAttacking = false;
    this.attackTimer = 0.0;
    this.isCasting = false;
    this.castTimer = 0.0;
    this.currentAnim = "stand";
    this.isInDuel = false;
    this.isInvulnerable = false;
    this.isStunned = false;

    // 3D Model Instance Reference
    this.instance = null;
    this.modelDef = HERO_MODELS[modelKey] || HERO_MODELS["ichigo"];
    this.target = null;
  }

  setInstance(instance) {
    this.instance = instance;
    this.instance.setTeamColor(this.teamId === 1 ? 1 : 0); // 1 = Blue, 0 = Red
    this.instance.setUniformScale(this.modelDef.scale);
    this.instance.setLocation([this.x, this.y, this.z]);
    this.playAnimation("stand", true);
  }

  playAnimation(animName, loop = true) {
    if (!this.instance || this.currentAnim === animName) return;
    const seqIndex = this.modelDef.anims[animName];
    if (seqIndex !== undefined) {
      this.currentAnim = animName;
      this.instance.setSequenceLoopMode(loop ? 2 : 0);
      this.instance.setSequence(seqIndex);
    }
  }

  updateMovement(dt) {
    if (this.isDead || this.isAttacking || this.isCasting) return;
    if (Math.abs(this.vx) > 0.1 || Math.abs(this.vy) > 0.1) {
      this.x += this.vx * dt;
      this.y += this.vy * dt;
      this.facingAngle = Math.atan2(this.vy, this.vx);
      this.playAnimation("walk", true);
    } else {
      this.playAnimation("stand", true);
    }

    if (this.instance) {
      this.instance.setLocation([this.x, this.y, this.z]);
      const q = [0, 0, Math.sin(this.facingAngle * 0.5), Math.cos(this.facingAngle * 0.5)];
      this.instance.setRotation(q);
    }
  }

  takeDamage(rawDamage, dealer) {
    if (this.isDead) return 0;
    if (this.isInvulnerable) return 0;
    // Warcraft 3 Armor Mitigation Formula
    const reduction = (this.armor * 0.06) / (1 + this.armor * 0.06);
    const finalDmg = Math.max(1, Math.floor(rawDamage * (1 - reduction)));
    this.health = Math.max(0, this.health - finalDmg);

    if (this.health <= 0) {
      this.die(dealer);
    }
    return finalDmg;
  }

  die(dealer) {
    this.isDead = true;
    this.playAnimation("death", false);
  }
}

/* ============================================================================
 * === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC ===
 * ============================================================================ */

export class FOTC3DArena {
  constructor(canvasElement, toastContainer) {
    this.canvas = canvasElement;
    this.toasts = toastContainer;
    this.audio = new AudioSynthesizer();

    // 2D Terrain Surface Canvas (Authentic WC3 Map Pipeline)
    this.terrainCanvas = document.getElementById('terrainCanvas');
    this.terrainCtx = this.terrainCanvas ? this.terrainCanvas.getContext('2d') : null;

    // 3D Viewer & Scene
    this.viewer = null;
    this.scene = null;
    this.camera = null;
    this.loadedModels = new Map();

    // Game Entities
    this.playerHero = null;
    this.enemyHero = null;
    this.creeps = [];
    this.floatingTexts = [];

    // Target Selection & Combat Automation
    this.currentTarget = null;
    this.attackMoveTarget = null;
    this.rtsReticles = [];
    this.torchAnimTime = 0;

    // Player Safe State (Default Gojo Satoru)
    this._gold = SafeState.encode(350);
    this._kills = SafeState.encode(0);
    this.playerHeroKey = "gojo";

    // Skills Cooldowns
    this.skillCooldowns = { q: 0, w: 0, e: 0, r: 0, d: 0 };
    this.skillMaxCD = { q: 4.0, w: 7.0, e: 14.0, r: 40.0, d: 60.0 };

    // Duel State
    this.duelCountdown = ARENA_CONFIG.DUEL_INTERVAL_SEC;
    this.isDuelActive = false;
    this.duelTimer = 0;

    // Spawner State
    this.creepSpawnTimer = 3.0;

    // Input States
    this.keys = {};
    this.joystickDir = { x: 0, y: 0, active: false };

    // Projection Buffer
    this.screenCoordsBuf = new Float32Array(3);
    this.viewportBuf = new Float32Array(4);

    // Screen Shake
    this.screenShakeTrauma = 0;
  }

  get gold() {
    return SafeState.decode(this._gold);
  }

  set gold(val) {
    this._gold = SafeState.encode(Math.max(0, val));
    this._updateHUD();
  }

  get kills() {
    return SafeState.decode(this._kills);
  }

  set kills(val) {
    this._kills = SafeState.encode(Math.max(0, val));
  }

  async initEngine() {
    this.audio.init();

    // Setup Canvas Resolutions
    this.canvas.width = window.innerWidth;
    this.canvas.height = window.innerHeight;
    if (this.terrainCanvas) {
      this.terrainCanvas.width = window.innerWidth;
      this.terrainCanvas.height = window.innerHeight;
    }
    this.viewportBuf[0] = 0;
    this.viewportBuf[1] = 0;
    this.viewportBuf[2] = this.canvas.width;
    this.viewportBuf[3] = this.canvas.height;

    // Initialize WebGL Model Viewer with transparent alpha
    const ModelViewerLib = window.ModelViewer || (window.viewer ? window.viewer : null);
    if (!ModelViewerLib) {
      console.error("ModelViewer library not found in window. Ensure viewer.min.js is loaded.");
      return;
    }

    this.viewer = new ModelViewerLib.viewer.ModelViewer(this.canvas, { alpha: true });
    this.viewer.addHandler(ModelViewerLib.viewer.handlers.mdx);
    this.viewer.addHandler(ModelViewerLib.viewer.handlers.blp);
    this.viewer.on('error', (e) => console.warn('[FOTC 3D Viewer Warning]', e.error, e.src, e.reason));

    this.scene = this.viewer.addScene();
    this.scene.alpha = true;

    // Override startFrame to guarantee transparent clearColor
    const gl = this.viewer.gl;
    this.viewer.startFrame = function() {
      gl.depthMask(true);
      gl.clearColor(0, 0, 0, 0); // Transparent WebGL buffer
      gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    };

    this.camera = this.scene.camera;
    this.camera.perspective(Math.PI / 4, this.canvas.width / this.canvas.height, 10, 25000);
    this.camera.moveToAndFace([-280, -450, 480], [-280, 0, 40], [0, 0, 1]);

    this._setupInputListeners();
    // 1. Fast parallel load of active match heroes
    await Promise.all([
      this._loadModel("gojo"),
      this._loadModel("saber")
    ]);
    this.spawnMatch();
    this._updateSkillLabels();
    this._startLoop();
    this.showToast("Authentic 3D Warcraft 3 Arena Ready!", "#38bdf8");

    // 2. Background preload other heroes
    this._preloadBackground(["ichigo", "inuyasha", "rocklee", "ace", "luffy", "byakuya"]);
  }

  pathSolver(src) {
    const filename = src.split(/[\\/]/).pop();
    return 'public/models/' + filename;
  }

  async _loadModel(key) {
    if (this.loadedModels.has(key)) return this.loadedModels.get(key);
    const def = HERO_MODELS[key];
    if (!def) return null;
    try {
      const model = await this.viewer.load('public/models/' + def.file, this.pathSolver.bind(this));
      if (model) {
        this.loadedModels.set(key, model);
        return model;
      }
    } catch (err) {
      console.warn(`Failed to load 3D model for ${key}:`, err);
    }
    return null;
  }

  async _preloadBackground(keys) {
    for (const key of keys) {
      await this._loadModel(key);
    }
  }

  spawnMatch() {
    // 1. Clear previous instances
    if (this.playerHero && this.playerHero.instance) {
      this.scene.removeInstance(this.playerHero.instance);
    }
    if (this.enemyHero && this.enemyHero.instance) {
      this.scene.removeInstance(this.enemyHero.instance);
    }
    for (const c of this.creeps) {
      if (c.instance) this.scene.removeInstance(c.instance);
    }
    this.creeps = [];

    // 2. Spawn Player Hero (Team 1 Radiant / Blue)
    this.playerHero = new Entity3D(
      "hero_player",
      this._getHeroDisplayName(this.playerHeroKey),
      1,
      this.playerHeroKey,
      -280,
      0,
      true
    );
    const pModel = this.loadedModels.get(this.playerHeroKey);
    if (pModel) {
      const inst = pModel.addInstance();
      this.scene.addInstance(inst);
      this.playerHero.setInstance(inst);
    }

    // 3. Spawn Enemy AI Hero (Team 2 Dire / Red)
    const enemyKey = this.playerHeroKey === "saber" ? "inuyasha" : "saber";
    this.enemyHero = new Entity3D(
      "hero_enemy",
      this._getHeroDisplayName(enemyKey),
      2,
      enemyKey,
      280,
      0,
      true
    );
    const eModel = this.loadedModels.get(enemyKey);
    if (eModel) {
      const inst = eModel.addInstance();
      this.scene.addInstance(inst);
      this.enemyHero.setInstance(inst);
    }

    this._updateHUD();
  }

  switchPlayerHero(heroKey) {
    if (!HERO_MODELS[heroKey]) return;
    this.playerHeroKey = heroKey;
    if (!this.loadedModels.has(heroKey)) {
      this._loadModel(heroKey).then(() => {
        this.spawnMatch();
        this._updateSkillLabels();
      });
    } else {
      this.spawnMatch();
      this._updateSkillLabels();
    }
    this.showToast(`Hero switched to: ${this._getHeroDisplayName(heroKey)}!`, "#a855f7");
  }

  _updateSkillLabels() {
    const qEl = document.getElementById('name-skill-q');
    const wEl = document.getElementById('name-skill-w');
    const eEl = document.getElementById('name-skill-e');
    const rEl = document.getElementById('name-skill-r');
    const dEl = document.getElementById('name-skill-d');

    if (this.playerHeroKey === 'gojo') {
      if (qEl) qEl.textContent = "Blue";
      if (wEl) wEl.textContent = "Red";
      if (eEl) eEl.textContent = "Infinity";
      if (rEl) rEl.textContent = "Purple";
      if (dEl) dEl.textContent = "Void";
    } else if (this.playerHeroKey === 'ichigo') {
      if (qEl) qEl.textContent = "Slash";
      if (wEl) wEl.textContent = "Shunpo";
      if (eEl) eEl.textContent = "Bankai";
      if (rEl) rEl.textContent = "Getsuga";
      if (dEl) dEl.textContent = "Mask";
    } else if (this.playerHeroKey === 'saber') {
      if (qEl) qEl.textContent = "Strike";
      if (wEl) wEl.textContent = "Wind";
      if (eEl) eEl.textContent = "Avalon";
      if (rEl) rEl.textContent = "Excalibur";
      if (dEl) dEl.textContent = "Caliburn";
    } else if (this.playerHeroKey === 'luffy') {
      if (qEl) qEl.textContent = "Pistol";
      if (wEl) wEl.textContent = "Bazooka";
      if (eEl) eEl.textContent = "Gear 2";
      if (rEl) rEl.textContent = "Gatling";
      if (dEl) dEl.textContent = "Gear 3";
    } else {
      if (qEl) qEl.textContent = "Skill 1";
      if (wEl) wEl.textContent = "Skill 2";
      if (eEl) eEl.textContent = "Skill 3";
      if (rEl) rEl.textContent = "ULT";
      if (dEl) dEl.textContent = "Domain";
    }
  }

  _getHeroDisplayName(key) {
    switch (key) {
      case "ichigo": return "Kurosaki Ichigo";
      case "saber": return "Saber (Artoria)";
      case "luffy": return "Monkey D. Luffy";
      case "inuyasha": return "Inuyasha";
      case "rocklee": return "Rock Lee";
      case "ace": return "Portgas D. Ace";
      case "byakuya": return "Kuchiki Byakuya";
      case "gojo": return "Gojo Satoru (Limitless)";
      default: return key.toUpperCase();
    }
  }

  /* ============================================================================
   * === 07. RENDER PIPELINE, UI HUD, MODALS & TOASTS ===
   * ============================================================================ */

  _startLoop() {
    let lastTime = performance.now();
    let accumulator = 0;

    const frame = (now) => {
      const rawDelta = (now - lastTime) / 1000;
      lastTime = now;
      const dt = Math.min(rawDelta, ARENA_CONFIG.MAX_FRAME_DELTA);

      accumulator += dt;
      while (accumulator >= ARENA_CONFIG.FIXED_TIMESTEP) {
        this.fixedUpdate(ARENA_CONFIG.FIXED_TIMESTEP);
        accumulator -= ARENA_CONFIG.FIXED_TIMESTEP;
      }

      this.render();
      requestAnimationFrame(frame);
    };

    requestAnimationFrame(frame);
  }

  fixedUpdate(dt) {
    // 1. Tick Timers & Cooldowns
    for (const s of ['q', 'w', 'e', 'r']) {
      if (this.skillCooldowns[s] > 0) {
        this.skillCooldowns[s] = Math.max(0, this.skillCooldowns[s] - dt);
      }
    }
    this._updateSkillButtonCooldowns();

    // 2. Duel Director
    this._tickDuel(dt);

    // 3. Creep Spawner
    this._tickCreepSpawner(dt);

    // 4. Update Player Controls
    this._updatePlayerControls(dt);

    // 5. Update Enemy Hero AI
    this._updateEnemyAI(dt);

    // 6. Update Creeps AI & Combat
    this._updateCreepsAI(dt);

    // 7. Update Floating Damage Texts
    for (let i = this.floatingTexts.length - 1; i >= 0; i--) {
      const ft = this.floatingTexts[i];
      ft.life -= dt;
      ft.z += 40 * dt;
      if (ft.life <= 0) {
        if (ft.dom && ft.dom.parentNode) ft.dom.parentNode.removeChild(ft.dom);
        this.floatingTexts.splice(i, 1);
      }
    }

    // 8. Screen Shake Decay
    if (this.screenShakeTrauma > 0) {
      this.screenShakeTrauma = Math.max(0, this.screenShakeTrauma - dt * 2.5);
    }
  }

  _tickDuel(dt) {
    if (!this.isDuelActive) {
      this.duelCountdown -= dt;
      if (this.duelCountdown <= 0) {
        this._startDuel();
      }
    } else {
      this.duelTimer -= dt;
      this._enforceDuelBoundary(this.playerHero);
      this._enforceDuelBoundary(this.enemyHero);
      if (this.duelTimer <= 0 || this.playerHero.isDead || this.enemyHero.isDead) {
        this._endDuel();
      }
    }
    this._updateDuelHUD();
  }

  _startDuel() {
    this.isDuelActive = true;
    this.duelTimer = ARENA_CONFIG.DUEL_DURATION_SEC;
    this.playerHero.isInDuel = true;
    this.enemyHero.isInDuel = true;

    // Teleport to duel circle
    this.playerHero.x = -110;
    this.playerHero.y = 0;
    this.enemyHero.x = 110;
    this.enemyHero.y = 0;

    this.audio.playFanfare();
    this.showToast("⚔️ 1v1 DUEL EVENT STARTED! ENTER THE RING!", "#ef4444");
  }

  _enforceDuelBoundary(entity) {
    if (!entity || entity.isDead) return;
    const d = Vec3Util.dist2D(entity.x, entity.y, ARENA_CONFIG.DUEL_CENTER[0], ARENA_CONFIG.DUEL_CENTER[1]);
    if (d > ARENA_CONFIG.DUEL_RADIUS) {
      const angle = Math.atan2(entity.y, entity.x);
      entity.x = Math.cos(angle) * (ARENA_CONFIG.DUEL_RADIUS - 5);
      entity.y = Math.sin(angle) * (ARENA_CONFIG.DUEL_RADIUS - 5);
    }
  }

  _endDuel() {
    this.isDuelActive = false;
    this.duelCountdown = ARENA_CONFIG.DUEL_INTERVAL_SEC;
    this.playerHero.isInDuel = false;
    this.enemyHero.isInDuel = false;

    let victorText = "Duel Ended in Draw!";
    if (this.playerHero.health > this.enemyHero.health) {
      this.gold += 500;
      this.kills += 1;
      victorText = "🏆 Player WON THE DUEL! +500 Gold Bounty!";
      this.audio.playFanfare();
    } else if (this.enemyHero.health > this.playerHero.health) {
      victorText = "💀 Enemy Won The Duel!";
    }

    // Teleport back to home lanes
    this.playerHero.x = -280;
    this.playerHero.y = 0;
    this.enemyHero.x = 280;
    this.enemyHero.y = 0;

    this.showToast(victorText, "#fbbf24");
  }

  _tickCreepSpawner(dt) {
    this.creepSpawnTimer -= dt;
    if (this.creepSpawnTimer <= 0) {
      this.creepSpawnTimer = ARENA_CONFIG.CREEP_SPAWN_INTERVAL_SEC;
      this._spawnCreepWave();
    }
  }

  _spawnCreepWave() {
    const t1Count = this.creeps.filter(c => c.teamId === 1 && !c.isDead).length;
    const t2Count = this.creeps.filter(c => c.teamId === 2 && !c.isDead).length;

    if (t1Count < ARENA_CONFIG.MAX_CREEPS_PER_TEAM) {
      const c1 = new Entity3D(
        "creep_t1_" + Date.now(),
        "Radiant Footman",
        1,
        "rocklee",
        ARENA_CONFIG.TEAM_1_SPAWN[0],
        (Math.random() - 0.5) * 80
      );
      const m1 = this.loadedModels.get("rocklee");
      if (m1) {
        const inst = m1.addInstance();
        this.scene.addInstance(inst);
        c1.setInstance(inst);
      }
      this.creeps.push(c1);
    }

    if (t2Count < ARENA_CONFIG.MAX_CREEPS_PER_TEAM) {
      const c2 = new Entity3D(
        "creep_t2_" + Date.now(),
        "Dire Grunt",
        2,
        "ace",
        ARENA_CONFIG.TEAM_2_SPAWN[0],
        (Math.random() - 0.5) * 80
      );
      const m2 = this.loadedModels.get("ace");
      if (m2) {
        const inst = m2.addInstance();
        this.scene.addInstance(inst);
        c2.setInstance(inst);
      }
      this.creeps.push(c2);
    }
  }

  _updatePlayerControls(dt) {
    if (!this.playerHero || this.playerHero.isDead) return;

    let moveX = 0;
    let moveY = 0;

    // Keyboard WASD / Arrows
    if (this.keys['KeyW'] || this.keys['ArrowUp']) moveY += 1;
    if (this.keys['KeyS'] || this.keys['ArrowDown']) moveY -= 1;
    if (this.keys['KeyA'] || this.keys['ArrowLeft']) moveX -= 1;
    if (this.keys['KeyD'] || this.keys['ArrowRight']) moveX += 1;

    // Virtual Touch Joystick Override
    if (this.joystickDir.active) {
      moveX = this.joystickDir.x;
      moveY = this.joystickDir.y;
    }

    const len = Math.hypot(moveX, moveY);
    if (len > 0.05) {
      this.playerHero.vx = (moveX / len) * this.playerHero.moveSpeed;
      this.playerHero.vy = (moveY / len) * this.playerHero.moveSpeed;
    } else if (this.attackMoveTarget && !this.attackMoveTarget.isDead) {
      // Approach target when player tapped ATTACK button while out of range
      const d = Vec3Util.dist2D(this.playerHero.x, this.playerHero.y, this.attackMoveTarget.x, this.attackMoveTarget.y);
      if (d <= this.playerHero.attackRange) {
        this.playerHero.vx = 0;
        this.playerHero.vy = 0;
        this.playerHero.facingAngle = Math.atan2(this.attackMoveTarget.y - this.playerHero.y, this.attackMoveTarget.x - this.playerHero.x);
        if (this.playerHero.attackCooldown <= 0) {
          this._executeHeroAttack(this.playerHero, this.attackMoveTarget);
        }
        this.attackMoveTarget = null;
      } else {
        const ang = Math.atan2(this.attackMoveTarget.y - this.playerHero.y, this.attackMoveTarget.x - this.playerHero.x);
        this.playerHero.vx = Math.cos(ang) * this.playerHero.moveSpeed;
        this.playerHero.vy = Math.sin(ang) * this.playerHero.moveSpeed;
        this.playerHero.facingAngle = ang;
      }
    } else {
      this.playerHero.vx = 0;
      this.playerHero.vy = 0;
    }

    // Boundary Clamp
    this.playerHero.x = Vec3Util.clamp(this.playerHero.x, ARENA_CONFIG.MAP_MIN_X, ARENA_CONFIG.MAP_MAX_X);
    this.playerHero.y = Vec3Util.clamp(this.playerHero.y, ARENA_CONFIG.MAP_MIN_Y, ARENA_CONFIG.MAP_MAX_Y);

    this.playerHero.updateMovement(dt);

    // Auto-attack nearest enemy if in range and not moving
    if (len <= 0.05 && !this.attackMoveTarget && this.playerHero.attackCooldown <= 0) {
      const enemy = this._findNearestEnemy(this.playerHero, this.playerHero.attackRange);
      if (enemy) {
        this._executeHeroAttack(this.playerHero, enemy);
      }
    } else if (this.playerHero.attackCooldown > 0) {
      this.playerHero.attackCooldown -= dt;
    }
  }

  _updateEnemyAI(dt) {
    if (!this.enemyHero || this.enemyHero.isDead) return;

    if (this.enemyHero.attackCooldown > 0) {
      this.enemyHero.attackCooldown -= dt;
    }

    // Find nearest target (Player Hero or Creep)
    const target = this._findNearestEnemy(this.enemyHero, 450);
    if (target) {
      const dist = Vec3Util.dist2D(this.enemyHero.x, this.enemyHero.y, target.x, target.y);
      if (dist > this.enemyHero.attackRange) {
        const dx = target.x - this.enemyHero.x;
        const dy = target.y - this.enemyHero.y;
        this.enemyHero.vx = (dx / dist) * this.enemyHero.moveSpeed;
        this.enemyHero.vy = (dy / dist) * this.enemyHero.moveSpeed;
      } else {
        this.enemyHero.vx = 0;
        this.enemyHero.vy = 0;
        if (this.enemyHero.attackCooldown <= 0) {
          this._executeHeroAttack(this.enemyHero, target);
        }
      }
    } else {
      // Roam towards center
      const dx = -this.enemyHero.x;
      const dy = -this.enemyHero.y;
      const dist = Math.hypot(dx, dy);
      if (dist > 30) {
        this.enemyHero.vx = (dx / dist) * (this.enemyHero.moveSpeed * 0.5);
        this.enemyHero.vy = (dy / dist) * (this.enemyHero.moveSpeed * 0.5);
      } else {
        this.enemyHero.vx = 0;
        this.enemyHero.vy = 0;
      }
    }

    this.enemyHero.updateMovement(dt);
  }

  _updateCreepsAI(dt) {
    for (let i = this.creeps.length - 1; i >= 0; i--) {
      const creep = this.creeps[i];
      if (creep.isDead) {
        creep.attackTimer += dt;
        if (creep.attackTimer > 3.0) {
          if (creep.instance) this.scene.removeInstance(creep.instance);
          this.creeps.splice(i, 1);
        }
        continue;
      }

      if (creep.attackCooldown > 0) {
        creep.attackCooldown -= dt;
      }

      // Check aggro (250 units)
      const target = this._findNearestEnemy(creep, 250);
      if (target) {
        const dist = Vec3Util.dist2D(creep.x, creep.y, target.x, target.y);
        if (dist > creep.attackRange) {
          const dx = target.x - creep.x;
          const dy = target.y - creep.y;
          creep.vx = (dx / dist) * creep.moveSpeed;
          creep.vy = (dy / dist) * creep.moveSpeed;
        } else {
          creep.vx = 0;
          creep.vy = 0;
          if (creep.attackCooldown <= 0) {
            this._executeCreepAttack(creep, target);
          }
        }
      } else {
        // March towards opposing base
        const targetX = creep.teamId === 1 ? ARENA_CONFIG.TEAM_2_SPAWN[0] : ARENA_CONFIG.TEAM_1_SPAWN[0];
        const dx = targetX - creep.x;
        const dist = Math.abs(dx);
        if (dist > 20) {
          creep.vx = Math.sign(dx) * creep.moveSpeed;
          creep.vy = 0;
        } else {
          creep.vx = 0;
          creep.vy = 0;
        }
      }

      creep.updateMovement(dt);
    }
  }

  _findNearestEnemy(source, maxRadius) {
    const enemies = [];
    if (source.teamId === 1) {
      if (this.enemyHero && !this.enemyHero.isDead) enemies.push(this.enemyHero);
      enemies.push(...this.creeps.filter(c => c.teamId === 2 && !c.isDead));
    } else {
      if (this.playerHero && !this.playerHero.isDead) enemies.push(this.playerHero);
      enemies.push(...this.creeps.filter(c => c.teamId === 1 && !c.isDead));
    }

    let nearest = null;
    let minDist = maxRadius;
    for (const e of enemies) {
      const d = Vec3Util.dist2D(source.x, source.y, e.x, e.y);
      if (d < minDist) {
        minDist = d;
        nearest = e;
      }
    }
    return nearest;
  }

  _executeHeroAttack(attacker, victim) {
    attacker.attackCooldown = attacker.attackInterval;
    attacker.isAttacking = true;
    attacker.playAnimation(Math.random() > 0.5 ? "attack" : "attack2", false);
    this.audio.playSlash();

    // Damage timing delay (0.2s for authentic animation sync)
    setTimeout(() => {
      attacker.isAttacking = false;
      if (!victim.isDead) {
        const dmg = attacker.attackDamage + Math.floor(Math.random() * 12 - 6);
        const dealt = victim.takeDamage(dmg, attacker);
        this.audio.playImpact();
        this._spawnFloatingText(`-${dealt}`, victim.x, victim.y, dealt > 80 ? "#ef4444" : "#f59e0b");
        this.screenShakeTrauma = 0.25;

        if (victim.isDead && attacker === this.playerHero) {
          const bounty = victim.isHero ? 300 : 42;
          this.gold += bounty;
          if (victim.isHero) this.kills += 1;
          this.showToast(`Enemy Eliminated! +${bounty} Gold!`, "#22c55e");
        }
      }
    }, 200);
  }

  _executeCreepAttack(attacker, victim) {
    attacker.attackCooldown = attacker.attackInterval;
    attacker.isAttacking = true;
    attacker.playAnimation("attack", false);
    setTimeout(() => {
      attacker.isAttacking = false;
      if (!victim.isDead) {
        const dmg = attacker.attackDamage + Math.floor(Math.random() * 6 - 3);
        const dealt = victim.takeDamage(dmg, attacker);
        this._spawnFloatingText(`-${dealt}`, victim.x, victim.y, "#94a3b8");
      }
    }, 220);
  }

  /* === SKILL CASTS === */
  castSkill(slot) {
    if (!this.playerHero || this.playerHero.isDead) return;
    if (this.skillCooldowns[slot] > 0) {
      this.showToast(`Skill [${slot.toUpperCase()}] is on cooldown!`, "#f59e0b");
      return;
    }

    if (this.playerHeroKey === 'gojo') {
      switch (slot) {
        case 'q': this._castGojoBlue(); break;
        case 'w': this._castGojoRed(); break;
        case 'e': this._castGojoInfinity(); break;
        case 'r': this._castGojoHollowPurple(); break;
        case 'd': this._castGojoDomainExpansion(); break;
      }
      return;
    }

    switch (slot) {
      case 'q':
        this._castGetsugaTenshou();
        break;
      case 'w':
        this._castShunpo();
        break;
      case 'e':
        this._castHollowMask();
        break;
      case 'r':
        this._castBankai();
        break;
      case 'd':
        this._castMugetsu();
        break;
    }
  }

  _castGojoBlue() {
    this.skillCooldowns.q = this.skillMaxCD.q;
    this.playerHero.playAnimation("spell", false);
    this.audio.playSpell();
    const angle = this.playerHero.facingAngle;
    const targetX = this.playerHero.x + Math.cos(angle) * 180;
    const targetY = this.playerHero.y + Math.sin(angle) * 180;

    const enemies = this._getAllEnemiesInRadius({ x: targetX, y: targetY, teamId: this.playerHero.teamId }, 280);
    for (const t of enemies) {
      t.x = targetX + (Math.random() - 0.5) * 40;
      t.y = targetY + (Math.random() - 0.5) * 40;
      const dealt = t.takeDamage(260, this.playerHero);
      this._spawnFloatingText(`🌀 BLUE ${dealt}`, t.x, t.y, "#0ea5e9");
    }
    this.screenShakeTrauma = 0.45;
    this.showToast("Cursed Technique Lapse: BLUE (Gravitational Suction)!", "#0ea5e9");
  }

  _castGojoRed() {
    this.skillCooldowns.w = this.skillMaxCD.w;
    this.playerHero.playAnimation("attack", false);
    this.audio.playImpact();
    const angle = this.playerHero.facingAngle;
    const enemies = this._getAllEnemiesInCone(this.playerHero, angle, 320, Math.PI / 2.2);
    for (const t of enemies) {
      t.x += Math.cos(angle) * 240;
      t.y += Math.sin(angle) * 240;
      const dealt = t.takeDamage(320, this.playerHero);
      this._spawnFloatingText(`💥 RED ${dealt}`, t.x, t.y, "#ef4444");
    }
    this.screenShakeTrauma = 0.6;
    this.showToast("Cursed Technique Reversal: RED (Repulsive Blast)!", "#ef4444");
  }

  _castGojoInfinity() {
    this.skillCooldowns.e = this.skillMaxCD.e;
    this.audio.playSpell();
    this.playerHero.isInvulnerable = true;
    this.playerHero.moveSpeed += 90;
    this._spawnFloatingText("🛡️ INFINITY", this.playerHero.x, this.playerHero.y, "#38bdf8");
    this.showToast("Limitless: INFINITY ACTIVATED! 100% Invulnerability (4.5s)", "#38bdf8");
    setTimeout(() => {
      if (this.playerHero) {
        this.playerHero.isInvulnerable = false;
        this.playerHero.moveSpeed -= 90;
      }
    }, 4500);
  }

  _castGojoHollowPurple() {
    this.skillCooldowns.r = this.skillMaxCD.r;
    this.playerHero.playAnimation("spell", false);
    this.audio.playSpell();
    this.screenShakeTrauma = 1.0;
    const angle = this.playerHero.facingAngle;
    const enemies = this._getAllEnemiesInCone(this.playerHero, angle, 680, Math.PI / 1.8);
    for (const t of enemies) {
      const dealt = t.takeDamage(680, this.playerHero);
      this._spawnFloatingText(`🟣 HOLLOW PURPLE ${dealt}`, t.x, t.y, "#a855f7");
    }
    this.showToast("SECRET ART: HOLLOW PURPLE! CATACLYSMIC BEAM FIRED!", "#c084fc");
  }

  _castGojoDomainExpansion() {
    this.skillCooldowns.d = this.skillMaxCD.d;
    this.playerHero.playAnimation("spell", false);
    this.audio.playFanfare();
    this.screenShakeTrauma = 1.0;
    this._triggerDomainOverlay();

    const enemies = this._getAllEnemiesInRadius(this.playerHero, 580);
    for (const t of enemies) {
      t.isStunned = true;
      const dealt = t.takeDamage(850, this.playerHero);
      this._spawnFloatingText(`🌌 UNLIMITED VOID ${dealt}`, t.x, t.y, "#818cf8");
      setTimeout(() => { t.isStunned = false; }, 4200);
    }
    this.showToast("RYŌIKI TENKAI: UNLIMITED VOID (MURYŌKŪSHO)!", "#818cf8");
  }

  _castMugetsu() {
    this.skillCooldowns.d = this.skillMaxCD.d;
    this.playerHero.playAnimation("spell", false);
    this.audio.playFanfare();
    this.screenShakeTrauma = 1.0;
    const enemies = this._getAllEnemiesInRadius(this.playerHero, 550);
    for (const t of enemies) {
      const dealt = t.takeDamage(880, this.playerHero);
      this._spawnFloatingText(`⚔️ MUGETSU ${dealt}`, t.x, t.y, "#dc2626");
    }
    this.showToast("FINAL GETSUGA TENSHOU: MUGETSU!", "#dc2626");
  }

  _triggerDomainOverlay() {
    const ov = document.createElement('div');
    ov.style.position = 'fixed';
    ov.style.top = '0';
    ov.style.left = '0';
    ov.style.width = '100vw';
    ov.style.height = '100vh';
    ov.style.background = 'radial-gradient(circle, rgba(99,102,241,0.4) 0%, rgba(2,6,23,0.92) 80%)';
    ov.style.zIndex = '15';
    ov.style.pointerEvents = 'none';
    ov.style.display = 'flex';
    ov.style.flexDirection = 'column';
    ov.style.alignItems = 'center';
    ov.style.justifyContent = 'center';
    ov.style.animation = 'fadeInOut 4.2s ease forwards';
    ov.innerHTML = `
      <div style="font-size: 32px; font-weight: 900; color: #a5b4fc; text-shadow: 0 0 20px #6366f1; letter-spacing: 2px;">
        領域展開 : 無量空処
      </div>
      <div style="font-size: 16px; font-weight: bold; color: #e0e7ff; margin-top: 8px;">
        DOMAIN EXPANSION: UNLIMITED VOID
      </div>
    `;
    document.body.appendChild(ov);
    setTimeout(() => {
      if (ov.parentNode) ov.parentNode.removeChild(ov);
    }, 4200);
  }

  _castGetsugaTenshou() {
    this.skillCooldowns.q = this.skillMaxCD.q;
    this.playerHero.playAnimation("spell", false);
    this.audio.playSpell();

    // Line Slash AoE Damage
    const angle = this.playerHero.facingAngle;
    const slashRange = 320;
    const targets = this._getAllEnemiesInCone(this.playerHero, angle, slashRange, Math.PI / 3);

    for (const t of targets) {
      const dmg = 210 + Math.floor(Math.random() * 40);
      const dealt = t.takeDamage(dmg, this.playerHero);
      this._spawnFloatingText(`⚡ ${dealt}`, t.x, t.y, "#38bdf8");
    }

    this.screenShakeTrauma = 0.5;
    this.showToast("Getsuga Tenshou! Crescent Wave Fired!", "#38bdf8");
  }

  _castShunpo() {
    this.skillCooldowns.w = this.skillMaxCD.w;
    this.audio.playSpell();
    const angle = this.playerHero.facingAngle;
    this.playerHero.x += Math.cos(angle) * 220;
    this.playerHero.y += Math.sin(angle) * 220;
    this.showToast("Shunpo Flash Step!", "#a855f7");
  }

  _castHollowMask() {
    this.skillCooldowns.e = this.skillMaxCD.e;
    this.audio.playSpell();
    this.playerHero.attackDamage += 40;
    this.playerHero.moveSpeed += 80;
    this.showToast("Bankai Hollow Mask Activated! (+40 Dmg, +80 Speed)", "#ef4444");
    setTimeout(() => {
      this.playerHero.attackDamage -= 40;
      this.playerHero.moveSpeed -= 80;
    }, 8000);
  }

  _castBankai() {
    this.skillCooldowns.r = this.skillMaxCD.r;
    this.playerHero.playAnimation("spell", false);
    this.audio.playSpell();

    // Gigantic Burst around hero
    const enemies = this._getAllEnemiesInRadius(this.playerHero, 400);
    for (const t of enemies) {
      const dmg = 480 + Math.floor(Math.random() * 80);
      const dealt = t.takeDamage(dmg, this.playerHero);
      this._spawnFloatingText(`💥 CRIT ${dealt}`, t.x, t.y, "#dc2626");
    }

    this.screenShakeTrauma = 0.9;
    this.showToast("FINAL GETSUGA TENSHOU: BANKAI BURST!", "#dc2626");
  }

  _getAllEnemiesInCone(source, facingAngle, maxDist, spreadAngle) {
    const enemies = [];
    const all = source.teamId === 1 ? [this.enemyHero, ...this.creeps.filter(c => c.teamId === 2)] : [this.playerHero, ...this.creeps.filter(c => c.teamId === 1)];
    for (const e of all) {
      if (!e || e.isDead) continue;
      const d = Vec3Util.dist2D(source.x, source.y, e.x, e.y);
      if (d <= maxDist) {
        const ang = Math.atan2(e.y - source.y, e.x - source.x);
        let diff = Math.abs(ang - facingAngle);
        while (diff > Math.PI) diff -= Math.PI * 2;
        if (Math.abs(diff) <= spreadAngle * 0.5) {
          enemies.push(e);
        }
      }
    }
    return enemies;
  }

  _getAllEnemiesInRadius(source, radius) {
    const enemies = [];
    const all = source.teamId === 1 ? [this.enemyHero, ...this.creeps.filter(c => c.teamId === 2)] : [this.playerHero, ...this.creeps.filter(c => c.teamId === 1)];
    for (const e of all) {
      if (!e || e.isDead) continue;
      if (Vec3Util.dist2D(source.x, source.y, e.x, e.y) <= radius) {
        enemies.push(e);
      }
    }
    return enemies;
  }

  /* === RENDER PIPELINE === */
  render() {
    if (!this.viewer || !this.camera) return;

    // 1. Camera follow player with smooth lerp
    if (this.playerHero) {
      let shakeX = 0;
      let shakeY = 0;
      if (this.screenShakeTrauma > 0) {
        const p = this.screenShakeTrauma * this.screenShakeTrauma;
        shakeX = (Math.random() - 0.5) * 40 * p;
        shakeY = (Math.random() - 0.5) * 40 * p;
      }

      const targetCamX = this.playerHero.x + shakeX;
      const targetCamY = this.playerHero.y + shakeY;
      const curLoc = this.camera.location;
      const nextX = Vec3Util.lerp(curLoc[0], targetCamX, ARENA_CONFIG.CAMERA_LERP_SPEED);
      const nextY = Vec3Util.lerp(curLoc[1] + 450, targetCamY, ARENA_CONFIG.CAMERA_LERP_SPEED);
      this.camera.moveToAndFace([nextX, nextY - 450, 480], [nextX, nextY, 40], [0, 0, 1]);
    }

    // 2. Render Authentic Warcraft 3 Terrain & Map Elements on 2D Surface
    this._drawWarcraftTerrain();

    // 3. Update and render 3D WebGL scene (Heroes & Creeps)
    this.viewer.updateAndRender();

    // 4. Project 3D Overhead Health Bars & Floating Texts
    this._projectOverheadHUD();

    // 5. Update In-Game Bottom WC3 Hero Status & Target HUD
    this._updateHeroPanel();
    this._updateTargetHUD();
  }

  _projectWorldPoint(x, y, z) {
    if (!this.camera) return null;
    this.camera.worldToScreen(this.screenCoordsBuf, [x, y, z], this.viewportBuf);
    if (this.screenCoordsBuf[2] < 0 || this.screenCoordsBuf[2] > 1) return null;
    return [this.screenCoordsBuf[0], this.canvas.height - this.screenCoordsBuf[1]];
  }

  _drawWarcraftTerrain() {
    if (!this.terrainCtx || !this.camera) return;
    const ctx = this.terrainCtx;
    const w = this.canvas.width;
    const h = this.canvas.height;

    // Lordaeron Atmospheric Background
    const bgGrad = ctx.createLinearGradient(0, 0, 0, h);
    bgGrad.addColorStop(0, '#040711');
    bgGrad.addColorStop(0.5, '#090e1a');
    bgGrad.addColorStop(1, '#050a14');
    ctx.fillStyle = bgGrad;
    ctx.fillRect(0, 0, w, h);

    // Lordaeron / Dalaran Cobblestone Pavers Grid
    const stepX = 100;
    const stepY = 100;
    const minX = ARENA_CONFIG.MAP_MIN_X;
    const maxX = ARENA_CONFIG.MAP_MAX_X;
    const minY = ARENA_CONFIG.MAP_MIN_Y;
    const maxY = ARENA_CONFIG.MAP_MAX_Y;

    for (let x = minX; x < maxX; x += stepX) {
      for (let y = minY; y < maxY; y += stepY) {
        const p0 = this._projectWorldPoint(x, y, 0);
        const p1 = this._projectWorldPoint(x + stepX, y, 0);
        const p2 = this._projectWorldPoint(x + stepX, y + stepY, 0);
        const p3 = this._projectWorldPoint(x, y + stepY, 0);

        if (!p0 || !p1 || !p2 || !p3) continue;

        const minScreenX = Math.min(p0[0], p1[0], p2[0], p3[0]);
        const maxScreenX = Math.max(p0[0], p1[0], p2[0], p3[0]);
        const minScreenY = Math.min(p0[1], p1[1], p2[1], p3[1]);
        const maxScreenY = Math.max(p0[1], p1[1], p2[1], p3[1]);
        if (maxScreenX < -50 || minScreenX > w + 50 || maxScreenY < -50 || minScreenY > h + 50) continue;

        const hash = Math.abs(Math.sin(x * 12.9898 + y * 78.233) * 43758.5453) % 1;
        const isLane = Math.abs(y - 320) < 60 || Math.abs(y + 320) < 60;
        const isBaseBlue = x < -620;
        const isBaseRed = x > 620;

        ctx.beginPath();
        ctx.moveTo(p0[0], p0[1]);
        ctx.lineTo(p1[0], p1[1]);
        ctx.lineTo(p2[0], p2[1]);
        ctx.lineTo(p3[0], p3[1]);
        ctx.closePath();

        if (isLane) {
          ctx.fillStyle = hash > 0.5 ? '#242426' : '#2b2a2e';
        } else if (isBaseBlue) {
          ctx.fillStyle = hash > 0.5 ? '#162235' : '#1a293d';
        } else if (isBaseRed) {
          ctx.fillStyle = hash > 0.5 ? '#2d1b1f' : '#381f24';
        } else {
          if (hash > 0.75) ctx.fillStyle = '#2d2b28';
          else if (hash > 0.5) ctx.fillStyle = '#36332f';
          else if (hash > 0.25) ctx.fillStyle = '#23211f';
          else ctx.fillStyle = '#1c1b19';
        }
        ctx.fill();

        ctx.strokeStyle = '#121214';
        ctx.lineWidth = 1;
        ctx.stroke();
      }
    }

    // Iconic Center Duel Arena ("Ring of Honor")
    const duelCenter = this._projectWorldPoint(0, 0, 0);
    if (duelCenter) {
      const radius = ARENA_CONFIG.DUEL_RADIUS;
      const numSegments = 32;

      ctx.beginPath();
      for (let i = 0; i <= numSegments; i++) {
        const ang = (i / numSegments) * Math.PI * 2;
        const pt = this._projectWorldPoint(Math.cos(ang) * radius, Math.sin(ang) * radius, 1);
        if (pt) {
          if (i === 0) ctx.moveTo(pt[0], pt[1]);
          else ctx.lineTo(pt[0], pt[1]);
        }
      }
      ctx.closePath();
      ctx.fillStyle = 'rgba(56, 52, 48, 0.9)';
      ctx.fill();

      // Glowing Golden Runic Ring Inscription
      ctx.strokeStyle = '#d97706';
      ctx.lineWidth = 3;
      ctx.shadowColor = '#fbbf24';
      ctx.shadowBlur = 10;
      ctx.stroke();
      ctx.shadowBlur = 0;

      // Inner Concentric Duel Ring
      ctx.beginPath();
      for (let i = 0; i <= numSegments; i++) {
        const ang = (i / numSegments) * Math.PI * 2;
        const pt = this._projectWorldPoint(Math.cos(ang) * (radius * 0.72), Math.sin(ang) * (radius * 0.72), 2);
        if (pt) {
          if (i === 0) ctx.moveTo(pt[0], pt[1]);
          else ctx.lineTo(pt[0], pt[1]);
        }
      }
      ctx.closePath();
      ctx.strokeStyle = 'rgba(245, 158, 11, 0.6)';
      ctx.lineWidth = 2;
      ctx.stroke();

      // 4 Stone Pillars & Animated Duel Torches
      const pillarOffsets = [
        [-155, -155], [155, -155], [155, 155], [-155, 155]
      ];
      this.torchAnimTime += 0.08;

      pillarOffsets.forEach(([px, py], idx) => {
        const basePt = this._projectWorldPoint(px, py, 0);
        const topPt = this._projectWorldPoint(px, py, 42);
        if (basePt && topPt) {
          // Cast Ground Light
          const lightRadius = 24 + Math.sin(this.torchAnimTime + idx) * 3;
          const radialGrad = ctx.createRadialGradient(basePt[0], basePt[1], 2, basePt[0], basePt[1], lightRadius);
          radialGrad.addColorStop(0, 'rgba(251, 191, 36, 0.4)');
          radialGrad.addColorStop(1, 'rgba(245, 158, 11, 0)');
          ctx.fillStyle = radialGrad;
          ctx.beginPath();
          ctx.arc(basePt[0], basePt[1], lightRadius, 0, Math.PI * 2);
          ctx.fill();

          // Pillar Column
          ctx.strokeStyle = '#4b5563';
          ctx.lineWidth = 6;
          ctx.beginPath();
          ctx.moveTo(basePt[0], basePt[1]);
          ctx.lineTo(topPt[0], topPt[1]);
          ctx.stroke();

          // Brazier Bowl
          ctx.fillStyle = '#1f2937';
          ctx.beginPath();
          ctx.arc(topPt[0], topPt[1], 7, 0, Math.PI * 2);
          ctx.fill();

          // Animated Fire Flame
          const flameFlicker = Math.sin(this.torchAnimTime * 2 + idx * 1.5) * 4;
          ctx.fillStyle = '#f97316';
          ctx.beginPath();
          ctx.arc(topPt[0], topPt[1] - 5 + flameFlicker * 0.3, 5, 0, Math.PI * 2);
          ctx.fill();
          ctx.fillStyle = '#fef08a';
          ctx.beginPath();
          ctx.arc(topPt[0], topPt[1] - 7 + flameFlicker * 0.5, 2.5, 0, Math.PI * 2);
          ctx.fill();
        }
      });
    }

    // Base Fountains
    const base1Pt = this._projectWorldPoint(-700, 0, 0);
    if (base1Pt) {
      const pulse = (Math.sin(this.torchAnimTime * 1.2) + 1) * 0.5;
      ctx.beginPath();
      ctx.arc(base1Pt[0], base1Pt[1], 36 + pulse * 6, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(2, 132, 199, 0.25)';
      ctx.fill();
      ctx.strokeStyle = '#38bdf8';
      ctx.lineWidth = 2;
      ctx.stroke();
    }

    const base2Pt = this._projectWorldPoint(700, 0, 0);
    if (base2Pt) {
      const pulse = (Math.cos(this.torchAnimTime * 1.2) + 1) * 0.5;
      ctx.beginPath();
      ctx.arc(base2Pt[0], base2Pt[1], 36 + pulse * 6, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(220, 38, 38, 0.25)';
      ctx.fill();
      ctx.strokeStyle = '#ef4444';
      ctx.lineWidth = 2;
      ctx.stroke();
    }

    // Arena Perimeter Wall
    const corners = [
      this._projectWorldPoint(minX, minY, 0),
      this._projectWorldPoint(maxX, minY, 0),
      this._projectWorldPoint(maxX, maxY, 0),
      this._projectWorldPoint(minX, maxY, 0)
    ];
    if (corners[0] && corners[1] && corners[2] && corners[3]) {
      ctx.beginPath();
      ctx.moveTo(corners[0][0], corners[0][1]);
      ctx.lineTo(corners[1][0], corners[1][1]);
      ctx.lineTo(corners[2][0], corners[2][1]);
      ctx.lineTo(corners[3][0], corners[3][1]);
      ctx.closePath();
      ctx.strokeStyle = '#64748b';
      ctx.lineWidth = 4;
      ctx.stroke();
    }

    // Dynamic 3D Unit Shadows & Selection Circles
    const allUnits = [];
    if (this.playerHero && !this.playerHero.isDead) allUnits.push(this.playerHero);
    if (this.enemyHero && !this.enemyHero.isDead) allUnits.push(this.enemyHero);
    this.creeps.forEach(c => { if (!c.isDead) allUnits.push(c); });

    allUnits.forEach(u => {
      const footPt = this._projectWorldPoint(u.x, u.y, 0);
      if (!footPt) return;

      const isPlayer = u === this.playerHero;
      const isTarget = u === this.currentTarget;
      const radX = u.isHero ? 28 : 16;
      const radY = u.isHero ? 14 : 8;

      // Soft Dark Drop Shadow
      ctx.beginPath();
      ctx.ellipse(footPt[0], footPt[1], radX, radY, 0, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(0, 0, 0, 0.55)';
      ctx.fill();

      // Warcraft III Green Selection Circle (Player)
      if (isPlayer) {
        ctx.beginPath();
        ctx.ellipse(footPt[0], footPt[1], radX + 4, radY + 2, 0, 0, Math.PI * 2);
        ctx.strokeStyle = '#22c55e';
        ctx.lineWidth = 2;
        ctx.stroke();

        const bx = footPt[0];
        const by = footPt[1];
        ctx.strokeStyle = '#4ade80';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(bx - radX - 6, by - 4); ctx.lineTo(bx - radX - 6, by - radY - 4); ctx.lineTo(bx - 6, by - radY - 4);
        ctx.moveTo(bx + radX + 6, by - 4); ctx.lineTo(bx + radX + 6, by - radY - 4); ctx.lineTo(bx + 6, by - radY - 4);
        ctx.moveTo(bx - radX - 6, by + 4); ctx.lineTo(bx - radX - 6, by + radY + 4); ctx.lineTo(bx - 6, by + radY + 4);
        ctx.moveTo(bx + radX + 6, by + 4); ctx.lineTo(bx + radX + 6, by + radY + 4); ctx.lineTo(bx + 6, by + radY + 4);
        ctx.stroke();
      }

      // Warcraft III Red Targeting Circle (Current Target)
      if (isTarget) {
        const pulse = Math.sin(this.torchAnimTime * 4) * 2;
        ctx.beginPath();
        ctx.ellipse(footPt[0], footPt[1], radX + 6 + pulse, radY + 3 + pulse * 0.5, 0, 0, Math.PI * 2);
        ctx.strokeStyle = '#ef4444';
        ctx.lineWidth = 2.5;
        ctx.shadowColor = '#dc2626';
        ctx.shadowBlur = 8;
        ctx.stroke();
        ctx.shadowBlur = 0;
      }
    });

    // Animated Green RTS Move Reticles
    for (let i = this.rtsReticles.length - 1; i >= 0; i--) {
      const ret = this.rtsReticles[i];
      ret.life -= 0.035;
      if (ret.life <= 0) {
        this.rtsReticles.splice(i, 1);
        continue;
      }
      const pt = this._projectWorldPoint(ret.x, ret.y, 0);
      if (pt) {
        const size = 18 * ret.life;
        const alpha = ret.life;
        ctx.save();
        ctx.strokeStyle = `rgba(34, 197, 94, ${alpha})`;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(pt[0] - size, pt[1]); ctx.lineTo(pt[0] - size * 0.4, pt[1]);
        ctx.moveTo(pt[0] + size, pt[1]); ctx.lineTo(pt[0] + size * 0.4, pt[1]);
        ctx.moveTo(pt[0], pt[1] - size); ctx.lineTo(pt[0], pt[1] - size * 0.4);
        ctx.moveTo(pt[0], pt[1] + size); ctx.lineTo(pt[0], pt[1] + size * 0.4);
        ctx.stroke();
        ctx.restore();
      }
    }
  }

  /* === PLAYER ATTACK COMMAND & AUTO-TARGETING === */
  triggerPlayerAttack() {
    if (!this.playerHero || this.playerHero.isDead) return;
    if (this.playerHero.attackCooldown > 0) return;

    // 1. Check if current target is valid and alive
    let target = this.currentTarget;
    if (target && target.isDead) {
      this.currentTarget = null;
      target = null;
    }

    // 2. Scan all living enemies within acquire range (480 units)
    if (!target) {
      target = this._findNearestEnemy(this.playerHero, 480);
      if (target) {
        this.currentTarget = target;
        this._updateTargetHUD();
      }
    }

    // 3. Attack or Move-to-Attack
    if (target) {
      const dist = Vec3Util.dist2D(this.playerHero.x, this.playerHero.y, target.x, target.y);
      if (dist <= this.playerHero.attackRange) {
        this.playerHero.vx = 0;
        this.playerHero.vy = 0;
        this.attackMoveTarget = null;
        this.playerHero.facingAngle = Math.atan2(target.y - this.playerHero.y, target.x - this.playerHero.x);
        this._executeHeroAttack(this.playerHero, target);
      } else {
        this.attackMoveTarget = target;
        const ang = Math.atan2(target.y - this.playerHero.y, target.x - this.playerHero.x);
        this.playerHero.vx = Math.cos(ang) * this.playerHero.moveSpeed;
        this.playerHero.vy = Math.sin(ang) * this.playerHero.moveSpeed;
        this.playerHero.facingAngle = ang;
        this.playerHero.playAnimation("walk", true);
      }
    } else {
      // No enemies nearby -> Swing weapon forward
      this.playerHero.playAnimation("attack", false);
      this.audio.playSword();
      this.playerHero.attackCooldown = 0.3;
    }
  }

  _updateTargetHUD() {
    const hud = document.getElementById('target-hud');
    const nameEl = document.getElementById('target-name');
    const fillEl = document.getElementById('target-hp-fill');
    const statsEl = document.getElementById('target-stats');
    if (!hud) return;

    if (!this.currentTarget || this.currentTarget.isDead) {
      hud.style.display = 'none';
      return;
    }

    hud.style.display = 'block';
    if (nameEl) nameEl.textContent = this.currentTarget.name;
    const hpPct = Math.max(0, Math.min(100, (this.currentTarget.health / this.currentTarget.maxHealth) * 100));
    if (fillEl) fillEl.style.width = `${hpPct}%`;
    if (statsEl) statsEl.textContent = `HP: ${this.currentTarget.health} / ${this.currentTarget.maxHealth}`;
  }

  _updateHeroPanel() {
    if (!this.playerHero) return;
    const hpFill = document.getElementById('hero-hp-fill');
    const hpText = document.getElementById('hero-hp-text');
    const mpFill = document.getElementById('hero-mp-fill');
    const mpText = document.getElementById('hero-mp-text');
    const portraitImg = document.getElementById('hero-portrait-img');
    const levelBadge = document.getElementById('hero-level-badge');
    const statDmg = document.getElementById('stat-dmg');
    const statArm = document.getElementById('stat-arm');

    if (hpFill) {
      const pct = Math.max(0, Math.min(100, (this.playerHero.health / this.playerHero.maxHealth) * 100));
      hpFill.style.width = `${pct}%`;
    }
    if (hpText) {
      hpText.textContent = `${this.playerHero.health} / ${this.playerHero.maxHealth}`;
    }
    if (mpFill) {
      const pct = Math.max(0, Math.min(100, (this.playerHero.mana / this.playerHero.maxMana) * 100));
      mpFill.style.width = `${pct}%`;
    }
    if (mpText) {
      mpText.textContent = `${this.playerHero.mana} / ${this.playerHero.maxMana}`;
    }
    if (statDmg) statDmg.textContent = `${this.playerHero.attackDamage - 6}-${this.playerHero.attackDamage + 6}`;
    if (statArm) statArm.textContent = `${this.playerHero.armor.toFixed(1)}`;
    if (levelBadge) levelBadge.textContent = `${this.playerHero.level || 1}`;
    if (portraitImg) {
      const portraits = {
        gojo: "🧙‍♂️",
        ichigo: "⚔️",
        saber: "🛡️",
        luffy: "🍖",
        inuyasha: "🐾",
        rocklee: "🍃",
        ace: "🔥",
        byakuya: "🌸"
      };
      portraitImg.textContent = portraits[this.playerHeroKey] || "🧙‍♂️";
    }
  }

  _projectOverheadHUD() {
    const allUnits = [];
    if (this.playerHero) allUnits.push(this.playerHero);
    if (this.enemyHero) allUnits.push(this.enemyHero);
    allUnits.push(...this.creeps);

    for (const u of allUnits) {
      if (u.isDead) {
        if (u._hudElement) u._hudElement.style.display = 'none';
        continue;
      }

      if (!u._hudElement) {
        u._hudElement = this._createHUDElement(u);
      }

      // Convert 3D world coord to 2D screen coordinate
      this.camera.worldToScreen(this.screenCoordsBuf, [u.x, u.y, 85], this.viewportBuf);
      const screenX = this.screenCoordsBuf[0];
      const screenY = this.canvas.height - this.screenCoordsBuf[1];

      u._hudElement.style.display = 'block';
      u._hudElement.style.left = `${screenX}px`;
      u._hudElement.style.top = `${screenY}px`;

      // Update HP bar fill
      const hpPct = Math.max(0, Math.min(100, (u.health / u.maxHealth) * 100));
      const fillEl = u._hudElement.querySelector('.unit-hp-fill');
      if (fillEl) fillEl.style.width = `${hpPct}%`;
    }
  }

  _createHUDElement(unit) {
    const div = document.createElement('div');
    div.className = 'unit-3d-hud';
    div.style.position = 'absolute';
    div.style.transform = 'translate(-50%, -100%)';
    div.style.pointerEvents = 'none';
    div.style.zIndex = '5';

    const color = unit.teamId === 1 ? '#38bdf8' : '#ef4444';
    div.innerHTML = `
      <div style="font-size: 11px; font-weight: bold; color: ${color}; text-shadow: 0 1px 3px #000; text-align: center; margin-bottom: 2px;">
        ${SafeState.sanitize(unit.name)}
      </div>
      <div style="width: 64px; height: 6px; background: rgba(0,0,0,0.7); border: 1px solid rgba(255,255,255,0.25); border-radius: 3px; overflow: hidden;">
        <div class="unit-hp-fill" style="width: 100%; height: 100%; background: ${unit.teamId === 1 ? '#22c55e' : '#ef4444'}; transition: width 0.1s linear;"></div>
      </div>
    `;

    document.getElementById('game-container').appendChild(div);
    return div;
  }

  _spawnFloatingText(text, worldX, worldY, color = "#fff") {
    const el = document.createElement('div');
    el.className = 'floating-damage';
    el.textContent = text;
    el.style.position = 'absolute';
    el.style.color = color;
    el.style.fontWeight = 'bold';
    el.style.fontSize = '16px';
    el.style.textShadow = '0 2px 4px #000';
    el.style.pointerEvents = 'none';
    el.style.zIndex = '20';
    el.style.transition = 'opacity 0.8s, transform 0.8s';

    document.getElementById('game-container').appendChild(el);

    this.floatingTexts.push({
      dom: el,
      worldX,
      worldY,
      z: 90,
      life: 0.8
    });
  }

  _updateHUD() {
    const heroNameEl = document.getElementById('hud-hero-name');
    const goldEl = document.getElementById('hud-gold');
    if (heroNameEl && this.playerHero) {
      heroNameEl.textContent = `${this.playerHero.name} (Lv. 1)`;
    }
    if (goldEl) {
      goldEl.textContent = `${this.gold} G`;
    }
  }

  _updateDuelHUD() {
    const duelEl = document.getElementById('hud-duel-timer');
    if (!duelEl) return;
    if (this.isDuelActive) {
      const sec = Math.ceil(this.duelTimer);
      duelEl.textContent = `DUEL: ${sec}s`;
      duelEl.style.color = "#ef4444";
    } else {
      const min = Math.floor(this.duelCountdown / 60);
      const sec = Math.floor(this.duelCountdown % 60);
      duelEl.textContent = `${String(min).padStart(2, '0')}:${String(sec).padStart(2, '0')}`;
      duelEl.style.color = "#f87171";
    }
  }

  _updateSkillButtonCooldowns() {
    for (const slot of ['q', 'w', 'e', 'r', 'd']) {
      const btn = document.getElementById(`btn-skill-${slot}`);
      if (!btn) continue;
      const cd = this.skillCooldowns[slot];
      const cdOverlay = btn.querySelector('.cd-overlay') || btn.querySelector('.skill-cd-overlay');
      const cdText = btn.querySelector('.skill-cd-text');
      if (cd > 0) {
        btn.classList.add('on-cooldown');
        if (cdOverlay) cdOverlay.style.display = 'block';
        if (cdText) {
          cdText.textContent = cd.toFixed(1);
          cdText.style.display = 'block';
        }
      } else {
        btn.classList.remove('on-cooldown');
        if (cdOverlay) cdOverlay.style.display = 'none';
        if (cdText) cdText.style.display = 'none';
      }
    }
  }

  showToast(msg, color = "#38bdf8") {
    if (!this.toasts) return;
    const toast = document.createElement('div');
    toast.className = 'toast-msg';
    toast.style.borderColor = color;
    toast.style.color = color;
    toast.textContent = msg;
    this.toasts.appendChild(toast);
    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(-10px)';
      setTimeout(() => {
        if (toast.parentNode) toast.parentNode.removeChild(toast);
      }, 300);
    }, 2800);
  }

  /* ============================================================================
   * === 08. INPUT CONTROLLER, EVENT LISTENERS & BOOTSTRAP ===
   * ============================================================================ */

  _setupInputListeners() {
    window.addEventListener('keydown', (e) => {
      this.keys[e.code] = true;
      if (e.code === 'KeyQ') this.castSkill('q');
      if (e.code === 'KeyW') this.castSkill('w');
      if (e.code === 'KeyE') this.castSkill('e');
      if (e.code === 'KeyR') this.castSkill('r');
      if (e.code === 'KeyD') this.castSkill('d');
      if (e.code === 'KeyB') this._toggleShop();
      if (e.code === 'KeyH') this._toggleHeroModal();
      if (e.code === 'Space' || e.code === 'KeyA') {
        this.triggerPlayerAttack();
      }
    });

    window.addEventListener('keyup', (e) => {
      this.keys[e.code] = false;
    });

    // Mobile Virtual Joystick Touch
    const zone = document.getElementById('joystick-zone');
    const knob = document.getElementById('joystick-knob');
    if (zone && knob) {
      let touchId = null;
      let startX = 0, startY = 0;

      zone.addEventListener('touchstart', (e) => {
        e.preventDefault();
        const t = e.changedTouches[0];
        touchId = t.identifier;
        const rect = zone.getBoundingClientRect();
        startX = rect.left + rect.width / 2;
        startY = rect.top + rect.height / 2;
        this.joystickDir.active = true;
        this.attackMoveTarget = null; // Joystick cancels auto-walk
      }, { passive: false });

      window.addEventListener('touchmove', (e) => {
        if (touchId === null) return;
        for (let i = 0; i < e.changedTouches.length; i++) {
          const t = e.changedTouches[i];
          if (t.identifier === touchId) {
            const dx = t.clientX - startX;
            const dy = t.clientY - startY;
            const dist = Math.hypot(dx, dy);
            const maxDist = 45;
            const clampedDist = Math.min(dist, maxDist);
            const angle = Math.atan2(dy, dx);

            knob.style.transform = `translate(${Math.cos(angle) * clampedDist}px, ${Math.sin(angle) * clampedDist}px)`;
            this.joystickDir.x = Math.cos(angle);
            this.joystickDir.y = -Math.sin(angle); // Invert Y for 3D coordinate space
            break;
          }
        }
      }, { passive: false });

      const touchEnd = (e) => {
        for (let i = 0; i < e.changedTouches.length; i++) {
          if (e.changedTouches[i].identifier === touchId) {
            touchId = null;
            knob.style.transform = `translate(0px, 0px)`;
            this.joystickDir.x = 0;
            this.joystickDir.y = 0;
            this.joystickDir.active = false;
            break;
          }
        }
      };

      window.addEventListener('touchend', touchEnd);
      window.addEventListener('touchcancel', touchEnd);
    }

    // Dedicated Mobile Attack Button Listener
    const attackBtn = document.getElementById('btn-attack');
    if (attackBtn) {
      const onAttackTap = (e) => {
        e.preventDefault();
        if (navigator.vibrate) navigator.vibrate(20);
        this.triggerPlayerAttack();
      };
      attackBtn.addEventListener('touchstart', onAttackTap, { passive: false });
      attackBtn.addEventListener('mousedown', onAttackTap);
    }

    // Touch Action Buttons (Instant touchstart response)
    ['q', 'w', 'e', 'r', 'd'].forEach(slot => {
      const btn = document.getElementById(`btn-skill-${slot}`);
      if (btn) {
        const onSkillTap = (e) => {
          e.preventDefault();
          if (navigator.vibrate) navigator.vibrate(15);
          this.castSkill(slot);
        };
        btn.addEventListener('touchstart', onSkillTap, { passive: false });
        btn.addEventListener('mousedown', onSkillTap);
      }
    });

    // Warcraft 3 RTS Mouse Click-to-Move & Enemy Select
    this.canvas.addEventListener('contextmenu', (e) => e.preventDefault());
    this.canvas.addEventListener('click', (e) => this._handleRTSClick(e));

    const heroBtn = document.getElementById('btn-open-heroes');
    if (heroBtn) heroBtn.addEventListener('click', () => this._toggleHeroModal());

    const shopBtn = document.getElementById('btn-open-shop');
    if (shopBtn) shopBtn.addEventListener('click', () => this._toggleShop());

    const soundBtn = document.getElementById('btn-sound-toggle');
    if (soundBtn) {
      soundBtn.addEventListener('click', () => {
        const active = this.audio.toggleMute();
        soundBtn.textContent = active ? "Audio: ON" : "Audio: OFF";
      });
    }

    window.addEventListener('resize', () => {
      this.canvas.width = window.innerWidth;
      this.canvas.height = window.innerHeight;
      if (this.terrainCanvas) {
        this.terrainCanvas.width = window.innerWidth;
        this.terrainCanvas.height = window.innerHeight;
      }
      this.viewportBuf[2] = this.canvas.width;
      this.viewportBuf[3] = this.canvas.height;
      if (this.camera) {
        this.camera.perspective(Math.PI / 4, this.canvas.width / this.canvas.height, 10, 25000);
      }
    });
  }

  _handleRTSClick(e) {
    if (!this.playerHero || this.playerHero.isDead) return;

    // 1. Check if clicked near an enemy (Select Target)
    const allEnemies = [];
    if (this.enemyHero && !this.enemyHero.isDead) allEnemies.push(this.enemyHero);
    this.creeps.forEach(c => { if (c.teamId === 2 && !c.isDead) allEnemies.push(c); });

    let clickedEnemy = null;
    let minScreenDist = 55;
    allEnemies.forEach(en => {
      const pt = this._projectWorldPoint(en.x, en.y, 40);
      if (pt) {
        const d = Math.hypot(e.clientX - pt[0], e.clientY - pt[1]);
        if (d < minScreenDist) {
          minScreenDist = d;
          clickedEnemy = en;
        }
      }
    });

    if (clickedEnemy) {
      this.currentTarget = clickedEnemy;
      this._updateTargetHUD();
      this.showToast(`Target: ${clickedEnemy.name}`, "#ef4444");
      return;
    }

    // 2. Spawn iconic Warcraft 3 Click Reticle in 2D DOM & 3D World
    const reticle = document.createElement('div');
    reticle.className = 'wc3-click-reticle';
    reticle.style.left = `${e.clientX}px`;
    reticle.style.top = `${e.clientY}px`;
    document.getElementById('game-container').appendChild(reticle);
    setTimeout(() => {
      if (reticle.parentNode) reticle.parentNode.removeChild(reticle);
    }, 450);

    const centerX = this.canvas.width / 2;
    const centerY = this.canvas.height / 2;
    const dx = e.clientX - centerX;
    const dy = centerY - e.clientY;
    const dist = Math.hypot(dx, dy);

    const worldClickX = this.playerHero.x + (dx * 1.2);
    const worldClickY = this.playerHero.y + (dy * 1.2);
    this.rtsReticles.push({ x: worldClickX, y: worldClickY, life: 1.0 });

    if (dist > 20) {
      this.attackMoveTarget = null;
      this.playerHero.vx = (dx / dist) * this.playerHero.moveSpeed;
      this.playerHero.vy = (dy / dist) * this.playerHero.moveSpeed;
      setTimeout(() => {
        if (!this.joystickDir.active && !this.keys['KeyW'] && !this.keys['KeyS'] && !this.keys['KeyA'] && !this.keys['KeyD']) {
          if (this.playerHero) {
            this.playerHero.vx = 0;
            this.playerHero.vy = 0;
          }
        }
      }, 1200);
    }
  }

  _toggleHeroModal() {
    let modal = document.getElementById('hero-select-modal');
    if (!modal) {
      modal = document.createElement('div');
      modal.id = 'hero-select-modal';
      modal.className = 'game-modal';
      modal.innerHTML = `
        <div class="modal-card">
          <div class="modal-header">
            <h3>SELECT 3D WARCRAFT 3 HERO</h3>
            <button class="modal-close" onclick="document.getElementById('hero-select-modal').style.display='none'">✕</button>
          </div>
          <div class="hero-grid-select" style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-top: 15px;">
            <button class="hero-pick-btn" data-hero="ichigo">⚔️ Ichigo (Bleach)</button>
            <button class="hero-pick-btn" data-hero="saber">🛡️ Saber (Fate)</button>
            <button class="hero-pick-btn" data-hero="inuyasha">🐾 Inuyasha</button>
            <button class="hero-pick-btn" data-hero="rocklee">🍃 Rock Lee</button>
            <button class="hero-pick-btn" data-hero="ace">🔥 Portgas D. Ace</button>
            <button class="hero-pick-btn" data-hero="luffy">👒 Luffy (One Piece)</button>
          </div>
        </div>
      `;
      document.body.appendChild(modal);

      modal.querySelectorAll('.hero-pick-btn').forEach(b => {
        b.addEventListener('click', () => {
          const hero = b.getAttribute('data-hero');
          this.switchPlayerHero(hero);
          modal.style.display = 'none';
        });
      });
    }
    modal.style.display = modal.style.display === 'flex' ? 'none' : 'flex';
  }

  _toggleShop() {
    let modal = document.getElementById('shop-modal');
    if (!modal) {
      modal = document.createElement('div');
      modal.id = 'shop-modal';
      modal.className = 'game-modal';
      modal.innerHTML = `
        <div class="modal-card">
          <div class="modal-header">
            <h3>GOBLIN MERCHANT SHOP</h3>
            <button class="modal-close" onclick="document.getElementById('shop-modal').style.display='none'">✕</button>
          </div>
          <div style="display: flex; flex-direction: column; gap: 8px; margin-top: 15px;">
            <button class="shop-buy-btn" data-cost="100" data-item="potion">🧪 Healing Potion (100 G) -> Restore 350 HP</button>
            <button class="shop-buy-btn" data-cost="250" data-item="claw">⚔️ Claw of Attack (250 G) -> +25 Attack Damage</button>
            <button class="shop-buy-btn" data-cost="400" data-item="boots">👢 Boots of Speed (400 G) -> +60 Movement Speed</button>
          </div>
        </div>
      `;
      document.body.appendChild(modal);

      modal.querySelectorAll('.shop-buy-btn').forEach(b => {
        b.addEventListener('click', () => {
          const cost = parseInt(b.getAttribute('data-cost'), 10);
          const item = b.getAttribute('data-item');
          if (this.gold >= cost) {
            this.gold -= cost;
            if (item === 'potion') this.playerHero.health = Math.min(this.playerHero.maxHealth, this.playerHero.health + 350);
            if (item === 'claw') this.playerHero.attackDamage += 25;
            if (item === 'boots') this.playerHero.moveSpeed += 60;
            this.showToast(`Purchased item! Gold remaining: ${this.gold} G`, "#22c55e");
            this.audio.playImpact();
          } else {
            this.showToast("Not enough Gold!", "#ef4444");
          }
        });
      });
    }
    modal.style.display = modal.style.display === 'flex' ? 'none' : 'flex';
  }
}
