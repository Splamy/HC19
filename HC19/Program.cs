using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HC19
{
	internal class Program
	{
		// "a_example.txt", "b_lovely_landscapes.txt", "c_memorable_moments.txt", "d_pet_pictures.txt", "e_shiny_selfies.txt"
		private static string[] files = new[] { "d_pet_pictures.txt", };

		private static void Main(string[] args)
		{
			foreach (var file in files)
			{
				Console.WriteLine("Doing: " + file);
				Data data = ProcessFile("../../../../Input/" + file);

				Bogo(data, file);

				WriteResult(data, file);
			}
		}

		public static void Do(Data d, string file)
		{
			Util.UnionVerticalImages(d, file);
			Console.WriteLine("Vertical images combined");
			ArrangeSlides(d);

			int score = 0;
			for (int i = 0; i < d.Imgs.Count - 1; i++)
			{
				score += d.Imgs[i].PointsWith(d.Imgs[i + 1]);
			}
			Console.WriteLine($"Score: {score}");
		}

		public static void Bogo(Data d, string file)
		{
			Util.UnionVerticalImages(d, file);
			Console.WriteLine("Vertical images combined");

			var score = new (int l, int r)[d.Imgs.Count];
			int scoreSum = 0;
			for (int i = 0; i < d.Imgs.Count - 1; i++)
			{
				var (a, b, c) = STrip(d, i);
				score[i] = STrip(a, b, c);
				scoreSum += score[i].l + score[i].r;
			}

			int mod = 0;

			var rnd = new Random();
			while (true)
			{
				int select = rnd.Next(0, d.Imgs.Count);
				int other;
				do
				{
					other = rnd.Next(0, d.Imgs.Count);
				} while (select == other);


				var (s1, s2, s3) = STrip(d, select);
				var (o1, o2, o3) = STrip(d, other);

				var sn = STrip(s1, o2, s3);
				var on = STrip(o1, s2, o3);
				var swap = sn.l + sn.r + on.l + on.r;
				var cur = score[select].l + score[select].r + score[other].l + score[other].r;
				if (swap > cur)
				{
					var tmp = d.Imgs[select];
					d.Imgs[select] = d.Imgs[other];
					d.Imgs[other] = tmp;

					Set(score, select, sn);
					Set(score, other, on);

					scoreSum -= cur;
					scoreSum += swap;
				}

				if (++mod % 10000 == 0)
				{
					Console.WriteLine("Score:" + scoreSum);
					mod = 0;
					Console.WriteLine("Write");
					WriteResult(d, file);
				}
			}
		}

		public static void Set((int l, int r)[] score, int i, (int l, int r) val)
		{
			score[i] = val;
			if (i > 0)
			{
				score[i - 1] = (score[i - 1].l, val.l);
			}
			if (i < score.Length - 1)
			{
				score[i + 1] = (val.r, score[i + 1].l);
			}
		}

		public static (Img, Img, Img) STrip(Data d, int i)
		{
			if (i == 0)
				return (null, d.Imgs[i], d.Imgs[i + 1]);
			else if (i == d.Imgs.Count - 1)
				return (d.Imgs[i - 1], d.Imgs[i], null);
			return (d.Imgs[i - 1], d.Imgs[i], d.Imgs[i + 1]);
		}

		public static (int l, int r) STrip(Img a, Img b, Img c)
		{
			int l = 0, r = 0;
			if (a != null)
			{
				l = a.PointsWith(b);
			}
			if (c != null)
			{
				r = b.PointsWith(c);
			}
			return (l, r);
		}

		private static void ArrangeSlides(Data d)
		{
			List<Img> slides = new List<Img>();
			var groupedImages = d.Imgs.GroupBy(img => img.Tags.Count).ToList();
			groupedImages.Sort((g1, g2) => g1.Key - g2.Key);

			Img leftFromLastGroup = null;
			var groupedList = groupedImages.Select(x => (x.Key, x.ToList())).ToArray();
			foreach (var (key, imgGroup) in groupedList)
			{
				Img currentImg = null;
				if (leftFromLastGroup == null)
				{
					currentImg = imgGroup.Last();
					imgGroup.RemoveAt(imgGroup.Count - 1);
				}
				else
				{
					currentImg = leftFromLastGroup;
				}

				while (imgGroup.Count > 1)
				{
					// find image that has at least key / 2
					int bestKeyPoints = 0;
					int bestImageIndex = 0;
					Img bestImage = null;
					foreach (var (i, otherImg) in imgGroup.Select((i, j) => (j, i)))
					{
						int points = currentImg.PointsWith(otherImg);
						if (points >= bestKeyPoints)
						{
							bestKeyPoints = points;
							bestImageIndex = i;
							bestImage = otherImg;
						}
					}

					slides.Add(currentImg);
					imgGroup.RemoveAt(bestImageIndex);
					currentImg = bestImage;
				}

				// only one image from group should be left here
				if (imgGroup.Count == 1)
				{
					leftFromLastGroup = imgGroup[0];
				}
				else
				{
					leftFromLastGroup = currentImg;
				}
			}

			if (leftFromLastGroup != null)
			{
				slides.Add(leftFromLastGroup);
			}
			d.Imgs = slides;
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
					data.Add(new Img(i, split[0] == "H", new HashSet<int>(itags)));
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
		public HashSet<int> Tags;

		public Img(int id, bool h, HashSet<int> tags)
		{
			Id = id;
			H = h;
			Tags = tags;
		}

		public override string ToString() => (H ? "H" : "V") + " " + string.Join(",", Tags);

		public virtual string WriteAsSlide() => Id.ToString(CultureInfo.InvariantCulture);

		public int PointsWith(Img i)
		{
			var intersection = Tags.Intersect(i.Tags).Count();

			int common = intersection;
			int only1 = Tags.Count - common;
			int only2 = i.Tags.Count - common;

			return Math.Min(Math.Min(common, only1), only2);
		}
	}

	public class DoubleImg : Img
	{
		public int SecondId;

		public DoubleImg(int id, bool h, HashSet<int> tags) : base(id, h, tags)
		{
			SecondId = 0;
		}

		public DoubleImg(Img firstImage, Img secondImage) : base(firstImage.Id, false,
			firstImage.Tags.Union(secondImage.Tags).ToHashSet())
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
			imgs.Sort((img1, img2) => img1.Tags.Count - img2.Tags.Count);

			var imgsR = new List<Img>();

			while (imgs.Count > 1)
			{
				Img img1 = imgs[0];
				Img img2 = imgs[1];
				/*
				for (int i = imgs.Count - 1; i >= 1; i--)
				{
					int tagSum = CalcTagSum(img1, imgs[i]);
					if (min <= tagSum && max >= tagSum)
					{
						img2 = imgs[i];
						break;
					}
				}*/

				var di = new DoubleImg(img1, img2);
				imgs.Remove(img1);
				imgs.Remove(img2);
				imgsR.Add(di);
			}

			imgsR.AddRange(images.Imgs.Where(img => img.H));
			images.Imgs = imgsR;
		}

		public static int CalcTagSum(Img img1, Img img2)
		{
			return img1.Tags.Union(img2.Tags).Count();
		}
	}
}