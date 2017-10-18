using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class LocalizationUtils  
{
	public enum Languages
	{
		en, // english
		fr,  // french
		jp   // japanese
	}

	static public Languages GetNext(Languages value)
	{
    	return (from Languages val in System.Enum.GetValues(typeof (Languages)) 
            where val > value 
            orderby val 
            select val).DefaultIfEmpty().First();
	}

	static public string getFlagSpritePath(Languages lang)
	{
		return "UI/" + lang.ToString();
	}

	static public string getLocFilePath(Languages lang)
	{
		return "Localization/" + lang.ToString();
	}
}