using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace ProjectRogue
{
    public class KeyMapper
    {

        static Dictionary<string, KeyMapper> keyMapperDatabase = new Dictionary<string, KeyMapper>();

        public static KeyMapper getMapper(string name)
        {
            return keyMapperDatabase[name];
        }

        Dictionary<string, List<MappedKey>>  mappings = new Dictionary<string, List<MappedKey>>();

        public void AddMapping(string name, List<MappedKey> keys)
        {
            if(!mappings.ContainsKey(name))
            {
                mappings.Add(name, keys);
            }
            else
            {
                mappings[name].AddRange(keys);
            }
        }

        public void AddMapping(string name, MappedKey key)
        {
            AddMapping(name, new List<MappedKey> { key });
        }

        public static void LoadKeyMappings(string path)
        {
            using(FileStream file = File.OpenRead(path))
            using(StreamReader reader = new StreamReader(file))
            {
                Dictionary<string, KeyMapper> output = new Dictionary<string, KeyMapper>();

                string currentGUI = "";

                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line == "")
                        continue;

                    if (line[0] == '/' && line[1] == '/')
                        continue;

                    if (line[0] == '#')
                    {
                        line = line.Remove(0, 1);
                        output.Add(line, new KeyMapper());
                        currentGUI = line;
                        continue;
                    }
                    
                    string[] parts = line.Split(new string[]{": "}, StringSplitOptions.RemoveEmptyEntries);

                    string[] firstParts = parts[0].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] secondParts = parts[1].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    string command = firstParts[0];

                    string key = secondParts[0];

                    string keyHelp = "";
                    if (firstParts.Length > 1)
                        keyHelp = firstParts[1];
                    else
                        keyHelp = key;

                    string modifiers = "";
                    if (secondParts.Length > 1)
                        modifiers = secondParts[1];
                    else
                        modifiers = "None";

                    MappedKey mappedKey = new MappedKey((Keys)Enum.Parse(typeof(Keys), key), (Modifiers)Enum.Parse(typeof(Modifiers), modifiers), keyHelp);
                    output[currentGUI].AddMapping(command, mappedKey);
                }

                keyMapperDatabase = output;
            }
        }

        public bool HasState(string name, KeyboardState state)
        {
            if (!mappings.ContainsKey(name))
                throw new MappingExeption(name);

            List<Keys> downKeys = state.GetPressedKeys().ToList();

            if (downKeys.Count < 1)
                return false;

            List<MappedKey> key = mappings[name];

            foreach(MappedKey mappedKey in key)
            {
                if (!downKeys.Contains(mappedKey.key))
                    continue;

                List<Modifiers> splitModifiers = new List<Modifiers>();

                switch (mappedKey.modifiers)
                {
                    case Modifiers.ShiftControlAlt:
                        splitModifiers.Add(Modifiers.Shift);
                        splitModifiers.Add(Modifiers.Control);
                        splitModifiers.Add(Modifiers.Alt);
                        break;
                    case Modifiers.ControlAlt:
                        splitModifiers.Add(Modifiers.Control);
                        splitModifiers.Add(Modifiers.Alt);
                        break;
                    case Modifiers.ShiftAlt:
                        splitModifiers.Add(Modifiers.Shift);
                        splitModifiers.Add(Modifiers.Alt);
                        break;
                    case Modifiers.ShiftControl:
                        splitModifiers.Add(Modifiers.Shift);
                        splitModifiers.Add(Modifiers.Control);
                        break;
                    default:
                        splitModifiers.Add(mappedKey.modifiers);
                        break;
                }

                List<Modifiers> realModifiers = new List<Modifiers>();
                int modifierCount = 0;

                if (downKeys.Contains(Keys.LeftAlt) || downKeys.Contains(Keys.RightAlt))
                {
                    realModifiers.Add(Modifiers.Alt);
                    modifierCount++;
                }
                if (downKeys.Contains(Keys.LeftShift) || downKeys.Contains(Keys.RightShift))
                {
                    realModifiers.Add(Modifiers.Shift);
                    modifierCount++;
                }
                if (downKeys.Contains(Keys.LeftControl) || downKeys.Contains(Keys.RightControl))
                {
                    realModifiers.Add(Modifiers.Control);
                    modifierCount++;
                }

                if (modifierCount == 0)
                    realModifiers.Add(Modifiers.None);

                if (downKeys.Count - modifierCount != 1)
                    continue;

                if (!MyMath.ScrambledEquals(realModifiers, splitModifiers))
                    continue;

                return true;
            }

            return false;           
        }
    }

    public class MappingExeption :Exception
    {
        public MappingExeption(string name)
            :base("The mapping " + name + " does not exist!") {}
    }

    public enum Modifiers {Shift, Control, Alt, ShiftControl, ShiftAlt, ControlAlt, ShiftControlAlt, None};

    public class MappedKey
    {
        internal Keys key;
        internal Modifiers modifiers;
        internal string help;

        public MappedKey(Keys key, Modifiers modifiers)
            : this(key, modifiers, key.ToString()) { }

        public MappedKey(Keys key, Modifiers modifiers, string keyHelp)
        {
            this.key = key;
            this.modifiers = modifiers;
            this.help = keyHelp;
        }

        public override int GetHashCode()
        {
            return (int) key;
        }

        public override bool Equals(Object obj)
        {
            MappedKey vec = obj as MappedKey;
            if ((object)vec == null)
                return false;

            return this == vec;
        }


        public static bool operator ==(MappedKey key1, MappedKey key2)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(key1, key2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)key1 == null) || ((object)key2 == null))
            {
                return false;
            }

            // Return true if the fields match:
            return key1.key == key2.key && key1.modifiers == key2.modifiers;
        }

        public static bool operator !=(MappedKey a, MappedKey b)
        {
            return !(a == b);
        }

    }
}
