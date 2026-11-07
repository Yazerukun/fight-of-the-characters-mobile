/* ============================================================================
 * SPATIAL HASH GRID (BROADPHASE COLLISION FILTERING)
 * Location: src/core/spatial_grid.js
 * ============================================================================ */
export class SpatialGrid {
  constructor(width, height, cellSize) {
    this.cellSize = cellSize;
    this.cols = Math.ceil(width / cellSize);
    this.rows = Math.ceil(height / cellSize);
    this.cells = new Array(this.cols * this.rows).fill(null).map(() => []);
  }

  clear() {
    for (let i = 0; i < this.cells.length; i++) {
      this.cells[i].length = 0;
    }
  }

  insert(entity) {
    const col = Math.max(0, Math.min(this.cols - 1, Math.floor(entity.pos.x / this.cellSize)));
    const row = Math.max(0, Math.min(this.rows - 1, Math.floor(entity.pos.y / this.cellSize)));
    this.cells[row * this.cols + col].push(entity);
  }

  queryRange(pos, radius, filterFn) {
    const results = [];
    const minCol = Math.max(0, Math.floor((pos.x - radius) / this.cellSize));
    const maxCol = Math.min(this.cols - 1, Math.floor((pos.x + radius) / this.cellSize));
    const minRow = Math.max(0, Math.floor((pos.y - radius) / this.cellSize));
    const maxRow = Math.min(this.rows - 1, Math.floor((pos.y + radius) / this.cellSize));
    const radSq = radius * radius;

    for (let r = minRow; r <= maxRow; r++) {
      for (let c = minCol; c <= maxCol; c++) {
        const list = this.cells[r * this.cols + c];
        for (let i = 0; i < list.length; i++) {
          const ent = list[i];
          if (ent.dead) continue;
          const distSq = (ent.pos.x - pos.x) ** 2 + (ent.pos.y - pos.y) ** 2;
          if (distSq <= radSq && (!filterFn || filterFn(ent))) {
            results.push(ent);
          }
        }
      }
    }
    return results;
  }
}
