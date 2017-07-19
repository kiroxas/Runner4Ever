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
		
		public List<List<int>> loaded{ get; set;}
		
		public List<int> getOneRandom()
		{
			index = Random.Range(0, loaded.Count);
			return loaded[index];
		}

		public List<int> getNextOne()
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
			list.loaded.Add(toInt(loadTileGroup(folder, f)));	
		}
		
		return list;
	}
	
	List<int> toInt(string[] lines)
	{
		List<int> list = new List<int>();
		
		foreach(string s in lines)
		{
			foreach(char c in s)
			{
				list.Add((int)Char.GetNumericValue(c));
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

	void createSection(List<int> toCreate, int xStart, int yStart)
	{
		
		int index = 0;
		for(int y =0; y < yTilePerSection; ++y)
		{
			for(int x = 0; x < xTilePerSection; ++x)
			{
				int tileType = toCreate[index];
				float yPos = yStart + (yTilePerSection - y) * tileHeight;
				float xPos = xStart + x * tileWidth;

				if(tileType == 1) // ground tile
				{
					if(landTiles.Length == 0)
					{
						Debug.LogError("Do not have any prefabs set in landTiles");
						return;
					}
					GameObject instance = landTiles[Random.Range(0, landTiles.Length)];

					if(instance == null)
					{
						Debug.LogError("You have a null instance in landTiles");
						return;
					}
					UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
				} 
				else if (tileType == 3) // random object
				{
					if(objectTiles.Length == 0)
					{
						Debug.LogError("Do not have any prefabs set in objectTiles");
						return;
					}
					GameObject instance = objectTiles[Random.Range(0, objectTiles.Length)];


					if(instance == null)
					{
						Debug.LogError("You have a null instance in objectTiles");
						return;
					}
					UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
				}
				else if (tileType == 2) // water tile
				{
					if(waterTiles.Length == 0)
					{
						Debug.LogError("Do not have any prefabs set in waterTiles");
						return;
					}
					GameObject instance = waterTiles[Random.Range(0, waterTiles.Length)];


					if(instance == null)
					{
						Debug.LogError("You have a null instance in waterTiles");
						return;
					}
					UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
				}
				else if(tileType == 4) // spawn player
				{
					createPlayer(xPos, yPos);
					createCheckpoint(xPos, yPos);
				}
				else if(tileType == 5)
				{
					createCheckpoint(xPos, yPos);
				}
				else if(tileType == 6) // tile that inverse wall jump
				{
					if(inverseLandTiles.Length == 0)
					{
						Debug.LogError("Do not have any prefabs set in inverseLandTiles");
						return;
					}
					GameObject instance = inverseLandTiles[Random.Range(0, inverseLandTiles.Length)];


					if(instance == null)
					{
						Debug.LogError("You have a null instance in inverseLandTiles");
						return;
					}
					UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
				}
				else if(tileType == 7) // tile that hurt player
				{
					if(hurtTiles.Length == 0)
					{
						Debug.LogError("Do not have any prefabs set in hurtTiles");
						return;
					}
					GameObject instance = hurtTiles[Random.Range(0, hurtTiles.Length)];


					if(instance == null)
					{
						Debug.LogError("You have a null instance in hurtTiles");
						return;
					}
					UnityEngine.Object.Instantiate(instance, new Vector3(xPos, yPos, 0),  Quaternion.identity);
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

		Debug.Log("blocks loaded " + block.loaded.Count);
		for(int i = 0; i < block.loaded.Count; ++i)
		{
			Debug.Log(block.loaded[i].Count);
		}

		for(int x = 0; x < tileGroupsNumber; ++x)
		{
			List<int> layout;
			
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
