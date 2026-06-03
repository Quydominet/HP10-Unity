using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

//this script saves and loads all the info we want
public class SaveHandler : MonoBehaviour
{
    string path = "Player.save";

    //data is what is finally saved
    public Dictionary<string, int> data;

    void Awake()
    {
        //WARNING! data.Clear() deletes EVERYTHING
        //data.Clear();
        //SaveData();
    }

    public void LoadData()
    {
        data = SaveHandler.DeserializeData<Dictionary<string, int>>(path);
    }

    public void SaveData() => SaveHandler.SerializeData(data, path);
    public static void SerializeData<T>(T data, string path)
    {
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fs, data);
            Debug.Log("Data written to " + path + " @ " + DateTime.Now.ToShortTimeString());
        }
        catch (SerializationException e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            fs.Close();
        }
    }

    public static T DeserializeData<T>(string path)
    {
        T data = default(T);

        if (File.Exists(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                data = (T)formatter.Deserialize(fs);
                Debug.Log("Data read from " + path);
            }
            catch (SerializationException e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        return data;
    }
}