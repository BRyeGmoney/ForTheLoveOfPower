using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;

namespace PenisPotato
{
    public class SaveMPData
    {
    }

    [Serializable]
    class SettingsSaveData
    {
    }

    [Serializable]
    public class MPSaveData
    {
        public String playerName { get; set; }
        public float playerLevel { get; set; }
        public Color[] playerColorChoice { get; set; }
    }

    [Serializable]
    class GameSaveData
    {
        public int audioLevel;
    }

    /// <summary>
    /// Class to simplify serialization
    /// </summary>
    public class SerializationHelper
    {

        private static readonly string _saveGameFileFullPath;

        //Sets the full path to the file for serialization
        static SerializationHelper()
        {
            //get the location of the folder where the program's .exe file is running from
            string runningDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //find out where the savegame xml file would be if it exists
            _saveGameFileFullPath = Path.Combine(runningDirectoryPath, "savedata.xml");
        }


        /// <summary>
        /// Method for loading an object from an xml file. The object will be loaded from a file called 'savedata.xml' which is placed next to the game's .EXE file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>If the file exists, the contents of the file as objects. If the file doesn't exist - the default for that type (i.e. Null for objects, false for bools and 0 for numeric values).</returns>
        public static T Load<T>()
        {

            try
            {
                //if the file doesn't exist 
                if (!File.Exists(_saveGameFileFullPath))
                {
                    //just return the default value for that object type
                    return default(T);
                }

                //create a new XmlSerializer for use in deserializing
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                //open the file and deserialize. The 'using' statement makes sure the file is closed again, even if an error occurs.
                using (FileStream stream = File.Open(_saveGameFileFullPath, FileMode.Open, FileAccess.Read))
                {
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading the type '" + typeof(T) + "' from the file '" + _saveGameFileFullPath + "'. The error is: " + ex.ToString(), ex);
            }
        }

        /// <summary>
        /// Method for saving an object to a file. The object will be saved to a file called 'savedata.xml' which is placed next to the game's .EXE file
        /// </summary>
        /// <param name="thingies">The object to save</param>
        public static void Save(object saveData)
        {
            try
            {
                //create a new XmlSerializer for use in serializing
                XmlSerializer serializer = new XmlSerializer(saveData.GetType());

                //create the file (overwrite if it exists) and serialize the object.
                // The 'using' statement makes sure the file is closed again, even if an error occurs.
                using (FileStream stream = File.Open(_saveGameFileFullPath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(stream, saveData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving an object of type '" + saveData.GetType() + "' to the file '" + _saveGameFileFullPath + "'. The error is: " + ex.ToString(), ex);
            }
        }
    }
}
