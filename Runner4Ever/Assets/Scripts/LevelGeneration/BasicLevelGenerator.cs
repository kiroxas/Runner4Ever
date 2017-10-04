using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace FileUtils
{
	public class FileSize
	{
		public int xSize;
		public int ySize;

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
	}

	[XmlRoot(ElementName="FileList")]
	public class FileList
	{
		[XmlArray("files")]
		[XmlArrayItem("file")]
		public List<string> files{ get; set;}

		public int index = 0;
		
		public List<FileSize> sizes{ get; set;}
		public List<List<char>> loaded{ get; set;}
		
		public List<char> getOneRandom()
		{
			index = Random.Range(0, loaded.Count);
			return loaded[index];
		}

		public FileSize getSize()
		{
			return sizes[index];
		}

		public List<char> getNextOne()
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
	}
}

public abstract class ILayoutGenerator
{
	protected List<char> wholeLayout;

	protected FileUtils.FileSize totalSize;

	abstract public List<char> getLayout();

	abstract public void generateLayout();

	abstract public FileUtils.FileSize getLevelSize();
}

public class BasicLevelGenerator : ILayoutGenerator
{
	public enum GenerationStyle
	{
		Random,
		InOrder
	}

	public GenerationStyle genStyle = GenerationStyle.Random;

<<<<<<< HEAD
	public GameObject instancePlayer;
	public GameObject checkpoint;
	public GameObject[] landTiles;
	public GameObject[] inverseLandTiles;
	public GameObject[] waterTiles;
	public GameObject[] objectTiles;
	public GameObject[] hurtTiles;
	public GameObject[] enemies;
	public GameObject[] disapearingTile;
	public GameObject[] escalator;
	public GameObject[] bumperTile;
	public GameObject[] movingTile;
	public GameObject[] killMovingTile;
	public GameObject[] standOnTile;
	public GameObject[] jumperTile;
=======
	public override List<char> getLayout()
	{
		return wholeLayout;
	}

	public BasicLevelGenerator(GenerationStyle style)
	{
		genStyle = style;
		wholeLayout = new List<char>();
		totalSize = new FileUtils.FileSize();
	}
>>>>>>> Pooling

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
			//Array.Reverse(lines);
			list.loaded.Add(toChar(lines));	
			list.sizes.Add(getSize(lines));
		}
		
		return list;
	}
	
	FileUtils.FileSize getSize(string[] lines)
	{
		int xSize = -1;
		int ySize = 0;

		foreach(string s in lines)
		{
			int xTemp = s.Length;
			if(xTemp == 0)
				continue;

			ySize++;

			if(xSize == -1)
			{
				xSize = xTemp;
			}
			else
			{
				if(xSize != xTemp)
				{
					Debug.LogError("The size of lines should be equal xSize : " + xSize + " new : " + xTemp);
				}
			}
		}

		return new FileUtils.FileSize(xSize, ySize);
	}

	List<char> toChar(string[] lines)
	{
		List<char> list = new List<char>();
		
		foreach(string s in lines)
		{
			foreach(char c in s)
			{
				list.Add(c);
			}
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
		string fs = data.text;
  		string[] lines = System.Text.RegularExpressions.Regex.Split ( fs, "\n|\r|\r\n" );

		 return lines;
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
<<<<<<< HEAD
				char tileType = toCreate[index];
				float yPos = yStart + (yTilePerSection - y) * tileHeight;
				float xPos = xStart + x * tileWidth;

				if(tileType == '1') // ground tile
				{
					createTileType(landTiles, xPos, yPos);
				} 
				else if (tileType == '3') // random object
				{
					createTileType(objectTiles, xPos, yPos);
				}
				else if (tileType == '2') // water tile
				{
					createTileType(waterTiles, xPos, yPos);
				}
				else if(tileType == '4') // spawn player
				{
					createPlayer(xPos, yPos);
					createCheckpoint(xPos, yPos);
				}
				else if(tileType == '5')
				{
					createCheckpoint(xPos, yPos);
				}
				else if(tileType == '6') // tile that inverse wall jump
				{
					createTileType(inverseLandTiles, xPos, yPos);
				}
				else if(tileType == '7') // tile that hurt player
				{
					createTileType(hurtTiles, xPos, yPos);
				}
				else if(tileType == '8') // enemy
				{
					createTileType(enemies, xPos, yPos);
				}
				else if(tileType == '9') // disapearing Tile
				{
					createTileType(disapearingTile, xPos, yPos);
				}
				else if(tileType == 'A') // escalator
				{
					createTileType(escalator, xPos, yPos);
				}
				else if(tileType == 'B') // bumper tile
				{
					createTileType(bumperTile, xPos, yPos);
				}
				else if(tileType == 'C') // moving tile
				{
					createTileType(movingTile, xPos, yPos);
				}
				else if(tileType == 'D') // kill moving tile
				{
					createTileType(killMovingTile, xPos, yPos);
				}
				else if(tileType == 'E') // stand on tile
				{
					createTileType(standOnTile, xPos, yPos);
				}
				else if(tileType == 'F') // jumper tile
				{
					createTileType(jumperTile, xPos, yPos);
				}

				++index;
=======
				Debug.LogError("Blocks in LevelGeneration should have the same Y value total : " + totalSize.ySize + " and merging one : " + block.ySize);
>>>>>>> Pooling
			}

			totalSize.xSize += block.xSize;
		}
	}

	private void merge(List<char> layoutToMerge, FileUtils.FileSize size)
	{
		List<char> newLayout = new List<char>();

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
			List<char> layout;
			
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
