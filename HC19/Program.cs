using System;
using System.Collections.Generic;
using System.Globalization;
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
				Console.WriteLine("Doing: " + file);
				Data data = ProcessFile("../../../../Input/" + file);

				Do(data, file);

				WriteResult(data, file);
			}
		}

		public static void Do(Data d, string file)
		{
			Util.UnionVerticalImages(d, file);
			d.Imgs.Sort((i, j) => i.Tags.Length - j.Tags.Length);

			int score = 0;
			for (int i = 0; i < d.Imgs.Count - 1; i++)
			{
				score += d.Imgs[i].PointsWith(d.Imgs[i + 1]);
			}
			Console.WriteLine($"Score: {score}");
		}

		private static void ArrangeSlides(Data d)
		{
			List<Img> slides = new List<Img>();
			var groupedImages = d.Imgs.GroupBy(img => img.Tags.Length).ToList();
			groupedImages.Sort((g1, g2) => g1.Key - g2.Key);

			Img leftFromLastGroup = null;			
			foreach (IGrouping<int, Img> imgGroup in groupedImages)
			{
				Img currentImg = imgGroup.First();
				// find image that has at least key / 2
				int bestKeyCount = 0;
				
			}			
		}

		private static void WriteResult(Data d, string file)
		{
			if (!Directory.Exists("../../../../Output/"))
				Directory.CreateDirectory("../../../../Output/");

			using (var fs = File.Open("../../../../Output/" + file, FileMode.Create, FileAccess.Write))
			{
				using (var s = new StreamWriter(fs))
				{
					s.WriteLine(d.Imgs.Count);
					foreach (Img i in d.Imgs)
					{
						s.WriteLine(i.WriteAsSlide());
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
		public int Id;
		public bool H;
		public int[] Tags;

		public Img(int id, bool h, int[] tags)
		{
			Id = id;
			H = h;
			Tags = tags;
		}

		public override string ToString() => (H ? "H" : "V") + " " + string.Join(",", Tags);

		public virtual string WriteAsSlide() => Id.ToString(CultureInfo.InvariantCulture);

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
			only1 += Tags.Length - i1;
			only2 += i.Tags.Length - i2;

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
					res.Add(b[i2]);
					i2++;
				}
			}

			return res.ToArray();
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

		public override string WriteAsSlide() => Id + " " + SecondId;
	}

	internal static class Util
	{
		public static void UnionVerticalImages(Data images, string file)
		{
			int min = 0;
			int max = 0;
			switch (file)
			{
			case "a_example.txt":
				min = 1;
				max = 3;
				break;
			case "b_lovely_landscapes.txt":
				min = 15;
				max = 30;
				break;
			case "c_memorable_moments.txt":
				min = 7;
				max = 14;
				break;
			case "d_pet_pictures.txt":
				min = 5;
				max = 17;
				break;
			case "e_shiny_selfies.txt":
				min = 13;
				max = 29;
				break;
			default: break;
			}

			// filter out vertical images
			var imgs = images.Imgs.Where(img => !img.H).ToList();
			imgs.Sort((img1, img2) => img1.Tags.Length - img2.Tags.Length);

			var imgsR = new List<Img>();

			while (imgs.Count > 1)
			{
				Img img1 = imgs[0];
				Img img2 = img1;
				for(int i = 1; i < imgs.Count; i++)
				{
					int tagSum = CalcTagSum(img1, imgs[i]);
					if(min <= tagSum && max >= tagSum)
					{
						img2 = imgs[i];
						break;
					}
				}
				
				DoubleImg di = new DoubleImg(img1, img2);
				imgs.Remove(img1);
				imgs.Remove(img2);
				imgsR.Add(di);
			}

			imgsR.AddRange(images.Imgs.Where(img => img.H));
			images.Imgs = imgsR;
		}

		public static int CalcTagSum(Img img1, Img img2)
		{
			var tags = 0;
			return 0;
		}
	}
}
