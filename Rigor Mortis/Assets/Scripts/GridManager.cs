﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private GameObject enemyContainter;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject playerContainter;
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private TextAsset levelMap;

    xml.levels xmlData;

    // Start is called before the first frame update
    void Start()
    {
        xmlData = XmlReader<xml.levels>.ReadXMLAsBytes(levelMap.bytes);
        GenerateLevel();
        PlaceEnemy();
    }

    /**
     * Generate Level
     * This creates a reader to get the information from the xml
     * Select the level by name
     * Get each of the map lines in the level
     * Get each value in the arrays, set them to a multidimensional using their position in array as co-ordinates
     * */
    void GenerateLevel()
    {
        var level = xmlData.level;
       
        var maplines = level.map.Select(m => m.value.Split(',').Select(v => int.Parse(v)).ToArray()).ToArray();

        var anonMap = level.map.SelectMany((m, x) => m.value.Split(',').Select((v, z) => new { Value = int.Parse(v), ZPos = z, XPos = x })).ToArray();
        foreach(var pos in anonMap)
        {
            if (pos.Value >= 0)
            {
                GameObject tile = Instantiate(tiles[pos.Value], new Vector3(pos.XPos, 0, pos.ZPos), new Quaternion(), gameObject.transform);
                tile.GetComponent<BlockScript>().coordinates = new Vector3(pos.XPos, 0, pos.ZPos);
                tile.name = tile.name.Replace("(Clone)", "");
                tile.name = tile.name + '(' + pos.XPos + ',' + pos.ZPos + ')';
            }
        }
    }

    void PlaceEnemy()
    {
        var level = xmlData.level;
        var enemies = level.enemies;

        foreach(var enemy in enemies)
        {
            GameObject placedEnemy = Instantiate(enemyPrefabs[enemy.type], new Vector3(enemy.posX, 1, enemy.posZ), new Quaternion(), enemyContainter.transform);
            placedEnemy.name = enemy.name;
        }
        GameObject placedPlayer = Instantiate(playerPrefabs[0], new Vector3( 7,1, 10), new Quaternion(), playerContainter.transform);
        placedPlayer.name = "target";
    }

    /**
     * This is the xml reader that will get the file by path
     * Serialise the data of the xml
     * Read the data of the file
     * Return the deseriabled data
     */
    public class XmlReader<T> where T : class
    {
        public static T ReadXML(string path)
        {
            var _serializer = new XmlSerializer(typeof(T));

            var xml = File.ReadAllBytes(path);

            using (var memoryStream = new MemoryStream(xml))
            {
                using (var reader = new XmlTextReader(memoryStream))
                {
                    return (T)_serializer.Deserialize(reader);
                }
            }
        }

        public static T ReadXMLAsBytes(byte[] xmlData)
        {
            var _serializer = new XmlSerializer(typeof(T));

            using (var memoryStream = new MemoryStream(xmlData))
            {
                using (var reader = new XmlTextReader(memoryStream))
                {
                    return (T)_serializer.Deserialize(reader);
                }
            }
        }
    }
}
namespace xml
{


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class levels
    {

        private levelsLevel levelField;

        /// <remarks/>
        public levelsLevel level
        {
            get
            {
                return this.levelField;
            }
            set
            {
                this.levelField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class levelsLevel
    {

        private levelsLevelMapline[] mapField;

        private levelsLevelEnemy[] enemiesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("map-line", IsNullable = false)]
        public levelsLevelMapline[] map
        {
            get
            {
                return this.mapField;
            }
            set
            {
                this.mapField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("enemy", IsNullable = false)]
        public levelsLevelEnemy[] enemies
        {
            get
            {
                return this.enemiesField;
            }
            set
            {
                this.enemiesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class levelsLevelMapline
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class levelsLevelEnemy
    {

        private string nameField;

        private byte typeField;

        private byte posXField;

        private byte posZField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte posX
        {
            get
            {
                return this.posXField;
            }
            set
            {
                this.posXField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte posZ
        {
            get
            {
                return this.posZField;
            }
            set
            {
                this.posZField = value;
            }
        }
    }


}