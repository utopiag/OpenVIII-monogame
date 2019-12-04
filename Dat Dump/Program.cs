﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace OpenVIII.Dat_Dump
{
    internal class Program
    {
        #region Methods

        static ConcurrentDictionary<int, Debug_battleDat> MonsterData = new ConcurrentDictionary<int, Debug_battleDat>();
        static ConcurrentDictionary<int, Debug_battleDat> CharacterData = new ConcurrentDictionary<int, Debug_battleDat>();
        
        private static void Main(string[] args)
        {
        start:
            Memory.Init(null, null, null);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t", // note: default is two spaces
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false,
            };
            using (StreamWriter csvFile = new StreamWriter(new FileStream("SequenceDump.csv", FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                csvFile.WriteLine($"Type{ls}Type ID{ls}Name{ls}Animation Count{ls}Sequence Count{ls}Sequence ID{ls}Offset{ls}Bytes");
                using (XmlWriter xmlWriter = XmlWriter.Create("SequenceDump.xml", xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("dat");

                    XmlMonsterData(xmlWriter,csvFile);
                    XmlCharacterData(xmlWriter,csvFile);


                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
            }

            Console.Write("Press [Enter] key to continue...  ");
            FF8String sval = Console.ReadLine().Trim((Environment.NewLine + " _").ToCharArray());
            goto start;
        }

        private static void XmlMonsterData(XmlWriter xmlWriter, StreamWriter csvFile)
        {
            xmlWriter.WriteStartElement("monsters");
            for (int i = 0; i <= 200; i++)
            {
                MonsterData.TryAdd(i, Debug_battleDat.Load(i, Debug_battleDat.EntityType.Monster));
                if (MonsterData.TryGetValue(i, out Debug_battleDat _BattleDat) && _BattleDat != null)
                {
                    const string type = "monster";
                    string id = i.ToString();
                    FF8String name = _BattleDat.information.name ?? new FF8String("");
                    string prefix = $"{type}{ls}{id}{ls}{name}";
                    xmlWriter.WriteStartElement(type);
                    xmlWriter.WriteAttributeString("id", id);
                    xmlWriter.WriteAttributeString("name", name);
                    prefix += $"{ls}{XMLAnimations(xmlWriter, _BattleDat)}";
                    XMLSequences(xmlWriter, _BattleDat, csvFile, prefix);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }

        private static string ls => CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        private static void XmlCharacterData(XmlWriter xmlWriter, StreamWriter csvFile)
        {
            xmlWriter.WriteStartElement("characters");
            for (int i = 0; i <= 10; i++)
            {
                Debug_battleDat test = Debug_battleDat.Load(i, Debug_battleDat.EntityType.Character,0);
                if (test != null && CharacterData.TryAdd(i, test))
                {
                }
                if (CharacterData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {

                    const string type = "character";
                    xmlWriter.WriteStartElement(type);
                    string id = i.ToString();
                    xmlWriter.WriteAttributeString("id", id);
                    FF8String name = Memory.Strings.GetName((Characters)i);
                    xmlWriter.WriteAttributeString("name", name);
                    string prefix0 = $"{type}{ls}{id}{ls}";
                    string prefix1 = $"{name}";
                    prefix1 += $"{ls}{XMLAnimations(xmlWriter, _BattleDat)}";
                    XMLSequences(xmlWriter, _BattleDat, csvFile, $"{prefix0}{prefix1}");
                    XmlWeaponData(xmlWriter,i,ref _BattleDat,csvFile, prefix1);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }
        private static void XmlWeaponData(XmlWriter xmlWriter, int character_id, ref Debug_battleDat r, StreamWriter csvFile, string prefix1)
        {
            ConcurrentDictionary<int, Debug_battleDat> WeaponData = new ConcurrentDictionary<int, Debug_battleDat>();
            xmlWriter.WriteStartElement("weapons");
            for (int i = 0; i <= 40; i++)
            {
                Debug_battleDat test;
                if (character_id == 1 || character_id == 9)
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon,i,r);
                else
                    test = Debug_battleDat.Load(character_id, Debug_battleDat.EntityType.Weapon, i);
                if (test != null && WeaponData.TryAdd(i, test))
                {
                }
                if (WeaponData.TryGetValue(i, out Debug_battleDat _BattleDat))
                {

                    const string type = "weapon";
                    string id = i.ToString();
                    xmlWriter.WriteStartElement(type);
                    xmlWriter.WriteAttributeString("id", id);
                    int index = Module_battle_debug.Weapons[(Characters)character_id].FindIndex(v => v == i);
                    var weapondata = Kernel_bin.WeaponsData.FirstOrDefault(v => v.Character == (Characters)character_id &&
                    v.AltID == index);
                    xmlWriter.WriteAttributeString("name", weapondata.Name);

                    string prefix = $"{type}{ls}{id}{ls}{weapondata.Name}/{prefix1}"; //bringing over name from character.
                    //xmlWriter.WriteAttributeString("name", Memory.Strings.GetName((Characters)i));
                    XMLAnimations(xmlWriter, _BattleDat);
                    XMLSequences(xmlWriter, _BattleDat,csvFile,prefix);
                    xmlWriter.WriteEndElement();

                }
            }
            xmlWriter.WriteEndElement();
        }

        private static void XMLSequences(XmlWriter xmlWriter, Debug_battleDat _BattleDat, StreamWriter csvFile, string prefix)
        {
            xmlWriter.WriteStartElement("sequences");
            string count = $"{_BattleDat.Sequences?.Count ?? 0}";
            xmlWriter.WriteAttributeString("count", count);
            if (_BattleDat.Sequences != null)
                foreach (Debug_battleDat.AnimationSequence s in _BattleDat.Sequences)
                {
                    xmlWriter.WriteStartElement("sequence");
                    string id = s.id.ToString();
                    string offset = s.offset.ToString("X");
                    string bytes = s.data.Length.ToString();

                    xmlWriter.WriteAttributeString("id", id);
                    xmlWriter.WriteAttributeString("offset", offset);
                    xmlWriter.WriteAttributeString("bytes", bytes);

                    csvFile?.Write($"{prefix??""}{ls}{count}{ls}{id}{ls}{s.offset}{ls}{bytes}");
                    foreach (byte b in s.data)
                    {
                        xmlWriter.WriteString($"{b.ToString("X2")} ");
                        csvFile?.Write($"{ls}{b}");
                    }
                    csvFile?.Write(Environment.NewLine);
                    xmlWriter.WriteEndElement();
                }
            csvFile?.Flush();
            xmlWriter.WriteEndElement();
        }

        private static string XMLAnimations(XmlWriter xmlWriter, Debug_battleDat _BattleDat)
        {
            string count = $"{_BattleDat.animHeader.animations?.Length ?? 0}";
            xmlWriter.WriteStartElement("animations");
            xmlWriter.WriteAttributeString("count", count);
            xmlWriter.WriteEndElement();
            return count;
        }

        #endregion Methods
    }
}