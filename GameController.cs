using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using System.IO.Compression;
using System.Diagnostics;

namespace ProjectRogue
{
    public static class GameController
    {
        public const string version = "0.13.0 \"Lloyanastio\"";
        public const string myMagicHeader = "This is a rogue save file, it will destroy you.";
        public const string separator = ",";
        public const string comment = "//";
        public const string FileName = @"./save/save.rogue";
        public const string dictionaryPath = @"./resources/dict.txt";


        private static GUI currentgui = null;

        public static GUI currentGUI
        {
            get
            {
                return currentgui;
            }
            set
            {
                if (value != null && !value.Loaded)
                    value.Load(currentGUI);
                else
                    currentgui = value;
            }
        }
        public static ProjectRogue mainWindow;

        private enum CompressionAlgorithm { dotNET, None }

        public static SaveGame saveGame;

        public static Player player
        {
            get { return saveGame.player; }
            set { saveGame.player = value; }
        }

        public static int currentFloor
        {
            get { return saveGame.currentFloor; }
            set { saveGame.currentFloor = value; }
        }

        public static int turnCounter
        {
            get { return saveGame.turnCounter; }
            set { saveGame.turnCounter = value; }
        }

        public static TileMap map
        {
            get { return saveGame.map[currentFloor]; }
            set { saveGame.map[currentFloor] = value; }
        }

        public static int mapX
        {
            get { return map.mapX; }
            set { map.mapX = value; }
        }

        public static int mapY
        {
            get { return map.mapY; }
            set { map.mapY = value; }
        }

        public static Dictionary<int, GameObject> gameObjectDatabase
        {
            get { return saveGame.gameObjectDatabase[currentFloor]; }
            set { saveGame.gameObjectDatabase[currentFloor] = value;}
        }

        public static List<Creature> creatureDatabase
        {
            get { return saveGame.creatureDatabase[currentFloor]; }
            set { saveGame.creatureDatabase[currentFloor] = value; }
        }

        public static int lastId
        {
            get { return saveGame.lastId[currentFloor]; }
            set { saveGame.lastId[currentFloor] = value; }
        }

        public static List<Message> gameLog
        {
            get { return saveGame.gameLog; }
            set { saveGame.gameLog = value; }
        }

        public static int seed;
        public static Random r;

        public static bool save = true;


        static CompressionAlgorithm compressSaves = CompressionAlgorithm.dotNET;

        public static void SaveGame()
        {
            using (FileStream saveFileStream = File.Create(FileName))
            {
                using (StreamWriter headerWriter = new StreamWriter(saveFileStream))
                {
                    headerWriter.WriteLine(myMagicHeader);
                    headerWriter.WriteLine(version);
                    headerWriter.WriteLine(compressSaves.ToString());
                }
            }

            using(FileStream saveFileStream = File.Open(FileName, FileMode.Append))
            {
                switch(compressSaves)
                {
                    case CompressionAlgorithm.dotNET:
                    {
                        using (DeflateStream compressedStream = new DeflateStream(saveFileStream, CompressionMode.Compress))
                        {
                            using (StreamWriter saveFileWriter = new StreamWriter(compressedStream))
                            {
                                WriteData(saveFileWriter);
                            }
                        }
                        break;
                    }

                    default:
                    {
                        using (StreamWriter saveFileWriter = new StreamWriter(saveFileStream))
                        {
                            WriteData(saveFileWriter);
                        }
                        break;
                    }
                }
            }
        }

        private static void WriteData(StreamWriter saveFileWriter)
        {
            saveFileWriter.WriteLine(saveGame.maxFloors);
            saveFileWriter.WriteLine(saveGame.currentFloor);
            saveFileWriter.WriteLine(saveGame.turnCounter);

            for (int i = 0; i < saveGame.maxFloors; i++) // write all gameObjects;
            {
                saveFileWriter.WriteLine(comment + "GameObjects for floor " + i.ToString());

                saveFileWriter.WriteLine(saveGame.map[i].mapX);
                saveFileWriter.WriteLine(saveGame.map[i].mapY);

                List<Tile> tiles = new List<Tile>(); //tiles need to be initialized first!
                List<Creature> creatures = new List<Creature>(); //creatures need to be second!
                List<GameObject> misc = new List<GameObject>(); //random stuff (hopefully nothing)

                foreach(GameObject go in saveGame.gameObjectDatabase[i].Values)
                {
                    if(go as Tile != null)
                    {
                        tiles.Add((Tile)go);
                    }
                    else if (go as Creature != null)
                    {
                        creatures.Add((Creature)go);
                    }
                    else
                    {
                        misc.Add(go);
                    }
                }

                saveFileWriter.WriteLine(comment + "tiles");

                foreach (Tile tile in tiles) // write tiles
                {
                    string saveString = string.Join(separator, tile.saveString);
                    saveFileWriter.WriteLine(saveString);
                }

                saveFileWriter.WriteLine(comment + "creatures");

                foreach (Creature creature in creatures) // write creatures
                {
                    string saveString = string.Join(separator, creature.saveString);
                    saveFileWriter.WriteLine(saveString);
                }

                if (misc.Count > 0)
                {
                    saveFileWriter.WriteLine(comment + "misc"); // write misc

                    foreach (GameObject go in misc)
                    {
                        string saveString = string.Join(separator, go.saveString);
                        saveFileWriter.WriteLine(saveString);
                    }
                }
            }
        }

        static bool ignoreVersion = false;
        static void ignoreVersionLoad()
        {
            ignoreVersion = true;
            LoadGame();
            ignoreVersion = false;
        }
        

        public static void LoadGame()
        {
            save = true; //we want to save this if valid

            int position;
            CompressionAlgorithm compressed;

            using(FileStream saveFileStream = File.OpenRead(FileName))
            {
                string magicHeader;
                string headerVersion;
                string compressHeader;

                using (StreamReader headerReader = new StreamReader(saveFileStream))
                {
                    magicHeader = headerReader.ReadLine();
                    if(magicHeader != myMagicHeader)
                    {
                        save = false;
                        currentGUI = new GUIYesNo("This save file is not valid. Overwrite file with new game?", NewGame, currentGUI.Close);
                        return;
                    }

                    headerVersion = headerReader.ReadLine();
                    if (version != headerVersion && !ignoreVersion)
                    {
                        currentGUI = new GUIYesNo("The save file appears to be an older version (" + headerVersion + ", the current verion is " + version + "). Try loading anyway?",
                            NewGame, ignoreVersionLoad);
                        return;
                    }

                    compressHeader = headerReader.ReadLine();
                    compressed = (CompressionAlgorithm)Enum.Parse(typeof(CompressionAlgorithm), compressHeader);
                }

                //reset the position because streamReader uses buffer
                position = Encoding.UTF8.GetByteCount(magicHeader + Environment.NewLine + headerVersion + Environment.NewLine + compressHeader + Environment.NewLine); // utf8 preamble is 3 bytes

            }

            using(FileStream saveFileStream = File.OpenRead(FileName))
            {
                saveFileStream.Seek(position, SeekOrigin.Begin);
                try
                {
                    switch(compressed)
                    {
                        case CompressionAlgorithm.dotNET:
                        {
                            using (DeflateStream decompressedStream = new DeflateStream(saveFileStream, CompressionMode.Decompress))
                            {
                                using (StreamReader saveFileReader = new StreamReader(decompressedStream))
                                {
                                    ReadData(saveFileReader);
                                }
                            }
                            break;
                        }
                        default:
                        {
                            using (StreamReader saveFileReader = new StreamReader(saveFileStream))
                            {
                                ReadData(saveFileReader);
                            }
                            break;
                        }
                    }   
                }
                catch(FormatException)
                {
                    save = false;
                    currentGUI = new GUIYesNo("This save file is not valid. Overwrite file with new game?", NewGame, currentGUI.Close);
                    return;

                }
                catch (InvalidOperationException)
                {
                    save = false;
                    currentGUI = new GUIYesNo("This save file is not valid. Overwrite file with new game?", NewGame, currentGUI.Close);
                    return;
                }
            }

            DeleteGame();
            GameLog.newMessage("Welcome back, " + player.name + "!");
            GraphX.UpdateVisibleArea(player, map);
        }

        private static void ReadData(StreamReader saveFileReader)
        {
            saveGame = new SaveGame(Convert.ToInt32(saveFileReader.ReadLine()));

            int actualCurrentFloor = Convert.ToInt32(saveFileReader.ReadLine());
            turnCounter = Convert.ToInt32(saveFileReader.ReadLine());

            saveFileReader.ReadLine();

            for (currentFloor = 0; currentFloor < saveGame.maxFloors; currentFloor++)
            {

                int tempMapX = Convert.ToInt32(saveFileReader.ReadLine());
                int tempMapY = Convert.ToInt32(saveFileReader.ReadLine());

                saveFileReader.ReadLine();

                string saveString = saveFileReader.ReadLine();
                while ((saveString[0].ToString() + saveString[1].ToString()) != "//") //read tiles
                {
                    Queue<string> saveStrings = new Queue<string>(saveString.Split(new string[] { separator }, StringSplitOptions.None));
                    Type type = Type.GetType("ProjectRogue." + saveStrings.Dequeue());
                    var instance = Activator.CreateInstance(type);
                    GameObject go = (GameObject)instance;
                    go.Load(saveStrings);
                    saveString = saveFileReader.ReadLine();
                }

                saveGame.map[currentFloor] = new TileMap(tempMapX, tempMapY, saveGame.gameObjectDatabase[currentFloor].Select(x => (Tile)x.Value).ToList());

                if (saveFileReader.EndOfStream)
                {
                    currentFloor = actualCurrentFloor;
                    return;
                }

                saveString = saveFileReader.ReadLine();
                while ((saveString[0].ToString() + saveString[1].ToString()) != "//") //read creatures
                {
                    Queue<string> saveStrings = new Queue<string>(saveString.Split(new string[] { separator }, StringSplitOptions.None));
                    Type type = Type.GetType("ProjectRogue." + saveStrings.Dequeue());
                    var instance = Activator.CreateInstance(type);
                    GameObject go = (GameObject)instance;
                    go.Load(saveStrings);

                    if (saveFileReader.EndOfStream)
                    {
                        currentFloor = actualCurrentFloor;
                        return;
                    }

                    saveString = saveFileReader.ReadLine();
                }
                
                if(saveString == comment + "misc")
                {
                    saveString = saveFileReader.ReadLine();
                    while ((saveString[0].ToString() + saveString[1].ToString()) != "//") //read misc
                    {
                        Queue<string> saveStrings = new Queue<string>(saveString.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
                        Type type = Type.GetType("ProjectRogue." + saveStrings.Dequeue());
                        var instance = Activator.CreateInstance(type);
                        GameObject go = (GameObject)instance;
                        go.Load(saveStrings);

                        if (saveFileReader.EndOfStream)
                        {
                            currentFloor = actualCurrentFloor;
                            return;
                        }

                        saveString = saveFileReader.ReadLine();
                    }
                }
            }

            currentFloor = actualCurrentFloor;
        }

        public static void NewGame()
        {
            save = true; //for now, we want to save

            saveGame = new SaveGame(3);
            r = new Random();

            //create the map
            for (currentFloor = 0; currentFloor < saveGame.map.Length; currentFloor++)
            {
                saveGame.map[currentFloor] = new TileMap(new Map(100, 100, r, currentFloor));
            }
            currentFloor = 0;

            //spawn the player
            List<Tile> dummy = new List<Tile>();
            dummy.AddRange(map.emptyTiles);

            Tile t = dummy[r.Next(dummy.Count)];
            dummy.Remove(t);

            NameGenerator ng = new NameGenerator(NameGenerator.LoadDictionary(dictionaryPath), 3, .001, true);
            string name = ng.GenerateName();
            player = new Player(t.x, t.y, 9, name);

            //start message
            GameLog.newMessage("Welcome, " + player.name + "!");
            GameLog.newMessage("Trog says: KILL THEM ALL!!", Color.Red);

            // spawn the monsters
            for (int i = 0; i < 50; i++)
            {
                t = dummy[r.Next(dummy.Count)];
                new Rat(t.x, t.y);
                dummy.Remove(t);
            }
            GraphX.UpdateVisibleArea(player, map);
        }

        public static void DeleteGame()
        {
            File.Delete(FileName);
        }
    }
}
