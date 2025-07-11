# AR Geospatial: Augmented Reality (AR) App

## Overview
AR Geospatial is an augmented reality (AR) application developed to enhance outdoor experiences through the integration of AR technology with location-based services. This project aims to provide users with a unique and entertaining way to engage with their environment during festive seasons, such as Halloween, by overlaying digital decorations onto real-world scenes.

Additionally, this project involves seamlessly integrating AR experiences with real-world outdoor environments and accurately placing digital content relative to specific buildings. This includes retrieving building information from external APIs, calculating distances and directions using GPS data, and dynamically scaling and placing AR objects based on building dimensions.

## Installation Instructions

1. **Android Application Installation:**
   - Copy the .apk file located in the folder "Android Application APK" to your Android device.
   - Open the .apk file on your device and follow the prompts to install the application.
   - Once installed, you can find the Android application in your app drawer named "AR Geospatial".

2. **Code Access and Project Setup:**
   - Navigate to the folder "Assets/Scripts" to access all the C# scripts used in the project.
   - To work on the project:
     - Download the Unity Application from [Unity's official website](https://unity.com/download).
     - Open "Unity Hub" and install the recommended Long Term Support (LTS) version from the "Installs" section.
     - After installing Unity, click "Add" and choose the folder "attachments".
     - Ignore any errors that may appear and proceed to open the Unity Engine window.
     - Go to File -> Build Settings, select the "Android" tab, and click "Switch Platform".
     - Check the "Development Build" box in the "Android" tab.
     - Download the Firebase SDK for Unity from [Firebase's official website](https://firebase.google.com/download/unity) and unzip it.
     - Go back to Unity and navigate to Assets -> Import Package -> Custom Package.
     - Locate the unzipped Firebase SDK folder.
     - Choose the FirebaseFirestore.unitypackage and click "Open".
     - Click on "Import" in the pop-up window that appears.
     - The project is now ready for exploration or modification.

## App Screenashots

<img src="Application%20Screenshots/1%20-%20Main%20Scene.jpg" alt="Main Scene" width="400">

<img src="Application%20Screenshots/2-%20Navigation%20to%20Location.jpg" alt="AR Navigation" width="400">

<img src="Application%20Screenshots/3%20-%20Destination%20Reached%20Prompt.jpg" alt="Prompt for Destination Reached" width="400">

<img src="Application%20Screenshots/4%20-%203D%20Object%20Displayed.jpg" alt="3D Object Displayed on Building" width="400">

## Additional Notes

- Ensure that your Android device supports ARCore for optimal performance of the AR features.
