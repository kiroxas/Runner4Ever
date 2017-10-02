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
	[XmlRoot(ElementName="FileList")]
	public class FileList
	{
		[XmlArray("files")]
		[XmlArrayItem("file")]
		public List<string> files{ get; set;}

		public int index = 0;
		
		public List<List<char>> loaded{ get; set;}
		
		public List<char> getOneRandom()
		{
			index = Random.Range(0, loaded.Count);
			return loaded[index];
		}

		public List<char> getNextOne()
		{
			index++;
			if(index >= loaded.Count)
				index =0;

			return loaded[index];	
		}
	}
}

public class BasicLevelGenerator : MonoBehaviour 
{
	public enum GenerationStyle
	{
		Random,
		InOrder
	}

	public int tileGroupsNumber = 4;
	public int xTilePerSection = 6;
	public int yTilePerSection = 6;

	public float tileWidth = 1.28f;
	public float tileHeight = 1.28f;

	public float bottomLeftXPos = 0;
	public float bottomLeftYPos = 0;

	public GenerationStyle genStyle = GenerationStyle.Random;

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

	FileUtils.FileList loadFileList(string folder)
	{
		//XmlSerializer serial = new XmlSerializer(typeof(FileUtils.FileList));
       // Stream reader = new FileStream(folder + "file_list.xml", FileMode.Open);
        //FileUtils.FileList list = (FileUtils.FileList)serial.Deserialize(reader);
		String path = folder + "file_list";
        TextAsset data = Resources.Load(path) as TextAsset;
        //XmlDocument xmldoc = new XmlDocument ();
		//xmldoc.LoadXml ( data.text );
		Debug.Log(path);
		Debug.Log(data.text);
		TextReader sr = new StringReader(data.text);

		XmlSerializer serial = new XmlSerializer(typeof(FileUtils.FileList));
		FileUtils.FileList list = (FileUtils.FileList)serial.Deserialize(sr);

		foreach(string f in list.files)
		{
			list.loaded.Add(toChar(loadTileGroup(folder, f)));	
		}
		
		return list;
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
	
	string[] loadTileGroup(string folder, string name)
	{
		String path = folder + name;
		TextAsset data = Resources.Load(path) as TextAsset;
		string fs = data.text;
  		string[] lines = System.Text.RegularExpressions.Regex.Split ( fs, "\n|\r|\r\n" );
		//string[] lines = System.IO.File.ReadAllLines(@folder + name);

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

	void createSection(List<char> toCreate, int xStart, int yStart)
	{
		
		int index = 0;
		for(int y =0; y < yTilePerSection; ++y)
		{
			for(int x = 0; x < xTilePerSection; ++x)
			{
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
			}
		}
	}

	void createPlayer(float x, float y)
	{
		GameObject player = UnityEngine.Object.Instantiate(instancePlayer, new Vector3(x , y , 0),  Quaternion.identity);
		FindObjectOfType<CameraFollow>().target = player.GetComponent<Transform>();
	}

	void createCheckpoint(float x, float y)
	{
		UnityEngine.Object.Instantiate(checkpoint, new Vector3(x , y , 0),  Quaternion.identity);
	}

	// Use this for initialization
	void Awake () 
	{
		FileUtils.FileList block = new FileUtils.FileList();
		
		block = loadFileList("LevelGeneration/");

		for(int x = 0; x < tileGroupsNumber; ++x)
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

			int xStart = (int)(bottomLeftXPos + x * tileWidth * xTilePerSection);
			createSection(layout, xStart, (int)bottomLeftYPos);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
