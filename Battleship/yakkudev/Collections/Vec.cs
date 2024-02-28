using System;

namespace yakkudev.Collections {
	public struct Vec {
		public int x, y;

		public Vec(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static readonly Vec Zero = new Vec(0, 0);

		public static readonly Vec Up = new Vec(0, -1);
		public static readonly Vec Down = new Vec(0, 1);
		public static readonly Vec Left = new Vec(-1, 0);
		public static readonly Vec Right = new Vec(1, 0);

		public static Vec RotateAA(Vec v, bool left = false) {
			if (left) {
				return new Vec(v.y, -v.x);
			}
			return -new Vec(v.y, -v.x);

		}

		public static Vec operator +(Vec a, Vec b) => new Vec(a.x + b.x, a.y + b.y);
		public static Vec operator -(Vec a, Vec b) => new Vec(a.x - b.x, a.y - b.y);
		public static Vec operator -(Vec a) => new Vec(-a.x, -a.y);
		public static Vec operator *(Vec a, int scale) => new Vec(a.x * scale, a.y * scale);
		public static Vec operator *(int scale, Vec a) => new Vec(a.x * scale, a.y * scale);
		public static bool operator ==(Vec a, Vec b) => (a.x == b.x) && (a.y == b.y);
		public static bool operator !=(Vec a, Vec b) => !(a == b);

		public static Vec Multiply(Vec l, Vec r) {
			// https://en.wikipedia.org/wiki/Hadamard_product_(matrices)
			return new Vec(l.x * r.x, l.y * r.y);
		}

		public static Vec Pow(Vec a, int pow = 2) {
			for (int i = 0; i < pow; i++) {
				a = Multiply(a, a);
			}
			return a;
		}

		public static double Distance(Vec a, Vec b) {
			// https://en.wikipedia.org/wiki/Euclidean_distance
			Vec c = Pow(a - b);
			return Math.Sqrt(c.x + c.y);
		}

		public override string ToString() {
			return $"({x};{y})";
		}
	}
}
