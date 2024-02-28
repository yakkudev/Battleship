﻿using System;
using System.Collections.Generic;

namespace Battleship {
	internal class Ship {
		List<Cell> cells = new List<Cell>();
		public Vec origin;
		public Vec direction = Vec.Right;

		public int Size {
			get { return cells.Count; }
			private set { }
		}

		public bool IsSunken {
			get { return Size > 0; }
			private set { }
		}


		public Ship(Vec origin, int size, Vec direction) {
			// cells = new List<Cell>(size); // doesn't work??
			for (int i = 0; i < size; i++) {
				cells.Add(new Cell());
			}
			this.origin = origin;
			this.direction = direction;
		}

		public Cell GetCell(int index) {
			if (index < 0 || index >= cells.Count) {
				return null;
			}
			return cells[index];
		}

		public bool ContainsPos(Vec pos) {
			Vec diff = pos - origin;
			// Check if X or Y is the same
			if (diff.x != 0 && diff.y != 0) return false;
			/*
			 * If distance between points is greater after a step in ship's orientation,
			 * it means that ship absolutely cannot contain pos 
			 */
			if (Vec.Distance(origin + direction, pos) > Vec.Distance(origin, pos)) return false;
			// Cast to int is required due to double imprecision
			if ((int)Vec.Distance(origin, pos) > cells.Count - 1) return false;
			return true;
		}

		public Cell TryGetCellAt(Vec pos) {
			if (!ContainsPos(pos)) return null;
			for (int i = 0; i < cells.Count; i++) {
				if (pos == origin + (direction * i)) return cells[i];
			}
			return null;
		}

		public List<Vec> GetCellPositions() {
            List<Vec> positions = new List<Vec>();
            for (int i = 0; i < cells.Count; i++) {
				positions.Add(origin + (direction * i));
			}
			return positions;
		}
	}
}
