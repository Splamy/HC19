using System;
using System.Collections.Generic;
using System.IO;

namespace HC19
{
	class Program
	{
		static string[] files = new[] { "a_example.txt" };

		static void Main(string[] args)
		{
			foreach (var file in files)
			{
				ProcessFile( "../../../../Input/" + file);
			}
		}

		static Data ProcessFile(string file)
		{
			var tags = new Dictionary<string, int>();
			using (var fs = File.OpenRead(file))
			using (var s = new StreamReader(fs))
			{
				int num = int.Parse(s.ReadLine());
				var data = new List<Img>(num);
				for (int i = 0; i < num; i++)
				{
					var line = s.ReadLine();
					var split = line.Split();
					var itags = new int[int.Parse(split[1])];

					for (int j = 0; j < itags.Length; j++)
					{
						if(!tags.TryGetValue(split[j + 2], out var itag))
						{
							itag = tags.Count;
							tags.Add(split[j + 2], itag);
						}
						itags[j] = itag;
					}

					Array.Sort(itags);
					data.Add(new Img(split[0] == "H", itags));
				}

				return new Data() { Imgs = data };
			}
		}
	}

	class Data
	{
		public List<Img> Imgs;
	}

	public class Img
	{
		public bool H;
		public int[] Tags;

		public Img(bool h, int[] tags)
		{
			H = h;
			Tags = tags;
		}

		public override string ToString() => (H ? "H" : "V") + " " + string.Join(",", Tags);

		public int PointsWith(Img i)
		{
			int common = 0;
			int only1 = 0;
			int only2 = 0;

			int i1 = 0;
			int i2 = 0;
			while (i1 < Tags.Length && i2 < i.Tags.Length)
			{
				if (Tags[i1] == i.Tags[i2])
				{
					common++;
					i1++;
					i2++;
				}
				else if (Tags[i1] < i.Tags[i2])
				{
					only1++;
					i1++;
				}
				else// if (Tags[i1] > i.Tags[i2])
				{
					only2++;
					i2++;
				}
			}

			return Math.Min(Math.Min(common, only1), only2);
		}

		public static int[] TagUnion(int[] a, int[] b)
		{
			List<int> res = new List<int>();
			int i1 = 0;
			int i2 = 0;
			while (i1 < a.Length && i2 < b.Length)
			{
				if (a[i1] == b[i2])
				{
					res.Add(a[i1]);
					i1++;
					i2++;
				}
				else if (a[i1] < b[i2])
				{
					res.Add(a[i1]);
					i1++;
				}
				else
				{
					res.Add(a[i2]);
					i2++;
				}
			}

			return res.ToArray();
		}
	}
}
