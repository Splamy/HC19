using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HC19
{
	internal class Program
	{
		private static string[] files = new[] { "a_example.txt", "b_lovely_landscapes.txt", "c_memorable_moments.txt", "d_pet_pictures.txt", "e_shiny_selfies.txt", };

		private static void Main(string[] args)
		{
			foreach (var file in files)
			{
				Data data = ProcessFile("../../../../Input/" + file);
				foreach (var imgs in data.Imgs.GroupBy(img => img.Tags.Length))
				{
					var ims = imgs.ToArray();
					for (int i = 0; i < ims.Length; i++)
					{
						if (i % 100 == 0)
							Console.WriteLine($"{i}/{ims.Length}");
						for (int j = i + 1; j < ims.Length; j++)
						{

						}
					}
				}
			}
		}

		private static Data ProcessFile(string file)
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
						if (!tags.TryGetValue(split[j + 2], out var itag))
						{
							itag = tags.Count;
							tags.Add(split[j + 2], itag);
						}
						itags[j] = itag;
					}

					Array.Sort(itags);
					data.Add(new Img(i, split[0] == "H", itags));
				}

				return new Data() { Imgs = data };
			}
		}
	}

	internal class Data
	{
		public List<Img> Imgs;
	}

	public class Img
	{
		public bool H;
		public int[] Tags;
		public int Id;

		public Img(int id, bool h, int[] tags)
		{
			H = h;
			Tags = tags;
			Id = id;
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
			var res = new List<int>();
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

	internal static class Util
	{
		public static Data UnionVerticalImages(Data images)
		{
			// filter out vertical images
			var imgs = images.Imgs.Where(img => !img.H).ToList();
			imgs.Sort((img1, img2) => img1.Tags.Length - img2.Tags.Length);

			Data unionedImages = new Data
			{
				Imgs = new List<Img>()
			};

			while (imgs.Count > 1)
			{
				Img img1 = imgs[0];
				
			}

			return unionedImages;
		}
	}

	public class DoubleImg : Img
	{
		public int SecondId;

		public DoubleImg(int id, bool h, int[] tags) : base(id, h, tags)
		{
			SecondId = 0;
		}

		public DoubleImg(Img firstImage, Img secondImage) : base(firstImage.Id, false, TagUnion(firstImage.Tags, secondImage.Tags))
		{
			SecondId = secondImage.Id;
		}
	}
}
