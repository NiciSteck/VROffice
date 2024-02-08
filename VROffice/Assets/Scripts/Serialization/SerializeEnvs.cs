using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

public class SerializeEnvs : MonoBehaviour
{
    private string dataDirPath = "";
    private string dataFileName = "EnvsData.json";

    public GameObject prefabContainer;
    public bool save;
    
    // Start is called before the first frame update
    void Start()
    {
        //dataDirPath = Application.persistentDataPath; //the clean place to store data, but not included in git repo
        loadEnvs();
    }

    // Update is called once per frame
    void Update()
    {
        if (save)
        {
            saveEnvs();
            save = false;
        }
    }

    private void OnApplicationQuit()
    {
        saveEnvs();
    }

    public void loadEnvs()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        if (File.Exists((fullPath)))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath,FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                EnvironmentsData data = JsonUtility.FromJson<EnvironmentsData>(dataToLoad);
                foreach (Transform child in transform) {
                    //get rid of the inspector envs to avoid duplicated because they are also serialized
                    Destroy(child.gameObject);
                }

                foreach (EnvData envData in data.envs)
                {
                    GameObject env = new GameObject(envData.name);
                    env.transform.SetParent(transform);
                    env.SetActive(false);
                    
                    EnvModel envModel = env.AddComponent<EnvModel>();
                    
                    foreach (ContainerData contData in envData.containers)
                    {
                        GameObject container = Instantiate(prefabContainer, env.transform);
                        container.name = contData.name;
                        container.transform.position = contData.position;
                        container.transform.rotation = contData.rotation;
                        container.transform.GetChild(0).localScale = contData.scale;
                        container.GetComponent<ContainerModel>().init(contData.id,contData.scale);
                        
                        envModel.containers.Add(container.GetComponent<ContainerModel>());
                    }
                    centerOnChildren(env.transform);
                    env.transform.position = Vector3.zero;
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        
    }
    
    public void centerOnChildren(Transform parent)
    {
        List<Transform> children = parent.Cast<Transform>().ToList();
        Vector3 center = Vector3.zero;
        foreach (Transform child in children)
        {
            center += child.position;
            child.parent = null;
        }
        parent.position = center/children.Count;
        foreach (Transform child in children)
        {
            child.parent = parent;
        }
    }

    public void saveEnvs()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            
            EnvironmentsData data = new EnvironmentsData
            {
                envs = new List<EnvData>()
            };
            
            foreach (Transform env in transform)
            {
                EnvData envData = new EnvData
                {
                    name = env.name,
                    containers = new List<ContainerData>()
                };

                EnvModel envModel = env.GetComponent<EnvModel>();
                foreach (ContainerModel container in envModel.containers)
                {
                    Transform parentContainer = container.transform;
                    Transform childContainer = container.transform.GetChild(0);
                    
                    ContainerData contData = new ContainerData
                    {
                        name = parentContainer.name,
                        id = container.ID,
                        position = parentContainer.position,
                        rotation = parentContainer.rotation,
                        scale = childContainer.localScale
                    };

                    envData.containers.Add(contData);
                }
                
                data.envs.Add(envData);
            }
            
            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
    
    [Serializable]
    private struct EnvironmentsData
    {
        public List<EnvData> envs;
    }
    
    [Serializable]
    private struct EnvData
    {
        public string name;
        public List<ContainerData> containers;
    }
    
    [Serializable]
    private struct ContainerData
    {
        public string name;
        public int id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
