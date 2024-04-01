using ARLocation;
using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    // Reference to the place at location GameObject
    public GameObject placeAtLocation;

    // Latitude and longitude coordinates of the object obtained from SceneController
    readonly double objectLat = SceneController.passedBuildingData.lat;
    readonly double objectLong = SceneController.passedBuildingData.lon;

    // Height of the object obtained from SceneController
    readonly double height = SceneController.passedBuildingData.buildingHeight;

    // Scale of the object obtained from SceneController
    readonly float scale = (float)SceneController.passedBuildingData.objectScale;

    void Start()
    {
        // Instantiate the animated object prefab
        GameObject animatedObject = Instantiate(Resources.Load<GameObject>("Prefab/Ghost"));

        // Set the scale of the animated object
        animatedObject.transform.localScale = new Vector3(scale, scale, scale);

        // Set the parent of the animated object to the place at location GameObject
        animatedObject.transform.SetParent(placeAtLocation.transform, false);

        // Get the PlaceAtLocation component attached to the place at location GameObject
        PlaceAtLocation placeAtLocationScript = placeAtLocation.GetComponent<PlaceAtLocation>();

        if (placeAtLocationScript != null)
        {
            // Create a new Location object with the latitude, longitude, and altitude of the object
            var loc = new Location()
            {
                Latitude = objectLat,
                Longitude = objectLong,
                Altitude = height,
                AltitudeMode = AltitudeMode.GroundRelative
            };

            // Set the placement options for the PlaceAtLocation script
            var opts = new PlaceAtLocation.PlaceAtOptions()
            {
                HideObjectUntilItIsPlaced = true,
                MaxNumberOfLocationUpdates = 2,
                MovementSmoothing = 0.1f,
                UseMovingAverage = false
            };

            // Assign the placement options to the PlaceAtLocation script
            placeAtLocationScript.PlacementOptions = opts;

            // Set the location input type and location for the PlaceAtLocation script
            placeAtLocationScript.LocationOptions.LocationInput.LocationInputType = LocationPropertyData.LocationPropertyType.Location;

            placeAtLocationScript.LocationOptions.LocationInput.Location = loc;

            // Activate the place at location GameObject
            placeAtLocation.SetActive(true);
        }
    }
}
