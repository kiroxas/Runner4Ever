using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace FileUtils
{
	public class Glyph 
	{
		/* major.minor.info */

		public string major;
		public string minor;
		public string info;

		static public char[] splitPattern = new char[] { '.'};

		public int split(string glyph)
		{
			if(String.IsNullOrEmpty(glyph) || FileSize.isOnlyWhiteSpace(glyph))
			{
				Debug.LogError("Empty Glyph !");
			}

			string[] lines =  glyph.Split(splitPattern, StringSplitOptions.RemoveEmptyEntries);

			if(lines.Length > 0)
			{
				major = lines[0];
			}
			
			if (lines.Length > 1 && String.IsNullOrEmpty(lines[1]) == false)
			{
				minor = lines[1];
			}

			if (lines.Length > 2 && String.IsNullOrEmpty(lines[2]) == false)
			{
				info = lines[2];
			}
			
			return lines.Length;
		}

		public Glyph()
		{}

		public Glyph(string glyph)
		{
			split(glyph);
		}

		public string getMajor()
		{
			return major;
		}

		public string getMinor()
		{
			return minor;
		}

		public string getInfo()
		{
			return info;
		}

		public bool hasAdditionalInfo()
		{
			return String.IsNullOrEmpty(minor) == false || String.IsNullOrEmpty(info) == false;
		}

		public string getFull()
		{
			return major + (String.IsNullOrEmpty(minor) ? "" : (splitPattern + minor)) + (String.IsNullOrEmpty(info) ? "" : (splitPattern + info));
		}

		public bool isEmpty()
		{
			return String.IsNullOrEmpty(major) && String.IsNullOrEmpty(minor) &&  String.IsNullOrEmpty(info);
		}
	}

	public class FileSize
	{
		public int xSize;
		public int ySize;

		static public char[] whitespace = new char[] { ' ', '\t' };

		public static bool isOnlyWhiteSpace(string s)
		{
			return s.All( c => whitespace.Contains(c));
		}

		public FileSize(int x, int y)
		{
			xSize = x;
			ySize = y;
		}

		public FileSize()
		{
			xSize = 0;
			ySize = 0;
		}

		public bool empty()
		{
			return xSize * ySize == 0;
		}

		static public FileSize getSize(string[] lines)
		{
			int xSize = -1;
			int ySize = 0;

			foreach(string s in lines)
			{
				if(String.IsNullOrEmpty(s) || isOnlyWhiteSpace(s))
					continue;

				string[] line = splitLine(s); 
				int xTemp = line.Length;
				ySize++;

				if(xSize == -1)
				{
					xSize = xTemp;
				}
				else
				{
					if(xSize != xTemp)
					{
						Debug.LogError("The size of lines should be equal xSize : " + xSize + " new : " + xTemp + " (happened at y : " + ySize + ')' + " with line :" + s);
					}
				}
			}

			return new FileSize(xSize, ySize);
		}

		static public string[] splitLine(string line)
		{
			return  line.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
		}

		static public int lineToGlyph(string line, ref List<Glyph> list)
		{
			int glyphCreated = 0;
			string[] blocks = splitLine(line);

			foreach(string b in blocks)
			{
				if(String.IsNullOrEmpty(b) || isOnlyWhiteSpace(b))
					continue;

				list.Add(new Glyph(b));
				++glyphCreated;
			}

			return glyphCreated;
		}

		static public List<Glyph> toChar(string[] lines)
		{
			List<Glyph> list = new List<Glyph>();
		
			foreach(string line in lines)
			{
				lineToGlyph(line, ref list);
			}
			
			return list;
		}

		static public string[] splitLinesOfFile(string file)
		{
			return System.Text.RegularExpressions.Regex.Split ( file, "\r\n|\n|\r" );
		}

		public static string[] load(string path)
		{
			TextAsset data = Resources.Load(path) as TextAsset;
			string fs = data.text;
  			
  			return splitLinesOfFile(fs);
		}
	}

	[XmlRoot(ElementName="FileList")]
	public class FileList
	{
		[XmlArray("files")]
		[XmlArrayItem("file")]
		public List<string> files{ get; set;}

		public int index = 0;
		
		public List<FileSize> sizes{ get; set;}
		public List<List<FileUtils.Glyph>> loaded{ get; set;}
		
		public List<FileUtils.Glyph> getOneRandom()
		{
			index = Random.Range(0, loaded.Count);
			return loaded[index];
		}

		public FileSize getSize()
		{
			return sizes[index];
		}

		public List<FileUtils.Glyph> getNextOne()
		{
			int ind = index;

			++index;
			if(index >= loaded.Count)
				index =0;

			return loaded[ind];	
		}

		public int filesNumber()
		{
			return loaded.Count;
		}

		public static FileUtils.FileList loadFrom(string folder, string path)
		{
        	TextAsset data = Resources.Load(folder + path) as TextAsset;

        	if(data == null)
        	{
        		Debug.LogError("Couldn't load file : " + path);
        	}

			TextReader sr = new StringReader(data.text);

			XmlSerializer serial = new XmlSerializer(typeof(FileUtils.FileList));
			FileUtils.FileList list = (FileUtils.FileList)serial.Deserialize(sr);

			foreach(string f in list.files)
			{
				string[] lines = FileUtils.FileSize.load(folder + f);
				list.loaded.Add(FileUtils.FileSize.toChar(lines));
				list.sizes.Add(FileUtils.FileSize.getSize(lines));
			}
		
			return list;
		}
	}	
}

public abstract class ILayoutGenerator
{
	protected List<FileUtils.Glyph> wholeLayout;

	protected FileUtils.FileSize totalSize;

	abstract public List<FileUtils.Glyph> getLayout();

	abstract public void generateLayout();

	abstract public FileUtils.FileSize getLevelSize();
}

public class BasicFileLevelLoader : ILayoutGenerator
{
	string filePath;

	public BasicFileLevelLoader(string file)
	{
		filePath = file;
		wholeLayout = new List<FileUtils.Glyph>();
		totalSize = new FileUtils.FileSize();
	}

	public override List<FileUtils.Glyph> getLayout()
	{
		return wholeLayout;
	}

	string[] load(string path)
	{
		TextAsset data = Resources.Load(path) as TextAsset;

		return FileUtils.FileSize.splitLinesOfFile(data.text);
	}

	public override void generateLayout()
	{
		string[] lines = load(filePath);
		totalSize = FileUtils.FileSize.getSize(lines);
		wholeLayout = FileUtils.FileSize.toChar(lines);
	}

	public override FileUtils.FileSize getLevelSize()
	{
		return totalSize;
	}
}

public class BasicLevelGenerator : ILayoutGenerator
{
	public enum GenerationStyle
	{
		Random,
		InOrder
	}

	public GenerationStyle genStyle = GenerationStyle.Random;

	public override List<FileUtils.Glyph> getLayout()
	{
		return wholeLayout;
	}

	public BasicLevelGenerator(GenerationStyle style)
	{
		genStyle = style;
		wholeLayout = new List<FileUtils.Glyph>();
		totalSize = new FileUtils.FileSize();
	}

	FileUtils.FileList loadFileList(string folder)
	{
		String path = folder + "file_list";
        TextAsset data = Resources.Load(path) as TextAsset;

		TextReader sr = new StringReader(data.text);

		XmlSerializer serial = new XmlSerializer(typeof(FileUtils.FileList));
		FileUtils.FileList list = (FileUtils.FileList)serial.Deserialize(sr);

		foreach(string f in list.files)
		{
			string[] lines = loadTileGroup(folder, f);
			list.loaded.Add(FileUtils.FileSize.toChar(lines));
			list.sizes.Add(FileUtils.FileSize.getSize(lines));
		}
		
		return list;
	}

	public override FileUtils.FileSize getLevelSize()
	{
		return totalSize;
	}
	
	string[] loadTileGroup(string folder, string name)
	{
		String path = folder + name;
		TextAsset data = Resources.Load(path) as TextAsset;

		return FileUtils.FileSize.splitLinesOfFile(data.text);
	}

	void createTileType(GameObject[] tiles, float xPos, float yPos)
	{
		if(tiles.Length == 0)
		{
			Debug.LogError("Do not have any prefabs set in createTileType");
			return;
		}
		GameObject instance = tiles[Random.Range(0, tiles.Length)];

		if(instance == null)
		{
			Debug.LogError("You have a null instance in createTileType");
			return;
		}
		UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
	}

	private void addBlockSize(FileUtils.FileSize block)
	{
		if(totalSize.empty())
		{
			totalSize.xSize = block.xSize;
			totalSize.ySize = block.ySize;
		}
		else
		{
			if(totalSize.ySize != block.ySize)
			{
				Debug.LogError("Blocks in LevelGeneration should have the same Y value total : " + totalSize.ySize + " and merging one : " + block.ySize);
			}

			totalSize.xSize += block.xSize;
		}
	}

	private void merge(List<FileUtils.Glyph> layoutToMerge, FileUtils.FileSize size)
	{
		List<FileUtils.Glyph> newLayout = new List<FileUtils.Glyph>();

		if(totalSize.ySize != 0 && size.ySize != totalSize.ySize)
		{
			Debug.LogError("Blocks in LevelGeneration should have the same Y value total : " + totalSize.ySize + " and merging one : " + size.ySize);
		}

		for(int y = 0; y < size.ySize; ++y)
		{
			for(int x = 0; x < totalSize.xSize; ++x)
			{
				newLayout.Add(wholeLayout[y * totalSize.xSize + x]);
			}

			for(int x = 0; x < size.xSize; ++x)
			{
				newLayout.Add(layoutToMerge[y * size.xSize + x]);
			}
		}

		wholeLayout = newLayout;
	}

	public override void generateLayout()
	{
		FileUtils.FileList block = new FileUtils.FileList();
		
		block = loadFileList("LevelGeneration/");

		for(int x = 0; x < block.filesNumber(); ++x)
		{
			List<FileUtils.Glyph> layout;
			
			if(genStyle == GenerationStyle.Random)
			{
				layout = block.getOneRandom();
			}
			else //if(genStyle == GenerationStyle.InOrder)
			{
				layout = block.getNextOne();
			}

			merge(layout, block.getSize());
			addBlockSize(block.getSize());
		}
	}

}
