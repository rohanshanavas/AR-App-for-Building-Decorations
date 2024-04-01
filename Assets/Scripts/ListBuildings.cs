using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase.Firestore;
using System.Text;
using Firebase.Extensions;
using Firebase;

public class ListBuildings : MonoBehaviour
{
    private string prefabName = "listPrefab"; // Name of the prefab used for displaying building information

    public List<Building> buildings = new List<Building>(); // List to store building data

    FirebaseFirestore firestore; // Reference to the Firestore database

    GameObject contentHolder; // Reference to the UI object holding the list of buildings

    void Start()
    {
        // Find the content holder object in the scene
        contentHolder = GameObject.FindGameObjectWithTag("BuildingList");

        // Check Firebase dependencies and initialize Firestore
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase is ready to use
                firestore = FirebaseFirestore.DefaultInstance;
                FetchFromDatabase(); // Fetch building data from Firestore
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    // Fetch building data from Firestore
    void FetchFromDatabase()
    {
        CollectionReference buildingsRef = firestore.Collection("Buildings");
        buildingsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (task.Exception != null)
            {
                Debug.LogError($"Failed to fetch building data: {task.Exception}");
                return;
            }

            // Iterate through documents to retrieve place IDs
            foreach (DocumentSnapshot documentSnapshot in task.Result.Documents)
            {
                if (documentSnapshot.ContainsField("placeID"))
                {
                    string placeID = documentSnapshot.GetValue<string>("placeID");
                    stringBuilder.Append($"way({placeID});");
                }
                else
                {
                    Debug.LogWarning($"Document {documentSnapshot.Id} does not contain 'placeID' field.");
                }
            }

            string buildingIDs = stringBuilder.ToString();

            if (!string.IsNullOrEmpty(buildingIDs))
            {
                StartCoroutine(makeURLRequest(buildingIDs)); // Make a web request to retrieve building information
            }
        });
    }

    // Make a web request to retrieve building information from Overpass API
    IEnumerator makeURLRequest(string joinedString)
    {
        string url = $"https://overpass-api.de/api/interpreter?data=[out:json];({joinedString});out;";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest(); // Send the web request

            StartCoroutine(createList(webRequest.downloadHandler.text)); // Create a list of buildings from the received JSON data
        }
    }

    // Create a list of buildings from JSON data
    IEnumerator createList(string jsonString)
    {
        if (jsonString != null)
        {
            Root buildingsInfo = JsonConvert.DeserializeObject<Root>(jsonString);

            if (buildingsInfo.elements.Count > 0)
            {
                // Iterate through building elements
                for (int i = 0; i < buildingsInfo.elements.Count; i++)
                {
                    List<Node> nodes = new List<Node>();

                    // Calculate building height based on building levels
                    int buildingHeight = int.Parse(buildingsInfo.elements[i].tags.buildinglevels) * 3;

                    // Create a string builder to store node information for building
                    StringBuilder stringBuilder = new StringBuilder();

                    // Iterate through node IDs and append to the string builder
                    for (int j = 0; j < buildingsInfo.elements[i].nodes.Count; j++)
                    {
                        stringBuilder.Append($"node({buildingsInfo.elements[i].nodes[j]});");
                    }

                    // Retrieve node information for building
                    yield return StartCoroutine(FindNodeInformation(stringBuilder.ToString(), nodes));

                    // Calculate average latitude and longitude for building centroid
                    double averageLatitude = nodes.Select(node => node.lat).Average();
                    double averageLongitude = nodes.Select(node => node.lon).Average();

                    // Calculate object scale based on bounding box dimensions
                    double objectScale = CalculateDistances(nodes);

                    // Instantiate prefab for displaying building information
                    GameObject thePrefab = (GameObject)Instantiate(Resources.Load("Prefab/" + prefabName));
                    thePrefab.transform.SetParent(contentHolder.transform, false);

                    // Set building name and distance text
                    Text[] texts = thePrefab.GetComponentsInChildren<Text>();
                    texts[0].text = buildingsInfo.elements[i].tags.name;
                    double distance = Math.Round(GeoCodeCalc.CalcDistance(DeviceGPSInfo.latitude, DeviceGPSInfo.longitude, 
                    averageLatitude, averageLongitude, GeoCodeCalcMeasurement.Metre), 2);
                    texts[1].text = "Distance : " + distance.ToString() + "m";

                    // Set button listener to navigate to AR view
                    Button button = thePrefab.GetComponentInChildren<Button>();
                    button.name = i.ToString();
                    Building buildingData = new Building(buildingsInfo.elements[i].id.ToString(), buildingsInfo.elements[i].tags.name,
                    distance, averageLatitude, averageLongitude, objectScale, buildingHeight);
                    AddListener(button, buildingData);

                    thePrefab.transform.localScale = new Vector3(1, 1, 1);

                    buildings.Add(buildingData); // Add building data to the list
                }

                orderbyDistance(buildings); // Order buildings by distance
            }
        }
    }

    // Retrieve node information for building
    IEnumerator FindNodeInformation(string buildingNodes, List<Node> nodes)
    {
        string node_url = $"https://overpass-api.de/api/interpreter?data=[out:json];({buildingNodes});out;";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(node_url))
        {
            yield return webRequest.SendWebRequest(); // Send the web request

            // Deserialize JSON data to retrieve node information
            Root nodeInfo = JsonConvert.DeserializeObject<Root>(webRequest.downloadHandler.text);

            // Add node information to the list
            for (int i = 0; i < nodeInfo.elements.Count; i++)
            {
                nodes.Add(new Node(nodeInfo.elements[i].id.ToString(), nodeInfo.elements[i].lat, nodeInfo.elements[i].lon));
            }
        }
    }

    // Calculate object scale based on bounding box dimensions
    double CalculateDistances(List<Node> nodesInBoundingBox)
    {
        double length = 0f;
        double width = 0f;

        for (int i = 0; i < nodesInBoundingBox.Count; i++)
        {
            Node current = nodesInBoundingBox[i];
            Node next;

            // Get next node in the list
            if (i == nodesInBoundingBox.Count - 1)
            {
                next = nodesInBoundingBox[0];
            }
            else
            {
                next = nodesInBoundingBox[i + 1];
            }

            // Calculate distance between current and next nodes
            double distance = Math.Round(GeoCodeCalc.CalcDistance(current.lat, current.lon, next.lat, 
            next.lon, GeoCodeCalcMeasurement.Metre), 2);

            // Update length and width based on distance
            if (i % 2 == 0 && distance > length)
            {
                length = distance;
            }
            else if (i % 2 != 0 && distance > width)
            {
                width = distance;
            }
        }

        return (length > width) ? length : width;
    }

    // Order buildings by distance
    void orderbyDistance(List<Building> itemsPassed)
    {
        itemsPassed = itemsPassed.OrderBy((a) => a.distance).ToList();
        RePopulate(itemsPassed);
    }

    // Repopulate building list
    void RePopulate(List<Building> itemsPassed)
    {
        foreach (Transform child in contentHolder.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < itemsPassed.Count; i++)
        {
            GameObject thePrefab = (GameObject)Instantiate(Resources.Load("Prefab/" + prefabName));
            thePrefab.transform.SetParent(contentHolder.transform, false);

            Text[] texts = thePrefab.GetComponentsInChildren<Text>();
            texts[0].text = itemsPassed[i].buildingName;
            texts[1].text = "Distance : " + itemsPassed[i].distance.ToString() + "m";

            Button button = thePrefab.GetComponentInChildren<Button>();
            button.name = i.ToString();

            thePrefab.transform.localScale = new Vector3(1, 1, 1);

            AddListener(button, itemsPassed[i]);
        }
    }

    // Add listener to button for navigating to AR view
    void AddListener(Button b, Building building)
    {
        b.onClick.AddListener(() => {
            SceneController.passedBuildingData = building;
            SceneController.changeScene("ARView");
        });
    }
}

// Class to store building data
public class Building
{
    public string buildingID;
    public string buildingName;
    public double distance;
    public double lat;
    public double lon;
    public double objectScale;
    public int buildingHeight;

    public Building() { }

    public Building(string buildingID, string buildingName, double distance, double lat, double lon, double objectScale, 
    int buildingHeight)
    {
        this.buildingID = buildingID;
        this.buildingName = buildingName;
        this.distance = distance;
        this.lat = lat;
        this.lon = lon;
        this.objectScale = objectScale;
        this.buildingHeight = buildingHeight;
    }
}

// Class to store node data
public class Node
{
    public string nodeID;
    public double lat;
    public double lon;

    public Node(string nodeID, double lat, double lon)
    {
        this.nodeID = nodeID;
        this.lat = lat;
        this.lon = lon;
    }
}

// Class to deserialize JSON data
public class Root
{
    public double version { get; set; }
    public string generator { get; set; }
    public Osm3s osm3s { get; set; }
    public List<Element> elements { get; set; }
}

// Class representing OSM3s object
public class Osm3s
{
    public DateTime timestamp_osm_base { get; set; }
    public string copyright { get; set; }
}

// Class representing building elements
public class Element
{
    public string type { get; set; }
    public long id { get; set; }
    public List<object> nodes { get; set; }
    public Tags tags { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
}

// Class representing building tags
public class Tags
{
    [JsonProperty("addr:housename")]
    public string addrhousename { get; set; }

    [JsonProperty("addr:place")]
    public string addrplace { get; set; }
    public string building { get; set; }

    [JsonProperty("building:levels")]
    public string buildinglevels { get; set; }
    public string description { get; set; }
    public string name { get; set; }
    public string wheelchair { get; set; }
}
