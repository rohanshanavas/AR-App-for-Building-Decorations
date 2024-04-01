using System;
using System.Collections;
using ARLocation;
using ARLocation.MapboxRoutes;
using TMPro;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class StartLoadingNav : MonoBehaviour
{
    public GameObject mapboxRoute; // Reference to the Mapbox route GameObject
    public GameObject placeAtLocationLoader; // Reference to the place at location loader GameObject
    GameObject signMeshPrefab; // Reference to the sign mesh prefab
    MapboxRoute mapboxScript; // Reference to the MapboxRoute script
    private bool destinationReached = false; // Flag to indicate if the destination has been reached

    // Destination latitude and longitude received from SceneController
    double destinationLat = SceneController.passedBuildingData.lat;
    double destinationLong = SceneController.passedBuildingData.lon;

    void Awake()
    {
        // Get the MapboxRoute script component
        mapboxScript = mapboxRoute.GetComponent<MapboxRoute>();
    }

    void Start()
    {
        // Create a new location object for the destination
        Location destination = new Location(destinationLat, destinationLong);

        // Create a route waypoint for the destination
        RouteWaypoint routeWaypoint = new RouteWaypoint
        {
            Type = RouteWaypointType.Location,
            Location = destination
        };

        // Check if the MapboxRoute script exists
        if (mapboxScript != null)
        {
            // Set the destination for the Mapbox route
            mapboxScript.Settings.RouteSettings.To = routeWaypoint;
            mapboxRoute.SetActive(true); // Activate the Mapbox route visualization
        }
        else
        {
            Debug.LogError("Mapbox Script not found!");
        }
    }

    void Update()
    {
        if (!destinationReached)
        {
            // Calculate the distance between current location and destination
            double distance = Math.Round(GeoCodeCalc.CalcDistance(DeviceGPSInfo.latitude, DeviceGPSInfo.longitude, 
            destinationLat, destinationLong, GeoCodeCalcMeasurement.Metre), 2);

            // Check if the distance to the destination is less than 60 meters
            if (distance < 60)
            {
                // Deactivate the Mapbox route visualization and clear the route
                mapboxRoute.SetActive(false);
                mapboxScript.clearRoute();

                destinationReached = true; // Set the destinationReached flag to true

                // Calculate position for the sign mesh prefab
                Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 10;

                // Instantiate the sign mesh prefab at the calculated position
                signMeshPrefab = (GameObject)Instantiate(Resources.Load("Prefab/SignMeshPrefab"), position, Quaternion.identity);
                signMeshPrefab.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                // Get the text component of the sign mesh prefab and set the text
                TextMeshPro textComponent = signMeshPrefab.GetComponentInChildren<TextMeshPro>();
                textComponent.text = "You have reached your destination";

                Destroy(signMeshPrefab, 5f); // Destroy the sign mesh prefab after 5 seconds

                StartCoroutine(CheckPrefabDestroyed()); // Start coroutine to check if the sign mesh prefab is destroyed
            }
        }
    }

    // Coroutine to check if the sign mesh prefab is destroyed
    IEnumerator CheckPrefabDestroyed()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Wait for 1 second

            // Check if the sign mesh prefab is destroyed
            if (signMeshPrefab == null)
            {
                placeAtLocationLoader.SetActive(true); // Activate the place at location loader
                break; // Exit the loop
            }
        }
    }
}
