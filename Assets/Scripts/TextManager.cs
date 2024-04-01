using UnityEngine;
using TMPro;

public class TextManager : MonoBehaviour
{

    public TextMeshProUGUI lat, lon, altitude;

    // Update is called once per frame
    void Update()
    {

        lat.text = "LAT: " + DeviceGPSInfo.latitude.ToString();
        lon.text = "LONG: " + DeviceGPSInfo.longitude.ToString();
        altitude.text = "ALTITUDE: " + DeviceGPSInfo.altitude.ToString();
    }
}
