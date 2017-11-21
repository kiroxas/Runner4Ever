//using NSubstitute;
using System.Collections.Generic;
using NUnit.Framework;

namespace LevelGenerator.Tests
{
	public class FileReadingTest
	{
		[Test]
		public void ReadOneSimpleGlyph()
		{
			string myLine = "0";

			FileUtils.Glyph g = new FileUtils.Glyph(myLine);

			Assert.That(g.getMajor(), Is.EqualTo("0"));
			Assert.IsNull(g.getMinor());
		}

		[Test]
		public void ReadOneSimpleMultipleGlyph()
		{
			string myLine = "0.9";

			FileUtils.Glyph g = new FileUtils.Glyph();

			int split = g.split(myLine);

			Assert.That(split, Is.EqualTo(2));

			Assert.That(g.getMajor(), Is.EqualTo("0"));
			Assert.That(g.getMinor(), Is.EqualTo("9"));
		}

		[Test]
		public void ReadOneSimpleLine()
		{
			string myLine = "0 0 0 0 0 0";

			List<FileUtils.Glyph> myList = new List<FileUtils.Glyph>();

			int glyphs = FileUtils.FileSize.lineToGlyph(myLine, ref myList);

			Assert.That(glyphs, Is.EqualTo(6));

			foreach(FileUtils.Glyph g in myList)
			{
				Assert.That(g.getMajor(), Is.EqualTo("0"));
				Assert.IsNull(g.getMinor());
			}
		}

		public void testGlyph(FileUtils.Glyph g, string expectedMajor, string expectedMinor)
		{
			Assert.That(g.getMajor(), Is.EqualTo(expectedMajor));
			Assert.That(g.getMinor(), Is.EqualTo(expectedMinor));
		}

		[Test]
		public void ReadOneComplicatedLine()
		{
			string myLine = "2.5 achie.go   solo.yann                   a.r 0  2. 5.1";

			List<FileUtils.Glyph> myList = new List<FileUtils.Glyph>();

			int glyphs = FileUtils.FileSize.lineToGlyph(myLine, ref myList);

			Assert.That(glyphs, Is.EqualTo(7));

			testGlyph(myList[0], "2", "5");
			testGlyph(myList[1], "achie", "go");
			testGlyph(myList[2], "solo", "yann");
			testGlyph(myList[3], "a", "r");
			testGlyph(myList[4], "0", null);
			testGlyph(myList[5], "2", null);
			testGlyph(myList[6], "5", "1");
		}

		[Test]
		public void TestMultipleLines()
		{
			string[] myLines = {"2.5 achie.go   solo.yann                   a.r 0   " , " 0 2.5 6.4 8.3 8 " };

			List<FileUtils.Glyph> myList = FileUtils.FileSize.toChar(myLines);

			Assert.That(myList.Count, Is.EqualTo(10));
		}


		[Test]
		public void TestLineSplitting()
		{
			string myFile = "2.5 achie.go   solo.yann                   a.r 0  \n 0 2.5 6.4 8.3 8 \n \n 0 0 0 0 0";

			string[] lines = FileUtils.FileSize.splitLinesOfFile(myFile);

			Assert.That(lines.Length, Is.EqualTo(4));

			List<FileUtils.Glyph> myList = FileUtils.FileSize.toChar(lines);

			Assert.That(myList.Count, Is.EqualTo(15));
		}

		[Test]
		public void TestLevelLoading()
		{
			string myFile = @"0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.youhou 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.pouet 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
4 0 0 0 0 0 0 0 0 0 0 0 0 0 E E E E E E E E E 4 0 0 0 0 0 0 0 0 0 0 0 0 0 E E E E E E E E E
0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2
0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2
0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2
0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 2 2 2 2 2 2 2
H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H H
1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1";

			string[] lines = FileUtils.FileSize.splitLinesOfFile(myFile);

			Assert.That(lines.Length, Is.EqualTo(36));

			FileUtils.FileSize size = FileUtils.FileSize.getSize(lines);
			List<FileUtils.Glyph> myList = FileUtils.FileSize.toChar(lines);

			Assert.That(size.xSize, Is.EqualTo(46));
			Assert.That(size.ySize, Is.EqualTo(36));
			Assert.That(myList.Count, Is.EqualTo(1656));

			foreach(FileUtils.Glyph g in myList)
			{
				Assert.That(g.isEmpty(), Is.EqualTo(false));
			}
		}
	}
}