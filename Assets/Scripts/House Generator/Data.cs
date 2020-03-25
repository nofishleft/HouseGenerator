using System;
using System.Collections.Generic;
using Vector3Int = UnityEngine.Vector3Int;
using Random = System.Random;

public class HouseGenerator
{
	const int MaxRoomArea = 80;
	const int MinSplittableArea = 32;
	const float MaxHallRate = 0.15f;

	Random rand;

	Rect House;
	int TotalHallArea = 0;
	Queue<Rect> Chunks, Halls, Blocks, Rooms;

	public HouseGenerator(int x, int y)
	{
		rand = new Random();
		Rect.rand = rand;
		House = new Rect(x, y);
		Chunks = new Queue<Rect>();
		Halls = new Queue<Rect>();
		Blocks = new Queue<Rect>();
		Rooms = new Queue<Rect>();
		direction = CoinFlip();
	}

	public (Queue<Rect>, Queue<Rect>, Rect) Drawables()
	{
		return (Rooms, Halls, House);
	}

	public void Generate()
	{
		ChunksToBlocks();
		BlocksToRooms();

			// where hall faces much older hall:
				// place wall;
			
			// put every room in queue of unreachable rooms;
			// while this queue is not empty:
				// get next room from queue;
				// if room is touching any number of halls:
					// make door, facing any avaliable hall;
					// put this room in queue of reachable rooms;
					// `continue`;
				// if room is touching any other reachable room:
					// connect this with other;
					// place door, if Random wants so;
					// `continue`;
				// put this room in queue of unreachable rooms;
			
			// place windows;
	}

	void ChunksToBlocks()
	{
		Chunks.Enqueue(House);

		while ((Chunks.Count > 0) && ((float)TotalHallArea / House.Area < MaxHallRate))
		{
			Rect chunk = Chunks.Dequeue();

			if (chunk.Area > MinSplittableArea && chunk.CanSubdivide_WithHall())
				SplitChunk(chunk);
			else Blocks.Enqueue(chunk);
		}

		// If exited loop due to reaching max hall area,
		// we have some areas still in the Chunks queue,
		// that need to be moved to the blocks queue
		while (Chunks.Count > 0)
		{
			Blocks.Enqueue(Chunks.Dequeue());
		}

		Chunks = null;
	}

	void SplitChunk(Rect chunk)
	{
		Rect hall;
		Rect chunk_a, chunk_b;
		HallChunkSplit(chunk, out chunk_a, out hall, out chunk_b);

		if (CoinFlip())
		{
			Chunks.Enqueue(chunk_a);
			Chunks.Enqueue(chunk_b);
		}
		else
		{
			Chunks.Enqueue(chunk_b);
			Chunks.Enqueue(chunk_a);
		}
		
		Halls.Enqueue(hall);
		
		TotalHallArea += hall.Area;
	}

	void Shuffle(Queue<Rect> Q)
	{
		List<Rect> l = new List<Rect>();

		l.AddRange(Q);

		Q.Clear();

		Shuffle(l);

		foreach (var r in l)
			Q.Enqueue(r);
	}

	void Shuffle(List<Rect> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rand.Next(n + 1);
			Rect value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	void BlocksToRooms()
	{
		while (Blocks.Count > 0)
		{

			Rect block = Blocks.Dequeue();

			if (WantSplitBlock(block))
			{
				Rect block_a, block_b;
				RandomBlockSplit(block, out block_a, out block_b);

				if (CoinFlip())
				{
					Blocks.Enqueue(block_a);
					Blocks.Enqueue(block_b);
				}
				else
				{
					Blocks.Enqueue(block_b);
					Blocks.Enqueue(block_a);
				}
			}
			else
				Rooms.Enqueue(block);
		}

		Blocks = null;
	}

	bool CanHallSplit()
	{
		return false;
	}

	void HallChunkSplit(Rect chunk, out Rect chunk_a, out Rect hall, out Rect chunk_b)
	{
		if (chunk.CanSubdivideHorizontally_WithHall() && chunk.CanSubdivideVertically_WithHall())
			if (CoinFlip())
				chunk.SubdivideHorizontally(out chunk_a, out hall, out chunk_b);
			else
				chunk.SubdivideVertically(out chunk_a, out hall, out chunk_b);

		else if (chunk.CanSubdivideHorizontally_WithHall())
			chunk.SubdivideHorizontally(out chunk_a, out hall, out chunk_b);

		else if (chunk.CanSubdivideVertically_WithHall())
			chunk.SubdivideVertically(out chunk_a, out hall, out chunk_b);
		else
			throw new Exception($"Can't subdivide chunk with hall: w-{chunk.w}, h-{chunk.h}.");
	}

	bool CoinFlip()
	{
		return rand.NextDouble() < 0.5d;
	}

	bool Roll(double probability)
	{
		return rand.NextDouble() < probability;
	}

	private bool direction = false;

	bool WantSplitBlock(Rect block)
	{
		return block.CanSubdivide() && (block.Area > MaxRoomArea || Roll(0.90d));
	}
	void RandomBlockSplit(Rect block, out Rect block_a, out Rect block_b)
	{
		// Rotate split direction
		direction = !direction;

		if (block.CanSubdivideHorizontally() && block.CanSubdivideVertically())
			if (direction)
				block.SubdivideHorizontally(out block_a, out block_b);
			else
				block.SubdivideVertically(out block_a, out block_b);

		else if (block.CanSubdivideHorizontally())
		{
			direction = true;
			block.SubdivideHorizontally(out block_a, out block_b);
		}
		else if (block.CanSubdivideVertically())
		{
			direction = false;
			block.SubdivideVertically(out block_a, out block_b);
		}
		else
			throw new Exception($"Can't subdivide block: w-{block.w}, h-{block.h}.");
	}
}

public struct Rect
{
	public static Random rand;

	public int l, r, t, b;

	public int w { get { return r - l + 1; } }
	public int h { get { return b - t + 1; } }

	public Rect(int w, int h)
	{
		this.l = 0;
		this.t = 0;
		this.r = w - 1;
		this.b = h - 1;
	}

	public Rect(int l, int r, int t, int b)
	{
		this.l = l;
		this.t = t;
		this.r = r;
		this.b = b;
	}

	public int Area { get { return w * h; } }

	public bool CanSubdivide()
	{
		return CanSubdivideHorizontally() || CanSubdivideVertically();
	}

	public bool CanSubdivide_WithHall()
	{
		return CanSubdivideHorizontally_WithHall() || CanSubdivideVertically_WithHall();
	}

	public bool CanSubdivideHorizontally()
	{
		return (w - 14 >= 0 && ((float) w / h > 1.5f || w > 16));
	}

	public bool CanSubdivideHorizontally_WithHall()
	{
		return (w - 16 >= 0);
	}

	public void SubdivideHorizontally(out Rect r_a, out Rect hall, out Rect r_b)
	{
		int available_width = (w - 16);

		int hallpos = UnityEngine.Random.Range(available_width, 0);

		r_a = new Rect(l, l + hallpos + 6, t, b);

		r_b = new Rect(r_a.r + 3, r, t, b);

		hall = new Rect(r_a.r + 1, r_a.r + 2, t, b);
	}

	public void SubdivideHorizontally(out Rect r_a, out Rect r_b)
	{
		int available_width = (w - 14);

		int pos = UnityEngine.Random.Range(available_width + 1, 0);

		r_a = new Rect(l, l + pos + 6, t, b);

		r_b = new Rect(r_a.r + 1, r, t, b);
	}

	public bool CanSubdivideVertically()
	{
		return (h - 14 >= 0 && ((float)h / w > 1.5f || h > 16));
	}

	public bool CanSubdivideVertically_WithHall()
	{
		return (h - 16 >= 0);
	}

	public void SubdivideVertically(out Rect r_a, out Rect hall, out Rect r_b)
	{
		int available_height = (h - 16);
		
		int hallpos = UnityEngine.Random.Range(available_height, 0);
		
		r_a = new Rect(l, r, t, t + hallpos + 6);
		
		r_b = new Rect(l, r, r_a.b + 3, b);
		
		hall = new Rect(l, r, r_a.b + 1, r_a.b + 2);
	}

	public void SubdivideVertically(out Rect r_a, out Rect r_b)
	{
		int available_height = (h - 14);
		
		int pos = UnityEngine.Random.Range(available_height + 1, 0);
		
		r_a = new Rect(l, r, t, t + pos + 6);

		r_b = new Rect(l, r, r_a.b + 1, b);
	}

	public List<Vector3Int> Edge()
	{
		List<Vector3Int> positions = new List<Vector3Int>();
		for (int x = l; x <= r; ++x)
		{
			positions.Add(new Vector3Int(x, t, 0));
			positions.Add(new Vector3Int(x, b, 0));
		}

		for (int y = t + 1; y <= b - 1; ++y)
		{
			positions.Add(new Vector3Int(l, y, 0));
			positions.Add(new Vector3Int(r, y, 0));
		}

		return positions;
	}
}