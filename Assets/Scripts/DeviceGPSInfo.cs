using UnityEngine;
using UnityEngine.Android;
using System.Collections;

public class DeviceGPSInfo : MonoBehaviour
{
    public static float longitude;
    public static float latitude;
    public static float altitude;
    public static DeviceGPSInfo instance;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject); 
    }

    IEnumerator Start()
    {

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        // Check if the user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location not enabled on device");
            yield break;
        }

        // Starts the location service
        Input.location.Start(5f, 10f);

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window
            Debug.Log("Location:\n" + Input.location.lastData.latitude + "\n" + Input.location.lastData.longitude + "\n" + Input.location.lastData.altitude + "\n" + Input.location.lastData.horizontalAccuracy + "\n" + Input.location.lastData.timestamp);  
        }

    }

    private void Update() 
    {
        // Update the GPS location if location service is enabled
        if (Input.location.isEnabledByUser)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            altitude = Input.location.lastData.altitude;
        }
    }

    private void OnDestroy()
    {
        // Stop location services when the script is destroyed
        Input.location.Stop();
    }

}
