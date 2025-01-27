﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HC19
{
	internal class Program
	{
		private static string[] files = new[] { "a_example.txt", "c_memorable_moments.txt", "d_pet_pictures.txt", "e_shiny_selfies.txt", };

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
			Console.WriteLine("Vertical images combined");
			ArrangeSlides(d);
			Console.WriteLine("Slides Arranged");

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
			var groupedImages = d.Imgs.GroupBy(img => img.Tags.Count).ToList();
			groupedImages.Sort((g1, g2) => g2.Key - g1.Key);

			Img leftFromLastGroup = null;
			var groupedList = groupedImages.Select(x => (x.Key, x.ToList())).ToArray();
			foreach (var (key, imgGroup) in groupedList)
			{
				Console.WriteLine("Arranging Group with #tags = " + key + ", Image Count: " + imgGroup.Count);
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

						if (bestKeyPoints >= (key / 2) || i > 200)
						{
							break;
						}
					}

					slides.Add(currentImg);
					imgGroup.RemoveAt(bestImageIndex);
					currentImg = bestImage;
					if (imgGroup.Count % 100 == 0)
					{
						Console.WriteLine("Found partner for image, left: " + imgGroup.Count);
					}
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
				int bestI = imgs.Count - 1;
				int bestCount = CalcTagSum(img1, imgs[bestI]);
				int tagCount = imgs[bestI].Tags.Count;
				for (int i = bestI - 1; i > 0; i--)
				{
					var img = imgs[i];
					if (img.Tags.Count != tagCount)
						break;
					var count = CalcTagSum(img1, img);
					if (count > bestCount)
					{
						bestCount = count;
						bestI = i;
					}
				}
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

				var di = new DoubleImg(img1, imgs[bestI]);
				imgs.RemoveAt(bestI);
				imgs.RemoveAt(0);
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