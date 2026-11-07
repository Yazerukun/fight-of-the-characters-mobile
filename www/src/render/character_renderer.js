/* ============================================================================
 * FULL-BODY ANIME CHARACTER RIG & COSTUME RENDERER
 * Location: src/render/character_renderer.js
 * ============================================================================ */

export class CharacterRenderer {
  constructor() {
    this.animTime = 0;
  }

  update(dt) {
    this.animTime += dt;
  }

  drawHero(ctx, hero, screenX, screenY, heroDef) {
    const isMoving = hero.vel && (hero.vel.lengthSq() > 10 || hero.isMoving);
    const walkCycle = isMoving ? Math.sin(this.animTime * 14) : 0;
    const breathe = Math.sin(this.animTime * 3) * 1.5;
    const facing = hero.facingAngle || 0;
    const heroId = (heroDef && heroDef.id) ? heroDef.id : 'ichigo';
    const isBuffed = hero.buffActive;

    ctx.save();
    ctx.translate(screenX, screenY);

    // 1. Soft Ground Contact Shadow
    ctx.save();
    ctx.fillStyle = 'rgba(0, 0, 0, 0.45)';
    ctx.beginPath();
    ctx.ellipse(0, 24, 20, 8, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // 2. Transformation Aura (Bankai, Super Saiyan, Gear Second, Kyuubi)
    if (isBuffed) {
      ctx.save();
      const auraPulse = Math.sin(this.animTime * 8) * 4;
      const gradient = ctx.createRadialGradient(0, 0, 10, 0, 0, 36 + auraPulse);
      gradient.addColorStop(0, heroDef.buffColor || '#dc2626');
      gradient.addColorStop(0.7, 'rgba(220, 38, 38, 0.25)');
      gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.fillStyle = gradient;
      ctx.beginPath();
      ctx.arc(0, 0, 38 + auraPulse, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    }

    // 3. Rotate Character towards Facing Angle
    // In top-down 2.5D, we tilt slightly to show full body costume and rotate facing
    ctx.save();
    ctx.rotate(facing - Math.PI / 2);

    // DRAW LEGS / FEET (Animated walk cycle)
    this.drawLegs(ctx, heroId, walkCycle);

    // DRAW TORSO & COSTUME (Robes, Gi, Vests, Belts)
    this.drawCostumeTorso(ctx, heroId, isBuffed, breathe);

    // DRAW HEAD, HAIR & FACIAL ACCESSORIES
    this.drawHeadAndHair(ctx, heroId, isBuffed);

    // DRAW WEAPONS (Zangetsu, Katana, Kunai, Excalibur)
    this.drawWeapons(ctx, heroId, isBuffed, walkCycle);

    ctx.restore(); // Restore rotation

    // 4. Overhead Level & Name Tag
    ctx.font = 'bold 12px -apple-system, sans-serif';
    ctx.textAlign = 'center';
    ctx.fillStyle = '#f8fafc';
    ctx.shadowColor = '#000';
    ctx.shadowBlur = 4;
    ctx.fillText(`${hero.name || 'Hero'} (Lv. ${hero.level || 1})`, 0, -38);

    // 5. HP & MP Overhead Gauges
    this.drawGauges(ctx, hero);

    ctx.restore(); // Restore translation
  }

  drawLegs(ctx, heroId, walkCycle) {
    const leftLegOffset = walkCycle * 6;
    const rightLegOffset = -walkCycle * 6;

    let pantColor = '#0f172a'; // Black default
    let shoeColor = '#e2e8f0'; // White socks default

    if (heroId === 'naruto') {
      pantColor = '#ea580c'; // Orange pants
      shoeColor = '#1e3a8a'; // Blue ninja sandals
    } else if (heroId === 'luffy') {
      pantColor = '#2563eb'; // Blue denim shorts
      shoeColor = '#d97706'; // Straw sandals
    } else if (heroId === 'goku') {
      pantColor = '#ea580c'; // Orange Gi pants
      shoeColor = '#1d4ed8'; // Blue boots with red trim
    } else if (heroId === 'zoro') {
      pantColor = '#064e3b'; // Dark green samurai trousers
      shoeColor = '#022c22';
    } else if (heroId === 'saber') {
      pantColor = '#1e40af'; // Blue royal skirt
      shoeColor = '#94a3b8'; // Silver armor boots
    }

    // Left Foot
    ctx.fillStyle = pantColor;
    ctx.fillRect(-10, 10 + leftLegOffset, 7, 14);
    ctx.fillStyle = shoeColor;
    ctx.fillRect(-10, 24 + leftLegOffset, 7, 5);

    // Right Foot
    ctx.fillStyle = pantColor;
    ctx.fillRect(3, 10 + rightLegOffset, 7, 14);
    ctx.fillStyle = shoeColor;
    ctx.fillRect(3, 24 + rightLegOffset, 7, 5);
  }

  drawCostumeTorso(ctx, heroId, isBuffed, breathe) {
    const torsoW = 20;
    const torsoH = 22 + breathe;

    ctx.save();

    if (heroId === 'ichigo') {
      // Black Shinigami Shihakusho Robe
      ctx.fillStyle = isBuffed ? '#18181b' : '#09090b';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // White Obi Belt
      ctx.fillStyle = isBuffed ? '#dc2626' : '#f8fafc';
      ctx.fillRect(-torsoW / 2, 2, torsoW, 4);

      // White Inner Collar (V-neck)
      ctx.fillStyle = '#f8fafc';
      ctx.beginPath();
      ctx.moveTo(-4, -10);
      ctx.lineTo(0, -4);
      ctx.lineTo(4, -10);
      ctx.fill();

      // Flowing Bankai Coat Tails
      if (isBuffed) {
        ctx.fillStyle = '#000000';
        ctx.beginPath();
        ctx.moveTo(-torsoW / 2, 10);
        ctx.lineTo(-torsoW / 2 - 6, 26);
        ctx.lineTo(-torsoW / 2 + 2, 14);
        ctx.fill();

        ctx.beginPath();
        ctx.moveTo(torsoW / 2, 10);
        ctx.lineTo(torsoW / 2 + 6, 26);
        ctx.lineTo(torsoW / 2 - 2, 14);
        ctx.fill();
      }
    } else if (heroId === 'naruto') {
      // Orange & Black Ninja Jumpsuit
      ctx.fillStyle = '#ea580c';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // Black Shoulder Armor / Yoke
      ctx.fillStyle = '#18181b';
      ctx.fillRect(-torsoW / 2, -10, torsoW, 7);

      // White Collar
      ctx.fillStyle = '#f8fafc';
      ctx.fillRect(-6, -11, 12, 3);

      // Red Uzumaki Swirl on Back/Chest
      ctx.strokeStyle = '#dc2626';
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(0, 0, 4, 0, Math.PI * 2);
      ctx.stroke();
    } else if (heroId === 'luffy') {
      // Red Sleeveless Vest (Open Chest)
      ctx.fillStyle = '#dc2626';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // Skin-toned Open Chest
      ctx.fillStyle = '#fed7aa';
      ctx.beginPath();
      ctx.moveTo(-4, -10);
      ctx.lineTo(0, 4);
      ctx.lineTo(4, -10);
      ctx.fill();

      // Yellow Sash Belt
      ctx.fillStyle = '#eab308';
      ctx.fillRect(-torsoW / 2, 6, torsoW, 4);

      // X-shaped Chest Scar
      ctx.strokeStyle = '#b91c1c';
      ctx.lineWidth = 1.5;
      ctx.beginPath();
      ctx.moveTo(-3, -4); ctx.lineTo(3, 2);
      ctx.moveTo(3, -4); ctx.lineTo(-3, 2);
      ctx.stroke();
    } else if (heroId === 'goku') {
      // Orange Martial Arts Gi
      ctx.fillStyle = isBuffed ? '#f59e0b' : '#ea580c';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // Blue Undershirt V-neck
      ctx.fillStyle = '#1d4ed8';
      ctx.beginPath();
      ctx.moveTo(-5, -10);
      ctx.lineTo(0, -3);
      ctx.lineTo(5, -10);
      ctx.fill();

      // Blue Martial Arts Sash / Belt
      ctx.fillStyle = '#1d4ed8';
      ctx.fillRect(-torsoW / 2, 4, torsoW, 4);

      // Kame Symbol Circle on Back
      ctx.fillStyle = '#ffffff';
      ctx.beginPath();
      ctx.arc(0, -2, 4, 0, Math.PI * 2);
      ctx.fill();
    } else if (heroId === 'zoro') {
      // Dark Green Samurai Longcoat with Green Haramaki Sash
      ctx.fillStyle = '#064e3b';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // Bright Green Haramaki Bellyband
      ctx.fillStyle = '#10b981';
      ctx.fillRect(-torsoW / 2, 0, torsoW, 8);

      // Open Muscular Chest
      ctx.fillStyle = '#fed7aa';
      ctx.beginPath();
      ctx.moveTo(-3, -10);
      ctx.lineTo(0, -1);
      ctx.lineTo(3, -10);
      ctx.fill();
    } else if (heroId === 'saber') {
      // Royal Blue Gown with Silver Metal Armor
      ctx.fillStyle = '#1e40af';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);

      // Silver Chestplate
      ctx.fillStyle = '#cbd5e1';
      ctx.fillRect(-8, -9, 16, 12);
      ctx.fillStyle = '#f8fafc';
      ctx.fillRect(-5, -7, 10, 8);

      // Gold Trim
      ctx.strokeStyle = '#facc15';
      ctx.lineWidth = 1.5;
      ctx.strokeRect(-8, -9, 16, 12);
    } else {
      // Generic Anime Battle Armor
      ctx.fillStyle = '#1e293b';
      ctx.fillRect(-torsoW / 2, -10, torsoW, torsoH);
      ctx.fillStyle = '#38bdf8';
      ctx.fillRect(-6, -6, 12, 10);
    }

    ctx.restore();
  }

  drawHeadAndHair(ctx, heroId, isBuffed) {
    const headRadius = 9;
    const headY = -18;

    ctx.save();

    // 1. Face / Skin Tone
    ctx.fillStyle = '#fed7aa';
    ctx.beginPath();
    ctx.arc(0, headY, headRadius, 0, Math.PI * 2);
    ctx.fill();

    // 2. Eyes
    ctx.fillStyle = '#09090b';
    ctx.fillRect(-4, headY - 1, 2.5, 2.5);
    ctx.fillRect(1.5, headY - 1, 2.5, 2.5);

    // 3. Hair & Headwear
    if (heroId === 'ichigo') {
      // Bright Spiky Orange Anime Hair
      ctx.fillStyle = '#f97316';
      // Spikes
      this.drawSpikyHair(ctx, headY, '#f97316');

      // Hollow Mask Horn / Half-Mask (If Buffed)
      if (isBuffed) {
        ctx.fillStyle = '#f8fafc';
        ctx.beginPath();
        ctx.arc(-2, headY - 3, 5, 0, Math.PI);
        ctx.fill();
        ctx.fillStyle = '#dc2626';
        ctx.fillRect(-4, headY - 5, 2, 4);
      }
    } else if (heroId === 'naruto') {
      // Yellow Spiky Hair
      this.drawSpikyHair(ctx, headY, '#facc15');

      // Konoha Forehead Protector (Hitai-ate)
      ctx.fillStyle = '#1e3a8a';
      ctx.fillRect(-9, headY - 5, 18, 4);
      // Metal Leaf Plate
      ctx.fillStyle = '#cbd5e1';
      ctx.fillRect(-4, headY - 5, 8, 3.5);
    } else if (heroId === 'luffy') {
      // Black Messy Hair
      ctx.fillStyle = '#18181b';
      ctx.beginPath();
      ctx.arc(0, headY - 3, headRadius + 1, Math.PI, Math.PI * 2);
      ctx.fill();

      // Iconic Yellow Straw Hat with Red Ribbon
      ctx.fillStyle = '#facc15';
      ctx.beginPath();
      ctx.ellipse(0, headY - 6, 15, 6, 0, 0, Math.PI * 2);
      ctx.fill();
      // Hat Dome
      ctx.beginPath();
      ctx.arc(0, headY - 9, 6, Math.PI, Math.PI * 2);
      ctx.fill();
      // Red Ribbon Band
      ctx.fillStyle = '#dc2626';
      ctx.fillRect(-6, headY - 9, 12, 2.5);
    } else if (heroId === 'goku') {
      // Goku Iconic Multi-pointed Hair
      const hairColor = isBuffed ? '#facc15' : '#18181b'; // Super Saiyan Golden if buffed!
      ctx.fillStyle = hairColor;
      
      // Giant characteristic left/right spikes
      ctx.beginPath();
      ctx.moveTo(-11, headY - 1);
      ctx.lineTo(-18, headY - 14);
      ctx.lineTo(-7, headY - 10);
      ctx.lineTo(-2, headY - 20);
      ctx.lineTo(3, headY - 10);
      ctx.lineTo(12, headY - 16);
      ctx.lineTo(9, headY - 2);
      ctx.closePath();
      ctx.fill();
    } else if (heroId === 'zoro') {
      // Marimo Spiky Green Hair
      this.drawSpikyHair(ctx, headY, '#10b981');
    } else if (heroId === 'saber') {
      // Golden Blonde Braided Bun & Ahoge
      ctx.fillStyle = '#fde047';
      ctx.beginPath();
      ctx.arc(0, headY - 3, headRadius + 1, Math.PI, Math.PI * 2);
      ctx.fill();
      // Top Bun
      ctx.beginPath();
      ctx.arc(0, headY - 12, 4.5, 0, Math.PI * 2);
      ctx.fill();
      // Blue Ribbon
      ctx.fillStyle = '#2563eb';
      ctx.fillRect(-3, headY - 10, 6, 2);
    } else {
      this.drawSpikyHair(ctx, headY, '#64748b');
    }

    ctx.restore();
  }

  drawSpikyHair(ctx, headY, color) {
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(-10, headY - 1);
    ctx.lineTo(-14, headY - 10);
    ctx.lineTo(-7, headY - 8);
    ctx.lineTo(-3, headY - 15);
    ctx.lineTo(0, headY - 10);
    ctx.lineTo(5, headY - 14);
    ctx.lineTo(8, headY - 7);
    ctx.lineTo(12, headY - 9);
    ctx.lineTo(10, headY - 1);
    ctx.closePath();
    ctx.fill();
  }

  drawWeapons(ctx, heroId, isBuffed, walkCycle) {
    ctx.save();

    if (heroId === 'ichigo') {
      // Giant Zangetsu Cleaver (Shikai) or Pitch-Black Katana with Tsuba (Bankai)
      if (isBuffed) {
        // Tensa Zangetsu: Sleek Pitch Black Blade with Red Reiatsu Glow
        ctx.strokeStyle = '#000000';
        ctx.lineWidth = 3.5;
        ctx.shadowColor = '#dc2626';
        ctx.shadowBlur = 10;
        ctx.beginPath();
        ctx.moveTo(12, 0);
        ctx.lineTo(26, -26);
        ctx.stroke();

        // Chain at Hilt
        ctx.strokeStyle = '#cbd5e1';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.arc(11, 2, 3, 0, Math.PI);
        ctx.stroke();
      } else {
        // Shikai: Giant Cleaver Blade Wrapped in White Cloth
        ctx.fillStyle = '#94a3b8';
        ctx.beginPath();
        ctx.moveTo(12, 2);
        ctx.lineTo(28, -28);
        ctx.lineTo(34, -24);
        ctx.lineTo(16, 8);
        ctx.closePath();
        ctx.fill();

        // Black Blade Edge
        ctx.strokeStyle = '#0f172a';
        ctx.lineWidth = 2;
        ctx.stroke();
      }
    } else if (heroId === 'zoro') {
      // Three Katana Blades (One in Right Hand, Two Sheathed at Left Hip)
      // Main Blade in hand
      ctx.strokeStyle = '#e2e8f0';
      ctx.lineWidth = 2.5;
      ctx.beginPath();
      ctx.moveTo(10, 0);
      ctx.lineTo(24, -22);
      ctx.stroke();

      // Sheaths at Hip
      ctx.strokeStyle = '#b91c1c';
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.moveTo(-8, 6); ctx.lineTo(-18, 16);
      ctx.stroke();

      ctx.strokeStyle = '#18181b';
      ctx.beginPath();
      ctx.moveTo(-10, 8); ctx.lineTo(-20, 18);
      ctx.stroke();
    } else if (heroId === 'saber') {
      // Golden Holy Sword Excalibur
      ctx.strokeStyle = '#facc15';
      ctx.lineWidth = 3;
      ctx.shadowColor = '#fef08a';
      ctx.shadowBlur = 12;
      ctx.beginPath();
      ctx.moveTo(10, 0);
      ctx.lineTo(26, -24);
      ctx.stroke();

      // Crossguard
      ctx.strokeStyle = '#3b82f6';
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.moveTo(7, -3); ctx.lineTo(13, 3);
      ctx.stroke();
    } else if (heroId === 'naruto') {
      // Swirling Blue Chakra Rasengan in Hand
      ctx.save();
      ctx.fillStyle = '#38bdf8';
      ctx.shadowColor = '#67e8f9';
      ctx.shadowBlur = 12;
      ctx.beginPath();
      ctx.arc(14, -6, 6, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    } else if (heroId === 'luffy') {
      // Steaming Red Overheated Rubber Fists (Gear Second)
      if (isBuffed) {
        ctx.fillStyle = '#f87171';
        ctx.shadowColor = '#ef4444';
        ctx.shadowBlur = 8;
        ctx.beginPath();
        ctx.arc(14, -4, 5, 0, Math.PI * 2);
        ctx.arc(-14, -4, 5, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    ctx.restore();
  }

  drawGauges(ctx, hero) {
    const barW = 46;
    const barH = 5;
    const hpPercent = Math.max(0, hero.hp / hero.maxHp);

    // HP Bar
    ctx.fillStyle = 'rgba(0, 0, 0, 0.75)';
    ctx.fillRect(-barW / 2, -32, barW, barH);
    ctx.fillStyle = '#22c55e';
    ctx.fillRect(-barW / 2, -32, barW * hpPercent, barH);
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 1;
    ctx.strokeRect(-barW / 2, -32, barW, barH);

    // MP Bar
    const mpPercent = Math.max(0, hero.mp / hero.maxMp);
    ctx.fillStyle = '#0284c7';
    ctx.fillRect(-barW / 2, -26, barW * mpPercent, 3);
  }
}
